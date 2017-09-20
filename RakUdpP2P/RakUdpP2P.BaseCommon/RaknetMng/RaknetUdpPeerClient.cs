﻿using RakNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RakUdpP2P.BaseCommon.RaknetMng
{
	public class RaknetUdpPeerClient : RaknetBase
	{
		private NatPunchthroughClient natPunchthroughClient = null;
		//private NatTypeDetectionClient natTypeDetectionClient = null;
		private UDPProxyClient udpProxyClient = null;

		private RaknetAddress _natServerAddress = null;
		private RaknetAddress _coordinatorAddress = null;
		private static RaknetAddress _proxyServerAddress = null; //临时代理服务器地址
		private RaknetAddress _peerServerAddress = null;
		private ulong _udpPeerServerGuid = 0;

		public RaknetUdpPeerClient()
		{
			natPunchthroughClient = new NatPunchthroughClient();
			//natTypeDetectionClient = new NatTypeDetectionClient();
			udpProxyClient = new UDPProxyClient();
		}

		public RaknetUdpPeerClient Start(RaknetAddress localAddress = null, ushort maxConnCount = ushort.MaxValue)
		{
			rakPeer.AttachPlugin(natPunchthroughClient);
			//rakPeer.AttachPlugin(natTypeDetectionClient);
			rakPeer.AttachPlugin(udpProxyClient);
			SocketDescriptor socketDescriptor = new SocketDescriptor();
			if (localAddress != null && !string.IsNullOrWhiteSpace(localAddress.Address) && localAddress.Port > 0)
			{
				socketDescriptor.hostAddress = localAddress.Address;
				socketDescriptor.port = localAddress.Port;
			}
			var startResult = rakPeer.Startup(maxConnCount, socketDescriptor, 1);
			if (startResult == StartupResult.SOCKET_PORT_ALREADY_IN_USE)
			{
				RaknetExtension.WriteWarning(string.Format(@"{0}端口被占用", socketDescriptor.port));
			}
			return this;
		}

		public bool Connect(RaknetAddress natServerAddress, RaknetAddress coordinatorAddress, RaknetAddress peerServerAddress, ulong udpPeerServerGuid)
		{
			_natServerAddress = natServerAddress;
			_coordinatorAddress = coordinatorAddress;
			_peerServerAddress = peerServerAddress;
			_udpPeerServerGuid = udpPeerServerGuid;
			//var connectResult = rakPeer.Connect(_natServerAddress.Address, _natServerAddress.Port, RaknetConfig.natServerPwd, RaknetConfig.natServerPwd.Length);
			//穿透失败后转代理，但要先连接协调器（在此测试，直接连接代理）
			var connectResult = rakPeer.Connect(_coordinatorAddress.Address, _coordinatorAddress.Port, "", 0);
			if (connectResult == ConnectionAttemptResult.CONNECTION_ATTEMPT_STARTED)
			{
				//natTypeDetectionClient.DetectNATType(new SystemAddress(_natServerAddress.Address, _natServerAddress.Port));
				OnConnectionRequestAccepted += RaknetUdpPeerClient_OnConnectionRequestAccepted;
				OnNatPunchthroughSucceeded += RaknetUdpPeerClient_OnNatPunchthroughSucceeded;
				OnNatPunchthroughFailed += RaknetUdpPeerClient_OnNatPunchthroughFailed;
				ReceiveThreadStart();
				return true;
			}
			return false;
		}



		private void RaknetUdpPeerClient_OnConnectionRequestAccepted(string address, ushort port)
		{
			Console.WriteLine(string.Format("请求连接成功后，获得的IPAddress：{0}：{1}", address, port));
			if (address == _natServerAddress.Address && port == _natServerAddress.Port)
			{
				//OpenNAT
				RakNetGUID peerServerRakNetGUID = new RakNetGUID(_udpPeerServerGuid);
				RakNetGUID myGuid = rakPeer.GetMyGUID();
				if (myGuid.g != _udpPeerServerGuid)
				{
					natPunchthroughClient.OpenNAT(peerServerRakNetGUID, new SystemAddress(address, port));
				}
			}
			else if (address == _coordinatorAddress.Address && port == _coordinatorAddress.Port)
			{
				//协调器连接成功后，请求转发消息
				udpProxyClient.SetResultHandler(new MyUDPProxyClientResultHandler());
				SystemAddress coordinatorAddress = new SystemAddress();
				coordinatorAddress.SetBinaryAddress(_coordinatorAddress.Address);
				coordinatorAddress.SetPortHostOrder(_coordinatorAddress.Port);
				//请求代理转发消息
				udpProxyClient.RequestForwarding(coordinatorAddress, rakPeer.GetMyBoundAddress(), new SystemAddress(_peerServerAddress.Address, _peerServerAddress.Port), 7000);
			}
			else if (_proxyServerAddress != null && address == _proxyServerAddress.Address && port == _proxyServerAddress.Port)
			{
				//代理连接成功，对_peerServerAddress重新赋值，该值实际是proxy的一个临时地址，通过该值直接发送消息给peerServer，但通过代理的方式要持续发送消息给proxy才能保持连接，如果一段时间内不发送消息，则连接主动断开
				_peerServerAddress = new RaknetAddress(address, port);
				//开启线程，持续向proxy发送消息，只能发送一个字节，并且消息类型必须为 DefaultMessageIDTypes.ID_USER_PACKET_ENUM
				ThreadPool.QueueUserWorkItem(obj =>
				{

				});
			}
			else
			{
				//通过NAT与peerServer连接成功后，立即断开与natServer的连接
				rakPeer.CloseConnection(new AddressOrGUID(new SystemAddress(_natServerAddress.Address, _natServerAddress.Port)), true);
			}
		}

		private void RaknetUdpPeerClient_OnNatPunchthroughSucceeded(string arg1, ushort arg2)
		{
			//穿透成功后连接peerServer
			rakPeer.Connect(_peerServerAddress.Address, _peerServerAddress.Port, "", 0);
		}
		private void RaknetUdpPeerClient_OnNatPunchthroughFailed(string address, ushort port)
		{
			//穿透失败后转代理，但要先连接协调器
			rakPeer.Connect(_coordinatorAddress.Address, _coordinatorAddress.Port, "", 0);
		}

		private class MyUDPProxyClientResultHandler : UDPProxyClientResultHandler
		{
			public override void OnForwardingSuccess(string proxyIPAddress, ushort proxyPort, SystemAddress proxyCoordinator, SystemAddress sourceAddress, SystemAddress targetAddress, RakNetGUID targetGuid, UDPProxyClient proxyClientPlugin)
			{
				Console.WriteLine("▲▲▲OnForwardingSuccess");
				//请求转发成功后，连接代理服务器
				_proxyServerAddress = new RaknetAddress(proxyIPAddress, proxyPort);
				var peer = proxyClientPlugin.GetRakPeerInterface();
				var systemAddress = peer.GetMyBoundAddress();
				peer.Connect(proxyIPAddress, proxyPort, "", 0);
			}

			public override void OnForwardingInProgress(string proxyIPAddress, ushort proxyPort, SystemAddress proxyCoordinator, SystemAddress sourceAddress, SystemAddress targetAddress, RakNetGUID targetGuid, UDPProxyClient proxyClientPlugin)
			{
				Console.WriteLine("▲▲▲OnForwardingInProgress");
			}
			public override void OnAllServersBusy(SystemAddress proxyCoordinator, SystemAddress sourceAddress, SystemAddress targetAddress, RakNetGUID targetGuid, UDPProxyClient proxyClientPlugin)
			{
				Console.WriteLine("▲▲▲OnAllServersBusy");
			}
			public override void OnForwardingNotification(string proxyIPAddress, ushort proxyPort, SystemAddress proxyCoordinator, SystemAddress sourceAddress, SystemAddress targetAddress, RakNetGUID targetGuid, UDPProxyClient proxyClientPlugin)
			{
				Console.WriteLine("▲▲▲OnForwardingNotification");
			}
			public override void OnNoServersOnline(SystemAddress proxyCoordinator, SystemAddress sourceAddress, SystemAddress targetAddress, RakNetGUID targetGuid, UDPProxyClient proxyClientPlugin)
			{
				Console.WriteLine("▲▲▲OnNoServersOnline");
			}
			public override void OnRecipientNotConnected(SystemAddress proxyCoordinator, SystemAddress sourceAddress, SystemAddress targetAddress, RakNetGUID targetGuid, UDPProxyClient proxyClientPlugin)
			{
				Console.WriteLine("▲▲▲OnRecipientNotConnected");
			}
		}
	}
}
