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
	public class RaknetUdpProxyCoordinator: RaknetBase
	{
		private UDPProxyCoordinator udpProxyCoordinator = null;

		public RaknetUdpProxyCoordinator()
		{

		}

		public bool Start(RaknetAddress localAddress = null, ushort maxConnCount = ushort.MaxValue)
		{
			RaknetCSRunTest.JudgeRaknetCanRun();
			EventRegister();
			rakPeer = RakPeerInterface.GetInstance();
			udpProxyCoordinator = new UDPProxyCoordinator();
			rakPeer.AttachPlugin(udpProxyCoordinator);
			udpProxyCoordinator.SetRemoteLoginPassword(new RakString(RaknetConfig.COORDINATOR_PASSWORD));
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
				return true;
			}
			return false;
		}

		public RaknetAddress GetMyAddress()
		{
			SystemAddress systemAddress = rakPeer.GetMyBoundAddress();
			return new RaknetAddress(systemAddress.ToString(false), systemAddress.GetPort());
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
