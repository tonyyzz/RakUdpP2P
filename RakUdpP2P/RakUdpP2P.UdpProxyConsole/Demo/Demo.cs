﻿using RakUdpP2P.BaseCommon.RaknetMng;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RakUdpP2P.UdpProxyConsole
{
	class Demo
	{
		public void Do()
		{
			#region udpProxy

			//start proxy
			RaknetUdpProxy raknetUdpProxy = new RaknetUdpProxy();
			var proxyStarted = raknetUdpProxy.Start(RaknetConfig.proxyAddress);
			if (!proxyStarted)
			{
				Console.WriteLine("Proxy启动失败");
				Console.ReadKey();
				return;
			}
			Console.WriteLine("Proxy启动成功，IP地址为：{0}", raknetUdpProxy.GetMyIpAddress().ToString());

			#endregion


			//start natServer
			RaknetUdpNATPTServer raknetUdpNATPTServer = new RaknetUdpNATPTServer();
			var udpNATPTServerStarted = raknetUdpNATPTServer.Start(RaknetConfig.natServerAddress);
			if (!udpNATPTServerStarted)
			{
				Console.WriteLine("UdpNATPTServer启动失败");
				Console.ReadKey();
				return;
			}
			Console.WriteLine("UdpNATPTServer启动成功，IP地址为：{0}", raknetUdpNATPTServer.GetMyIpAddress().ToString());

			//start PeerServer
			RaknetUdpPeerServer raknetUdpPeerServer = new RaknetUdpPeerServer();
			var udpPeerServerStarted = raknetUdpPeerServer.Start().Connect(raknetUdpNATPTServer.GetMyIpAddress(), raknetUdpProxy.GetMyIpAddress());
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

			//start PeerClient
			RaknetUdpPeerClient raknetUdpPeerClient = new RaknetUdpPeerClient();
			var udpPeerClientStarted = raknetUdpPeerClient.Start().Connect(raknetUdpNATPTServer.GetMyIpAddress(), raknetUdpProxy.GetMyIpAddress(), raknetUdpPeerServer.GetMyIpAddress(), raknetUdpPeerServer.GetMyGuid());
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
			Console.WriteLine("PeerClient尝试连接【{0}:{1}】失败", address, port);
		}
		private static void RaknetUdpPeerClient_OnReceive(string address, ushort port, byte[] bytes, RaknetUdpPeerClient raknetUdpPeerClient)
		{
			//step 4
			string dataStr = Encoding.UTF8.GetString(bytes);
			Console.WriteLine("PeerClient接收PeerServer发送过来的数据：【{0}】", dataStr);
		}


		private static void RaknetUdpPeerServer_OnConnect(string address, ushort port, RaknetUdpPeerServer raknetUdpPeerServer)
		{
			//step 1.2
			Console.WriteLine("PeerServer中的连接个数：{0}，最新连接进来的IPAddress为：{1}:{2}",
				raknetUdpPeerServer.GetConnectionCount(), address, port);
		}

		private void RaknetUdpPeerServer_OnConnectFailed(string address, ushort port, RaknetUdpPeerServer raknetUdpPeerServer)
		{
			Console.WriteLine("PeerServer尝试连接【{0}:{1}】失败", address, port);
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
