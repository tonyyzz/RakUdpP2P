using RakNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RakUdpP2P.BaseCommon.RaknetMng
{
	public class RaknetUdpPeerServer : RaknetBase
	{
		private NatPunchthroughClient natPunchthroughClient = null;
		//private NatTypeDetectionClient natTypeDetectionClient = null;
		private UDPProxyClient udpProxyClient = null;

		private RaknetAddress _natServerAddress = null;
		private RaknetAddress _coordinatorAddress = null;

		public RaknetUdpPeerServer()
		{
			natPunchthroughClient = new NatPunchthroughClient();
			//natTypeDetectionClient = new NatTypeDetectionClient();
			udpProxyClient = new UDPProxyClient();
		}
		public RaknetUdpPeerServer Start(RaknetAddress localAddress = null, ushort maxConnCount = ushort.MaxValue)
		{
			rakPeer.AttachPlugin(natPunchthroughClient);
			//rakPeer.AttachPlugin(natTypeDetectionClient);
			rakPeer.SetMaximumIncomingConnections(maxConnCount);
			SocketDescriptor socketDescriptor = new SocketDescriptor();
			if (localAddress != null && !string.IsNullOrWhiteSpace(localAddress.Address) && localAddress.Port > 0)
			{
				socketDescriptor.hostAddress = localAddress.Address;
				socketDescriptor.port = localAddress.Port;
			}
			var startResult = rakPeer.Startup(maxConnCount, socketDescriptor, 1);
			if (startResult == StartupResult.SOCKET_PORT_ALREADY_IN_USE)
			{
				RaknetExtension.WriteWarning(string.Format(@"peerServer {0}端口被占用", socketDescriptor.port));
			}
			return this;
		}

		public bool Connect(RaknetAddress natServerAddress, RaknetAddress coordinatorAddress)
		{
			_natServerAddress = natServerAddress;
			_coordinatorAddress = coordinatorAddress;
			ReceiveThreadStart();
			var connectNatServerResult = rakPeer.Connect(_natServerAddress.Address, _natServerAddress.Port, RaknetConfig.natServerPwd, RaknetConfig.natServerPwd.Length);
			if (connectNatServerResult == ConnectionAttemptResult.CONNECTION_ATTEMPT_STARTED)//尝试连接穿透服务器开始
			{
				//natTypeDetectionClient.DetectNATType(new SystemAddress(_natServerAddress.Address, _natServerAddress.Port));

				//未完。。。，还要连接coordinator，走Proxy流程
				udpProxyClient.SetResultHandler(new MyUDPProxyClientResultHandler());
				var connectCoordinatorResult = rakPeer.Connect(_coordinatorAddress.Address, _coordinatorAddress.Port, "", 0);
				if (connectCoordinatorResult == ConnectionAttemptResult.CONNECTION_ATTEMPT_STARTED) //尝试连接协调器开始
				{
					return true;
				}
				else
				{
					RaknetExtension.WriteWarning("peerServer连接协调器失败");
				}
			}
			else
			{
				RaknetExtension.WriteWarning("peerServer连接穿透服务器失败");
			}
			isThreadRunning = false;
			return false;
		}

		// <summary>
		/// 停止
		/// </summary>
		/// <param name="beforeAction"></param>
		public void Stop(Action beforeAction = null)
		{
			beforeAction?.Invoke();
			string myAddress = GetMyAddress().ToString();
			rakPeer.CloseConnection(new AddressOrGUID(new SystemAddress(_natServerAddress.Address, _natServerAddress.Port)), true);
			isThreadRunning = false;
			rakPeer.Shutdown(10);
			RakPeerInterface.DestroyInstance(rakPeer);
			Console.WriteLine("UdpPeerServer停止了：{0}", myAddress);
		}

		private class MyUDPProxyClientResultHandler : UDPProxyClientResultHandler
		{
			public override void OnForwardingSuccess(string proxyIPAddress, ushort proxyPort, SystemAddress proxyCoordinator, SystemAddress sourceAddress, SystemAddress targetAddress, RakNetGUID targetGuid, UDPProxyClient proxyClientPlugin)
			{
				Console.WriteLine("▲▲▲OnForwardingSuccess");
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
