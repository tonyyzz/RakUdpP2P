using RakUdpP2P.BaseCommon;
using RakUdpP2P.BaseCommon.RaknetMng;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RakUdpP2P.PeerClient
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

			Console.WriteLine("请输入PeerServer IPAddress");
			string peerServerIpAddress = Console.ReadLine();

			Console.WriteLine("请输入PeerServer Port：");
			string peerServerPort = Console.ReadLine();

			ushort peerServerPortUshort = 0;
			ushort.TryParse(peerServerPort, out peerServerPortUshort);

			Console.WriteLine("请输入PeerServer GUID：");
			string peerServerGuid = Console.ReadLine();

			ulong peerServerGuidUlong = 0;
			ulong.TryParse(peerServerGuid, out peerServerGuidUlong);


			var raknetUdpNATPTServerAddress = new RaknetIPAddress(natServerIpAddress, natServerPortUshort);
			var raknetUdpProxyAddress = new RaknetIPAddress(proxyIpAddress, proxyPortUshort);

			var raknetUdpPeerServerAddress = new RaknetIPAddress(peerServerIpAddress, peerServerPortUshort);

			//start PeerClient
			RaknetUdpPeerClient raknetUdpPeerClient = new RaknetUdpPeerClient();
			var udpPeerClientStarted = raknetUdpPeerClient.Start().Connect(raknetUdpNATPTServerAddress, raknetUdpProxyAddress, raknetUdpPeerServerAddress, peerServerGuidUlong);
			if (!udpPeerClientStarted)
			{
				Console.WriteLine("UdpPeerClient启动失败");
				Console.ReadKey();
				return;
			}
			Console.WriteLine("UdpPeerClient启动成功，IP地址为：{0}", raknetUdpPeerClient.GetMyIpAddress().ToString());
			raknetUdpPeerClient.OnConnect += RaknetUdpPeerClient_OnConnect;
			raknetUdpPeerClient.OnConnectFailed += RaknetUdpPeerClient_OnConnectFailed;
			raknetUdpPeerClient.OnReceive += RaknetUdpPeerClient_OnReceive;

			ConsoleCloseHandler.raknetUdpPeerClient = raknetUdpPeerClient;
			ConsoleCloseHandler.SetConsoleCtrlHandler(ConsoleCloseHandler.cancelHandler, true);

			Console.WriteLine("------------------外网地址：{0}", IPAddressUtils.GetOuterNatIP());
		}

		private static void RaknetUdpPeerClient_OnConnect(string address, ushort port, RaknetUdpPeerClient raknetUdpPeerClient)
		{
			//step 1.1
			Console.WriteLine("与PeerClient连接的PeerServer的IPAddress为{0}", raknetUdpPeerClient.GetPeerServerAddress().ToString());
			Console.WriteLine("PeerClient给PeerServer发送消息");
			byte[] data = Encoding.UTF8.GetBytes("这是从Client发送的测试数据");
			raknetUdpPeerClient.Send(data);
		}
		private void RaknetUdpPeerClient_OnConnectFailed(string address, ushort port, RaknetUdpPeerClient raknetUdpPeerClient)
		{
			Console.WriteLine("PeerClient尝试连接【{0}:{1}】失败", address, port);//address和port则表示尝试连接但失败的ip地址和端口

			//PeerClient端
			//如果尝试连接失败，那就是失败，不另作处理

		}
		private static void RaknetUdpPeerClient_OnReceive(string address, ushort port, byte[] bytes, RaknetUdpPeerClient raknetUdpPeerClient)
		{
			//step 4
			string dataStr = Encoding.UTF8.GetString(bytes);
			Console.WriteLine("PeerClient接收PeerServer发送过来的数据：【{0}】", dataStr);
		}
	}
}
