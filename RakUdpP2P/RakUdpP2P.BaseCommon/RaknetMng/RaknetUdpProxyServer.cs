using RakNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RakUdpP2P.BaseCommon.RaknetMng
{
	public class RaknetUdpProxyServer : RaknetBase
	{
		private UDPProxyServer udpProxyServer = null;

		public RaknetUdpProxyServer()
		{

		}

		public RaknetUdpProxyServer Start(RaknetAddress localAddress = null, ushort maxConnCount = ushort.MaxValue)
		{
			RaknetCSRunTest.JudgeRaknetCanRun();
			EventRegister();
			rakPeer = RakPeerInterface.GetInstance();
			udpProxyServer = new UDPProxyServer();
			rakPeer.AttachPlugin(udpProxyServer);
			udpProxyServer.SetResultHandler(new MyUDPProxyServerResultHandler());
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
				RaknetExtension.WriteWarning(string.Format(@"{0}端口被占用", socketDescriptor.port));
				return this;
			}
			List<int> startList = new List<int>()
			{
				(int)StartupResult.RAKNET_STARTED,
				(int)StartupResult.RAKNET_ALREADY_STARTED,
			};
			if (startList.Any(m => m == (int)startResult))
			{

			}
			else
			{

			}
			return this;
		}

		public bool Connect(RaknetAddress coordinatorAddress)
		{
			ReceiveThreadStart();
			var connectResult = rakPeer.Connect(coordinatorAddress.Address, coordinatorAddress.Port, "", 0);
			if (connectResult == ConnectionAttemptResult.CONNECTION_ATTEMPT_STARTED) //尝试连接开始
			{
				OnConnectionRequestAccepted += RaknetUdpProxyServer_OnConnectionRequestAccepted;
				return true;
			}
			return false;
		}



		public RaknetAddress GetMyAddress()
		{
			SystemAddress systemAddress = rakPeer.GetMyBoundAddress();
			return new RaknetAddress(systemAddress.ToString(false), systemAddress.GetPort());
		}

		private void RaknetUdpProxyServer_OnConnectionRequestAccepted(string address, ushort port)
		{
			//登录coordinator
			var flag = udpProxyServer.LoginToCoordinator(RaknetConfig.COORDINATOR_PASSWORD, new SystemAddress(address, port));
		}

		private class MyUDPProxyServerResultHandler : UDPProxyServerResultHandler
		{
			public override void OnLoginSuccess(RakString usedPassword, UDPProxyServer proxyServerPlugin)
			{
				Console.WriteLine("▲▲▲OnLoginSuccess");
			}

			public override void OnWrongPassword(RakString usedPassword, UDPProxyServer proxyServerPlugin)
			{
				Console.WriteLine("▲▲▲OnWrongPassword");
			}

			public override void OnAlreadyLoggedIn(RakString usedPassword, UDPProxyServer proxyServerPlugin)
			{
				Console.WriteLine("▲▲▲OnAlreadyLoggedIn");
			}

			public override void OnNoPasswordSet(RakString usedPassword, UDPProxyServer proxyServerPlugin)
			{
				Console.WriteLine("▲▲▲OnNoPasswordSet");
			}
		}
	}
}
