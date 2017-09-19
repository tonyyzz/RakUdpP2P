using RakUdpP2P.BaseCommon.RaknetMng;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RakUdpP2P.UdpProxyConsole
{
	class Program
	{
		static RaknetUdpPeerServer raknetUdpPeerServer = new RaknetUdpPeerServer();
		static void Main(string[] args)
		{
			#region udpProxy
			//RaknetUdpProxyCoordinator raknetUdpProxyCoordinator = new RaknetUdpProxyCoordinator();
			//var proxyCoordinatorStarted = raknetUdpProxyCoordinator.Start(RaknetConfig.coordinatorAddress);
			//if (!proxyCoordinatorStarted)
			//{
			//	Console.WriteLine("Coordinator启动失败");
			//	Console.ReadKey();
			//	return;
			//}
			//Console.WriteLine("Coordinator启动成功，IP地址为：{0}", raknetUdpProxyCoordinator.GetMyAddress().ToString());

			//RaknetUdpProxyServer raknetUdpProxyServer = new RaknetUdpProxyServer();
			//var proxyServerStarted = raknetUdpProxyServer.Start().Connect(raknetUdpProxyCoordinator.GetMyAddress());
			//if (!proxyServerStarted)
			//{
			//	Console.WriteLine("ProxyServer启动失败");
			//	Console.ReadKey();
			//	return;
			//}
			//Console.WriteLine("ProxyServer启动成功，IP地址为：{0}", raknetUdpProxyServer.GetMyAddress().ToString()); 
			#endregion

			RaknetUdpNATPTServer raknetUdpNATPTServer = new RaknetUdpNATPTServer();
			var udpNATPTServerStarted = raknetUdpNATPTServer.Start(RaknetConfig.natServerAddress);
			if (!udpNATPTServerStarted)
			{
				Console.WriteLine("UdpNATPTServer启动失败");
				Console.ReadKey();
				return;
			}
			Console.WriteLine("UdpNATPTServer启动成功，IP地址为：{0}", raknetUdpNATPTServer.GetMyAddress().ToString());


			//RaknetUdpPeerServer raknetUdpPeerServer = new RaknetUdpPeerServer();
			var udpPeerServerStarted = raknetUdpPeerServer.Start().Connect(raknetUdpNATPTServer.GetMyAddress());
			if (!udpPeerServerStarted)
			{
				Console.WriteLine("UdpPeerServer启动失败");
				Console.ReadKey();
				return;
			}
			Console.WriteLine("UdpPeerServer启动成功，IP地址为：{0}，GUID：{1}",
				raknetUdpPeerServer.GetMyAddress().ToString(),
				raknetUdpPeerServer.GetMyRaknetGUID()
				);

			RaknetUdpPeerClient raknetUdpPeerClient = new RaknetUdpPeerClient();
			var udpPeerClientStarted = raknetUdpPeerClient.Start().Connect(raknetUdpPeerServer.GetMyAddress(), raknetUdpPeerServer.GetMyRaknetGUID());
			if (!udpPeerClientStarted)
			{
				Console.WriteLine("UdpPeerClient启动失败");
				Console.ReadKey();
				return;
			}
			Console.WriteLine("UdpPeerClient启动成功，IP地址为：{0}", raknetUdpPeerClient.GetMyAddress().ToString());


			Console.ReadKey();
		}

		
	}
}
