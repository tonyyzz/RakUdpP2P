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
		static RaknetUdpPeerClient raknetUdpPeerClient = new RaknetUdpPeerClient();
		static void Main(string[] args)
		{
			#region udpProxy
			RaknetUdpProxyCoordinator raknetUdpProxyCoordinator = new RaknetUdpProxyCoordinator();
			var proxyCoordinatorStarted = raknetUdpProxyCoordinator.Start(RaknetConfig.coordinatorAddress);
			if (!proxyCoordinatorStarted)
			{
				Console.WriteLine("Coordinator启动失败");
				Console.ReadKey();
				return;
			}
			Console.WriteLine("Coordinator启动成功，IP地址为：{0}", raknetUdpProxyCoordinator.GetMyIpAddress().ToString());

			RaknetUdpProxyServer raknetUdpProxyServer = new RaknetUdpProxyServer();
			var proxyServerStarted = raknetUdpProxyServer.Start().Connect(raknetUdpProxyCoordinator.GetMyIpAddress());
			if (!proxyServerStarted)
			{
				Console.WriteLine("ProxyServer启动失败");
				Console.ReadKey();
				return;
			}
			Console.WriteLine("ProxyServer启动成功，IP地址为：{0}", raknetUdpProxyServer.GetMyIpAddress().ToString());
			#endregion

			RaknetUdpNATPTServer raknetUdpNATPTServer = new RaknetUdpNATPTServer();
			var udpNATPTServerStarted = raknetUdpNATPTServer.Start(RaknetConfig.natServerAddress);
			if (!udpNATPTServerStarted)
			{
				Console.WriteLine("UdpNATPTServer启动失败");
				Console.ReadKey();
				return;
			}
			Console.WriteLine("UdpNATPTServer启动成功，IP地址为：{0}", raknetUdpNATPTServer.GetMyIpAddress().ToString());

			//PeerServer
			//RaknetUdpPeerServer raknetUdpPeerServer = new RaknetUdpPeerServer();
			var udpPeerServerStarted = raknetUdpPeerServer.Start().Connect(raknetUdpNATPTServer.GetMyIpAddress(), raknetUdpProxyCoordinator.GetMyIpAddress());
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
			raknetUdpPeerServer.OnReceive += RaknetUdpPeerServer_OnReceive;

			//PeerClient
			//RaknetUdpPeerClient raknetUdpPeerClient = new RaknetUdpPeerClient();
			var udpPeerClientStarted = raknetUdpPeerClient.Start().Connect(raknetUdpNATPTServer.GetMyIpAddress(), raknetUdpProxyCoordinator.GetMyIpAddress(), raknetUdpPeerServer.GetMyIpAddress(), raknetUdpPeerServer.GetMyGuid());
			if (!udpPeerClientStarted)
			{
				Console.WriteLine("UdpPeerClient启动失败");
				Console.ReadKey();
				return;
			}
			Console.WriteLine("UdpPeerClient启动成功，IP地址为：{0}", raknetUdpPeerClient.GetMyIpAddress().ToString());
			raknetUdpPeerClient.OnConnect += RaknetUdpPeerClient_OnConnect;
			raknetUdpPeerClient.OnReceive += RaknetUdpPeerClient_OnReceive;


			Console.ReadKey();
		}


		private static void RaknetUdpPeerClient_OnConnect(string address, ushort port)
		{
			Console.WriteLine("与PeerClient连接的PeerServer的IPAddress为{0}", raknetUdpPeerClient.GetPeerServerAddress().ToString());
			Console.WriteLine("PeerClient给PeerServer发送消息");
			byte[] data = Encoding.UTF8.GetBytes("这是从Client发送的测试数据");
			raknetUdpPeerClient.Send(data);
		}
		private static void RaknetUdpPeerClient_OnReceive(string address, ushort port, byte[] bytes)
		{
			string dataStr = Encoding.UTF8.GetString(bytes);
			Console.WriteLine("PeerClient接收PeerServer发送过来的数据：【{0}】", dataStr);
		}


		private static void RaknetUdpPeerServer_OnConnect(string address, ushort port)
		{
			Console.WriteLine("PeerServer中的连接个数：{0}，最新连接进来的IPAddress为：{1}:{2}",
				raknetUdpPeerServer.GetConnectionCount(), address, port);
		}

		private static void RaknetUdpPeerServer_OnReceive(string address, ushort port, byte[] bytes)
		{
			string dataStr = Encoding.UTF8.GetString(bytes);
			Console.WriteLine("PeerServer接收PeerClient发送过来的数据：【{0}】", dataStr);

			Console.WriteLine("PeerServer给PeerClient发送消息");
			byte[] data = Encoding.UTF8.GetBytes("这是从Server发送的测试数据");
			raknetUdpPeerServer.Send(address, port, data);
		}
	}
}
