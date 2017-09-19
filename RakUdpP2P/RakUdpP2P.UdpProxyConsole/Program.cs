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
		static void Main(string[] args)
		{
			RaknetUdpProxyCoordinator raknetUdpProxyCoordinator = new RaknetUdpProxyCoordinator();
			var proxyCoordinatorStarted = raknetUdpProxyCoordinator.Start(RaknetConfig.coordinatorAddress);
			if (!proxyCoordinatorStarted)
			{
				Console.WriteLine("Coordinator启动失败");
				Console.ReadKey();
				return;
			}
			Console.WriteLine("Coordinator启动成功，IP地址为：{0}", raknetUdpProxyCoordinator.GetMyAddress().ToString());

			RaknetUdpProxyServer raknetUdpProxyServer = new RaknetUdpProxyServer();
			var proxyServerStarted = raknetUdpProxyServer.Start().Connect(raknetUdpProxyCoordinator.GetMyAddress());
			if (!proxyServerStarted)
			{
				Console.WriteLine("ProxyServer启动失败");
				Console.ReadKey();
				return;
			}
			Console.WriteLine("ProxyServer启动成功，IP地址为：{0}", raknetUdpProxyServer.GetMyAddress().ToString());

			Console.ReadKey();
		}

	}
}
