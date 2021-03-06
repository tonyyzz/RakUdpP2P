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
		/// <summary>
		/// 连接上PeerServer的事件通知
		/// </summary>
		public event Action<string, ushort, RaknetUdpPeerClient> OnConnect;
		/// <summary>
		/// 连接失败的事件通知
		/// </summary>
		public event Action<string, ushort, RaknetUdpPeerClient> OnConnectFailed;
		/// <summary>
		/// 收到PeerServer消息的事件通知
		/// </summary>
		public event Action<string, ushort, byte[], RaknetUdpPeerClient> OnReceive;
		/// <summary>
		/// 与PeerServer断开连接的事件通知
		/// </summary>
		public event Action<string, ushort, RaknetUdpPeerClient> OnDisConnect;


		private NatPunchthroughClient natPunchthroughClient = null;//（内部用）
		private UDPProxyClient udpProxyClient = null;//（内部用）

		private RaknetIPAddress _peerServerAddress = null;
		private ulong _udpPeerServerGuid = 0;
		private bool _isConnectPeerServer = false; //是否连接上peerServer

		private RaknetIPAddress _natServerAddress = null;//（内部用）
		private static RaknetIPAddress _coordinatorAddress = null;//（内部用）
		private static RaknetIPAddress _proxyServerAddress = null; //（内部用）临时代理服务器地址

		private bool isProxyMsgSending = false; //（内部用）启动代理转发消息时，是否持续发送消息来保持连接

		public RaknetUdpPeerClient()
		{
			natPunchthroughClient = new NatPunchthroughClient();
			udpProxyClient = new UDPProxyClient();
			OnConnect += RaknetUdpPeerClient_OnConnect;
			OnConnectFailed += RaknetUdpPeerClient_OnConnectFailed;
			OnReceive += RaknetUdpPeerClient_OnReceive;
			OnDisConnect += RaknetUdpPeerClient_OnDisConnect;
		}

		private void RaknetUdpPeerClient_OnConnectFailed(string address, ushort port, RaknetUdpPeerClient raknetUdpPeerClient) { }
		private void RaknetUdpPeerClient_OnDisConnect(string address, ushort port, RaknetUdpPeerClient raknetUdpPeerClient) { }
		private void RaknetUdpPeerClient_OnReceive(string address, ushort port, byte[] bytes, RaknetUdpPeerClient raknetUdpPeerClient) { }
		private void RaknetUdpPeerClient_OnConnect(string address, ushort port, RaknetUdpPeerClient raknetUdpPeerClient) { }

		public RaknetUdpPeerClient Start(RaknetIPAddress localAddress = null, ushort maxConnCount = ushort.MaxValue)
		{
			rakPeer.AttachPlugin(natPunchthroughClient);
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

		public bool Connect(RaknetIPAddress natServerAddress, RaknetIPAddress coordinatorAddress, RaknetIPAddress peerServerAddress, ulong udpPeerServerGuid)
		{
			_natServerAddress = natServerAddress;
			_coordinatorAddress = coordinatorAddress;
			_peerServerAddress = peerServerAddress;
			_udpPeerServerGuid = udpPeerServerGuid;
			OnConnectionRequestAccepted += RaknetUdpPeerClient_OnConnectionRequestAccepted;
			OnNatPunchthroughSucceeded += RaknetUdpPeerClient_OnNatPunchthroughSucceeded;
			OnNatPunchthroughFailed += RaknetUdpPeerClient_OnNatPunchthroughFailed;
			OnDisconnectionNotification += RaknetUdpPeerClient_OnDisconnectionNotification;
			OnConnectionLost += RaknetUdpPeerClient_OnConnectionLost;
			OnRaknetReceive += RaknetUdpPeerClient_OnRaknetReceive;
			OnConnectionAttemptFailed += RaknetUdpPeerClient_OnConnectionAttemptFailed;
			OnNoFreeIncomingConnections += RaknetUdpPeerClient_OnNoFreeIncomingConnections;
			ReceiveThreadStart();
			////启动NATPunchthrough连接
			var connectResult = rakPeer.Connect(_natServerAddress.Address, _natServerAddress.Port, RaknetConfig.natServerPwd, RaknetConfig.natServerPwd.Length);
			if (connectResult == ConnectionAttemptResult.CONNECTION_ATTEMPT_STARTED)
			{
				return true;
			}
			isThreadRunning = false;
			return false;
		}

		private void RaknetUdpPeerClient_OnNoFreeIncomingConnections(string address, ushort port)
		{
			//连接失败
			isThreadRunning = false;
			isProxyMsgSending = false;
			_isConnectPeerServer = false;
			OnConnectFailed(address, port, this);
		}

		private void RaknetUdpPeerClient_OnConnectionAttemptFailed(string address, ushort port)
		{
			if (address == _natServerAddress.Address && port == _natServerAddress.Port)
			{
				//如果NatServer连接失败，走Proxy，连接coordinator
				rakPeer.Connect(_coordinatorAddress.Address, _coordinatorAddress.Port, "", 0);
			}
			else if (address == _coordinatorAddress.Address && port == _coordinatorAddress.Port)
			{
				//连接失败
				isThreadRunning = false;
				isProxyMsgSending = false;
				_isConnectPeerServer = false;
				OnConnectFailed(address, port, this);
			}
			else
			{
				//连接失败
				isThreadRunning = false;
				isProxyMsgSending = false;
				_isConnectPeerServer = false;
				OnConnectFailed(address, port, this);
			}

		}

		private void RaknetUdpPeerClient_OnRaknetReceive(string address, ushort port, byte[] bytes)
		{
			OnReceive(address, port, bytes, this);
		}

		/// <summary>
		/// 获取与PeerServer的连接状态
		/// </summary>
		/// <returns></returns>
		public bool GetConnectPeerServerState()
		{
			return _isConnectPeerServer;
		}

		/// <summary>
		/// 获取连接的PeerServer的连接地址（注意：由于代理的原因，该方法只能在连接成功后调用才会得到正确的IPAddress）
		/// </summary>
		/// <returns></returns>
		public RaknetIPAddress GetPeerServerAddress()
		{
			return _peerServerAddress;
		}

		public ulong GetPeerServerGuid()
		{
			return _udpPeerServerGuid;
		}

		public RaknetIPAddress GetMyIpAddress()
		{
			return GetMyAddress();
		}

		/// <summary>
		/// 给PeerServer发送消息
		/// </summary>
		public void Send(byte[] bytes)
		{
			bytes = new byte[] { (byte)DefaultMessageIDTypes.ID_USER_PACKET_ENUM }.Concat(bytes).ToArray();
			rakPeer.Send(bytes, bytes.Length,
				PacketPriority.HIGH_PRIORITY,
				PacketReliability.RELIABLE_ORDERED,
				(char)0,
				new AddressOrGUID(new SystemAddress(_peerServerAddress.Address, _peerServerAddress.Port)),
				false);
		}

		private void RaknetUdpPeerClient_OnDisconnectionNotification(string address, ushort port)
		{
			if (address == _natServerAddress.Address && port == _natServerAddress.Port)
			{

			}
			else if (address == _coordinatorAddress.Address && port == _coordinatorAddress.Port)
			{

			}
			else
			{
				isProxyMsgSending = false;//（内部用）
				_isConnectPeerServer = false;
				OnDisConnect(address, port, this);
			}
		}

		private void RaknetUdpPeerClient_OnConnectionLost(string address, ushort port)
		{
			if (address == _natServerAddress.Address && port == _natServerAddress.Port)
			{
				//若果失去NatServer的连接，则转向proxy，连接coordinator
				rakPeer.Connect(_coordinatorAddress.Address, _coordinatorAddress.Port, "", 0);
			}
			else if (address == _coordinatorAddress.Address && port == _coordinatorAddress.Port)
			{
				isProxyMsgSending = false;//（内部用）
				_isConnectPeerServer = false;
				OnDisConnect(address, port, this);
			}
			else
			{
				isProxyMsgSending = false;//（内部用）
				_isConnectPeerServer = false;
				OnDisConnect(address, port, this);
			}
		}

		private void RaknetUdpPeerClient_OnConnectionRequestAccepted(string address, ushort port)
		{
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
				udpProxyClient.SetResultHandler(new MyUDPProxyClientResultHandler());
				SystemAddress coordinatorAddress = new SystemAddress();
				coordinatorAddress.SetBinaryAddress(_coordinatorAddress.Address);
				coordinatorAddress.SetPortHostOrder(_coordinatorAddress.Port);
				udpProxyClient.RequestForwarding(coordinatorAddress, rakPeer.GetMyBoundAddress(), new SystemAddress(_peerServerAddress.Address, _peerServerAddress.Port), 7000);
			}
			else if (_proxyServerAddress != null && address == _proxyServerAddress.Address && port == _proxyServerAddress.Port)
			{
				_isConnectPeerServer = true;
				new Thread(n =>
				{
					OnConnect(address, port, this);
				})
				{ IsBackground = true, Priority = ThreadPriority.Highest }.Start();
				//代理连接成功，对_peerServerAddress重新赋值，该值实际是proxy的一个临时地址，通过该值直接发送消息给peerServer，但通过代理的方式要持续发送消息给proxy才能保持连接，如果一段时间内不发送消息，则连接主动断开
				_peerServerAddress = new RaknetIPAddress(address, port);
				//开启线程，持续向proxy发送消息，只能发送一个字节，并且消息类型必须为 DefaultMessageIDTypes.ID_USER_PACKET_ENUM
				ThreadPool.QueueUserWorkItem(obj =>
				{
					isProxyMsgSending = true;
					while (true)
					{
						if (!isProxyMsgSending)
						{
							break;
						}
						RaknetExtension.WriteInfo("dddddddd");
						//循环发送消息以保持连接
						var tempByte = new byte[] { (byte)DefaultMessageIDTypes.ID_USER_PACKET_ENUM };
						rakPeer.Send(tempByte, tempByte.Length,
							PacketPriority.LOW_PRIORITY,
							PacketReliability.RELIABLE_ORDERED,
							(char)0,
							new AddressOrGUID(new SystemAddress(_peerServerAddress.Address, _peerServerAddress.Port)),
							false);
						Thread.Sleep(4000);
					}
				});
			}
			else
			{
				_isConnectPeerServer = true;
				new Thread(n =>
				{
					OnConnect(address, port, this);
				})
				{ IsBackground = true, Priority = ThreadPriority.Highest }.Start();
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
				RaknetExtension.WriteInfo("▲▲▲OnForwardingSuccess");
				_proxyServerAddress = new RaknetIPAddress(targetAddress.ToString(false), targetAddress.GetPort());
				var peer = proxyClientPlugin.GetRakPeerInterface();
				var systemAddress = peer.GetMyBoundAddress();
				peer.Connect(_proxyServerAddress.Address, _proxyServerAddress.Port, "", 0);
			}

			public override void OnForwardingInProgress(string proxyIPAddress, ushort proxyPort, SystemAddress proxyCoordinator, SystemAddress sourceAddress, SystemAddress targetAddress, RakNetGUID targetGuid, UDPProxyClient proxyClientPlugin)
			{
				RaknetExtension.WriteInfo("▲▲▲OnForwardingInProgress");
			}
			public override void OnAllServersBusy(SystemAddress proxyCoordinator, SystemAddress sourceAddress, SystemAddress targetAddress, RakNetGUID targetGuid, UDPProxyClient proxyClientPlugin)
			{
				RaknetExtension.WriteInfo("▲▲▲OnAllServersBusy");
			}
			public override void OnForwardingNotification(string proxyIPAddress, ushort proxyPort, SystemAddress proxyCoordinator, SystemAddress sourceAddress, SystemAddress targetAddress, RakNetGUID targetGuid, UDPProxyClient proxyClientPlugin)
			{
				RaknetExtension.WriteInfo("▲▲▲OnForwardingNotification");
			}
			public override void OnNoServersOnline(SystemAddress proxyCoordinator, SystemAddress sourceAddress, SystemAddress targetAddress, RakNetGUID targetGuid, UDPProxyClient proxyClientPlugin)
			{
				RaknetExtension.WriteInfo("▲▲▲OnNoServersOnline");
			}
			public override void OnRecipientNotConnected(SystemAddress proxyCoordinator, SystemAddress sourceAddress, SystemAddress targetAddress, RakNetGUID targetGuid, UDPProxyClient proxyClientPlugin)
			{
				RaknetExtension.WriteInfo("▲▲▲OnRecipientNotConnected");
			}
		}

		// <summary>
		/// 停止
		/// </summary>
		/// <param name="beforeAction"></param>
		public void Stop(Action beforeAction = null)
		{
			beforeAction?.Invoke();
			isProxyMsgSending = false;//（内部用）
			_isConnectPeerServer = false;
			string myAddress = GetMyAddress().ToString();
			rakPeer.CloseConnection(new AddressOrGUID(new SystemAddress(_natServerAddress.Address, _natServerAddress.Port)), true);
			rakPeer.CloseConnection(new AddressOrGUID(new SystemAddress(_coordinatorAddress.Address, _coordinatorAddress.Port)), true);
			rakPeer.CloseConnection(new AddressOrGUID(new SystemAddress(_natServerAddress.Address, _natServerAddress.Port)), true);
			isThreadRunning = false;
			rakPeer.Shutdown(10);
			RakPeerInterface.DestroyInstance(rakPeer);
			RaknetExtension.WriteWarning(string.Format("UdpPeerClient停止了：{0}", myAddress));
		}
	}
}
