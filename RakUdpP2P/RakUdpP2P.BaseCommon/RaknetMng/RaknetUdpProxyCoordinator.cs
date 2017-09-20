using RakNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RakUdpP2P.BaseCommon.RaknetMng
{
	/// <summary>
	/// Raknet UdpProxyCoordinator 
	/// </summary>
	public class RaknetUdpProxyCoordinator : RaknetBase
	{
		private UDPProxyCoordinator udpProxyCoordinator = null;

		public RaknetUdpProxyCoordinator()
		{
			udpProxyCoordinator = new UDPProxyCoordinator();
		}

		public bool Start(RaknetAddress localAddress = null, ushort maxConnCount = ushort.MaxValue)
		{
			rakPeer.AttachPlugin(udpProxyCoordinator);
			udpProxyCoordinator.SetRemoteLoginPassword(RaknetConfig.COORDINATOR_PASSWORD);
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
				return false;
			}
			List<int> startList = new List<int>()
			{
				(int)StartupResult.RAKNET_STARTED,
				(int)StartupResult.RAKNET_ALREADY_STARTED,
			};
			if (startList.Any(m => m == (int)startResult))
			{
				ReceiveThreadStart();
				OnUdpProxyGeneral += RaknetUdpProxyCoordinator_OnUdpProxyGeneral;
				return true;
			}
			return false;
		}

		private void RaknetUdpProxyCoordinator_OnUdpProxyGeneral(string address, ushort port, byte theByte)
		{
			RaknetExtension.WriteInfo(string.Format(@"ID_UDP_PROXY_GENERAL类型：{0}", theByte));
		}

		/// <summary>
		/// 停止
		/// </summary>
		/// <param name="beforeAction"></param>
		public void Stop(Action beforeAction = null)
		{
			beforeAction?.Invoke();
			string myAddress = GetMyAddress().ToString();
			isThreadRunning = false;
			rakPeer.Shutdown(10);
			RakPeerInterface.DestroyInstance(rakPeer);
			Console.WriteLine("coordinator停止了：{0}", myAddress);
		}
	}
}
