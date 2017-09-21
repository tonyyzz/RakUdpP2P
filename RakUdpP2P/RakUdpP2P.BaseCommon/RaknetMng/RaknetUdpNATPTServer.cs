using RakNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RakUdpP2P.BaseCommon.RaknetMng
{
	public class RaknetUdpNATPTServer : RaknetBase
	{
		private NatPunchthroughServer natPunchthroughServer = null;

		public RaknetUdpNATPTServer()
		{
			natPunchthroughServer = new NatPunchthroughServer();
		}

		public bool Start(RaknetIPAddress localAddress = null, ushort maxConnCount = ushort.MaxValue)
		{
			rakPeer.AttachPlugin(natPunchthroughServer);
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
			ReceiveThreadStart();
			List<int> startList = new List<int>()
			{
				(int)StartupResult.RAKNET_STARTED,
				(int)StartupResult.RAKNET_ALREADY_STARTED,
			};
			if (startList.Any(m => m == (int)startResult))
			{
				return true;
			}
			isThreadRunning = false;
			return false;
		}

		public RaknetIPAddress GetMyIpAddress()
		{
			return GetMyAddress();
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
			Console.WriteLine("UdpNATPTServer停止了：{0}", myAddress);
		}
	}
}
