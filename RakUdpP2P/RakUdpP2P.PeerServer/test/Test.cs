using RakUdpP2P.BaseCommon;
using RakUdpP2P.BaseCommon.RaknetMng;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RakUdpP2P.PeerServer
{
	class Test
	{
		public void Do()
		{

			Console.WriteLine("请输入NatServer IPAddress：");
			string natServerIpAddress = Console.ReadLine();

			Console.WriteLine("请输入NatServer Port：");
			string natServerPort = Console.ReadLine();

			ushort natServerPortUshort = 0;
			ushort.TryParse(natServerPort, out natServerPortUshort);

			Console.WriteLine("请输入Proxy IPAddress：");
			string proxyIpAddress = Console.ReadLine();

			Console.WriteLine("请输入Proxy Port：");
			string proxyPort = Console.ReadLine();

			ushort proxyPortUshort = 0;
			ushort.TryParse(proxyPort, out proxyPortUshort);

			var raknetUdpNATPTServerAddress = new RaknetIPAddress(natServerIpAddress, natServerPortUshort);
			var raknetUdpProxyAddress = new RaknetIPAddress(proxyIpAddress, proxyPortUshort);

			//start PeerServer
			RaknetUdpPeerServer raknetUdpPeerServer = new RaknetUdpPeerServer();
			var udpPeerServerStarted = raknetUdpPeerServer.Start().Connect(raknetUdpNATPTServerAddress, raknetUdpProxyAddress);
			if (!udpPeerServerStarted)
			{
				Console.WriteLine("UdpPeerServer启动失败");
				Console.ReadKey();
				return;
			}
			Console.WriteLine("UdpPeerServer启动成功，IP地址为：{0}，GUID：{1}",
				raknetUdpPeerServer.GetMyIpAddress().ToString(),
				raknetUdpPeerServer.GetMyGuid()
				);
			raknetUdpPeerServer.OnConnect += RaknetUdpPeerServer_OnConnect;
			raknetUdpPeerServer.OnConnectFailed += RaknetUdpPeerServer_OnConnectFailed;
			raknetUdpPeerServer.OnReceive += RaknetUdpPeerServer_OnReceive;

			ConsoleCloseHandler.raknetUdpPeerServer = raknetUdpPeerServer;
			ConsoleCloseHandler.SetConsoleCtrlHandler(ConsoleCloseHandler.cancelHandler, true);

			Console.WriteLine("------------------外网地址：{0}", IPAddressUtils.GetOuterNatIP());

			Console.WriteLine("PeerServer GUID：{0}", raknetUdpPeerServer.GetMyGuid());
		}

		private static void RaknetUdpPeerServer_OnConnect(string address, ushort port, RaknetUdpPeerServer raknetUdpPeerServer)
		{
			//step 1.2
			Console.WriteLine("PeerServer中的连接个数：{0}，最新连接进来的IPAddress为：{1}:{2}",
				raknetUdpPeerServer.GetConnectionCount(), address, port);
		}

		private void RaknetUdpPeerServer_OnConnectFailed(string address, ushort port, RaknetUdpPeerServer raknetUdpPeerServer)
		{
			Console.WriteLine("PeerServer尝试连接【{0}:{1}】失败", address, port);//address和port则表示尝试连接但失败的ip地址和端口

			//PeerServer端
			//接下来的思路：如果连接失败，则直接遍历剩下的还未尝试连接的Nat服务器和proxy服务器重连，如果所有的都尝试连接失败，则表示失败
			//（请事先获取所有可以连接的Nat服务器和Proxy服务器的IPAddress，优先遍历所有的NatServer，因为Nat成功率高，而proxy只是作为备选方案）
			//客户端自行实现...

		}

		private static void RaknetUdpPeerServer_OnReceive(string address, ushort port, byte[] bytes, RaknetUdpPeerServer raknetUdpPeerServer)
		{
			//step 2
			string dataStr = Encoding.UTF8.GetString(bytes);
			Console.WriteLine("PeerServer接收PeerClient发送过来的数据：【{0}】", dataStr);

			//step 3
			Console.WriteLine("PeerServer给PeerClient发送消息");
			byte[] data = Encoding.UTF8.GetBytes("这是从Server发送的测试数据");
			raknetUdpPeerServer.Send(address, port, data);
		}
	}
}
