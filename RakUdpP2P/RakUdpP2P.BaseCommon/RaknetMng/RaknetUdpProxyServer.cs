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

		private RaknetIPAddress _coordinatorAddress = null;

		internal RaknetUdpProxyServer()
		{
			udpProxyServer = new UDPProxyServer();
		}

		internal RaknetUdpProxyServer Start(RaknetIPAddress localAddress = null, ushort maxConnCount = ushort.MaxValue)
		{
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
				RaknetExtension.WriteWarning(string.Format(@"proxyServer {0}端口被占用", socketDescriptor.port));
				return this;
			}
			return this;
		}

		internal bool Connect(RaknetIPAddress coordinatorAddress)
		{
			_coordinatorAddress = coordinatorAddress;
			OnConnectionRequestAccepted += RaknetUdpProxyServer_OnConnectionRequestAccepted;
			ReceiveThreadStart();
			var connectResult = rakPeer.Connect(coordinatorAddress.Address, coordinatorAddress.Port, "", 0);
			if (connectResult == ConnectionAttemptResult.CONNECTION_ATTEMPT_STARTED) //尝试连接开始
			{
				return true;
			}
			isThreadRunning = false;
			return false;
		}

		internal RaknetIPAddress GetMyIpAddress()
		{
			return GetMyAddress();
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
				RaknetExtension.WriteInfo("▲▲▲OnLoginSuccess");
			}

			public override void OnWrongPassword(RakString usedPassword, UDPProxyServer proxyServerPlugin)
			{
				RaknetExtension.WriteInfo("▲▲▲OnWrongPassword");
			}

			public override void OnAlreadyLoggedIn(RakString usedPassword, UDPProxyServer proxyServerPlugin)
			{
				RaknetExtension.WriteInfo("▲▲▲OnAlreadyLoggedIn");
			}

			public override void OnNoPasswordSet(RakString usedPassword, UDPProxyServer proxyServerPlugin)
			{
				RaknetExtension.WriteInfo("▲▲▲OnNoPasswordSet");
			}
		}

		// <summary>
		/// 停止
		/// </summary>
		/// <param name="beforeAction"></param>
		internal void Stop(Action beforeAction = null)
		{
			beforeAction?.Invoke();
			string myAddress = GetMyAddress().ToString();
			rakPeer.CloseConnection(new AddressOrGUID(new SystemAddress(_coordinatorAddress.Address, _coordinatorAddress.Port)), true);
			isThreadRunning = false;
			rakPeer.Shutdown(10);
			RakPeerInterface.DestroyInstance(rakPeer);
			RaknetExtension.WriteWarning(string.Format("UdpProxyServer停止了：{0}", myAddress));
		}
	}
}
