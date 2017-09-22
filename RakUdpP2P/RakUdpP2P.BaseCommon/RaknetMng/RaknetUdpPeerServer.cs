using RakNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RakUdpP2P.BaseCommon.RaknetMng
{
	public class RaknetUdpPeerServer : RaknetBase
	{
		/// <summary>
		/// 有新的PeerClient连接进来的事件通知
		/// </summary>
		public event Action<string, ushort, RaknetUdpPeerServer> OnConnect;
		/// <summary>
		/// 连接失败的事件通知
		/// </summary>
		public event Action<string, ushort, RaknetUdpPeerServer> OnConnectFailed;
		/// <summary>
		/// 收到PeerClient的消息事件通知
		/// </summary>
		public event Action<string, ushort, byte[], RaknetUdpPeerServer> OnReceive;
		/// <summary>
		/// 有PeerClient断开与PeerServer的连接事件通知
		/// </summary>
		public event Action<string, ushort, RaknetUdpPeerServer> OnDisConnect;
		public event Action<string, ushort, RaknetUdpPeerServer> OnLoseServerConnect;



		private NatPunchthroughClient natPunchthroughClient = null;
		//private NatTypeDetectionClient natTypeDetectionClient = null;
		private UDPProxyClient udpProxyClient = null;

		private RaknetIPAddress _natServerAddress = null;
		private RaknetIPAddress _coordinatorAddress = null;

		private bool isConnectNatServer = false;
		private bool isConnectCoordinator = false;

		public RaknetUdpPeerServer()
		{
			natPunchthroughClient = new NatPunchthroughClient();
			//natTypeDetectionClient = new NatTypeDetectionClient();
			udpProxyClient = new UDPProxyClient();
			OnConnect += RaknetUdpPeerServer_OnConnect;
			OnConnectFailed += RaknetUdpPeerServer_OnConnectFailed;
			OnDisConnect += RaknetUdpPeerServer_OnDisConnect;
			OnReceive += RaknetUdpPeerServer_OnReceive;
			OnLoseServerConnect += RaknetUdpPeerServer_OnLoseServerConnect;
		}

		private void RaknetUdpPeerServer_OnLoseServerConnect(string address, ushort port, RaknetUdpPeerServer raknetUdpPeerServer) { }

		private void RaknetUdpPeerServer_OnConnectFailed(string address, ushort port, RaknetUdpPeerServer raknetUdpPeerServer) { }

		private void RaknetUdpPeerServer_OnReceive(string address, ushort port, byte[] bytes, RaknetUdpPeerServer raknetUdpPeerServer) { }
		private void RaknetUdpPeerServer_OnDisConnect(string address, ushort port, RaknetUdpPeerServer raknetUdpPeerServer) { }
		private void RaknetUdpPeerServer_OnConnect(string address, ushort port, RaknetUdpPeerServer raknetUdpPeerServer) { }

		public RaknetUdpPeerServer Start(RaknetIPAddress localAddress = null, ushort maxConnCount = ushort.MaxValue)
		{
			rakPeer.AttachPlugin(natPunchthroughClient);
			//rakPeer.AttachPlugin(natTypeDetectionClient);
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
				RaknetExtension.WriteWarning(string.Format(@"peerServer {0}端口被占用", socketDescriptor.port));
			}
			return this;
		}

		public bool Connect(RaknetIPAddress natServerAddress, RaknetIPAddress coordinatorAddress)
		{
			_natServerAddress = natServerAddress;
			_coordinatorAddress = coordinatorAddress;
			OnNewIncomingConnection += RaknetUdpPeerServer_OnNewIncomingConnection;
			OnDisconnectionNotification += RaknetUdpPeerServer_OnDisconnectionNotification;
			OnRaknetReceive += RaknetUdpPeerServer_OnRaknetReceive;
			//OnUnconnectedPong += RaknetUdpPeerServer_OnUnconnectedPong;
			OnConnectionAttemptFailed += RaknetUdpPeerServer_OnConnectionAttemptFailed;
			OnNoFreeIncomingConnections += RaknetUdpPeerServer_OnNoFreeIncomingConnections;
			OnConnectionRequestAccepted += RaknetUdpPeerServer_OnConnectionRequestAccepted;
			OnConnectionLost += RaknetUdpPeerServer_OnConnectionLost;
			ReceiveThreadStart();

			var connectNatServerResult = rakPeer.Connect(_natServerAddress.Address, _natServerAddress.Port, RaknetConfig.natServerPwd, RaknetConfig.natServerPwd.Length);
			if (connectNatServerResult == ConnectionAttemptResult.CONNECTION_ATTEMPT_STARTED)//尝试连接穿透服务器开始
			{
				//natTypeDetectionClient.DetectNATType(new SystemAddress(_natServerAddress.Address, _natServerAddress.Port));

				//连接coordinator，为走Proxy流程备用
				udpProxyClient.SetResultHandler(new MyUDPProxyClientResultHandler());
				var connectCoordinatorResult = rakPeer.Connect(_coordinatorAddress.Address, _coordinatorAddress.Port, "", 0);
				if (connectCoordinatorResult == ConnectionAttemptResult.CONNECTION_ATTEMPT_STARTED) //尝试连接协调器开始
				{
					return true;
				}
				else
				{
					RaknetExtension.WriteWarning("peerServer尝试连接协调器失败");
				}
			}
			else
			{
				RaknetExtension.WriteWarning("peerServer尝试连接穿透服务器失败");
			}



			//另：先ping
			//var pingFlag = rakPeer.Ping(_natServerAddress.Address, _natServerAddress.Port, true);
			//if (pingFlag)
			//{
			//	return true;
			//}


			isThreadRunning = false;
			return false;
		}

		private void RaknetUdpPeerServer_OnConnectionLost(string address, ushort port)
		{
			if (address == _natServerAddress.Address && port == _natServerAddress.Port)
			{
				if (isConnectNatServer)
				{
					//失去NatServer的连接
					isConnectNatServer = false;
					isThreadRunning = false;
					rakPeer.Connect(_coordinatorAddress.Address, _coordinatorAddress.Port, "", 0);
				}
			}
			else if (address == _coordinatorAddress.Address && port == _coordinatorAddress.Port)
			{
				if (isConnectNatServer)
				{
					if (isConnectCoordinator)
					{

					}
					else
					{

					}
				}
				else
				{
					if (isConnectCoordinator)
					{
						//失去Proxy的连接，则表示彻底失去连接，触发事件
						OnLoseServerConnect(address, port, this);
					}
					else
					{

					}
				}
			}
			else
			{

			}
		}

		//尝试连接被接受
		private void RaknetUdpPeerServer_OnConnectionRequestAccepted(string address, ushort port)
		{
			if (address == _natServerAddress.Address && port == _natServerAddress.Port)
			{
				isConnectNatServer = true;
			}
			else if (address == _coordinatorAddress.Address && port == _coordinatorAddress.Port)
			{
				isConnectCoordinator = true;
			}
			else
			{

			}
		}

		private void RaknetUdpPeerServer_OnNoFreeIncomingConnections(string address, ushort port)
		{
			//连接失败
			isThreadRunning = false;
			OnConnectFailed(address, port, this);
		}

		private void RaknetUdpPeerServer_OnConnectionAttemptFailed(string address, ushort port)
		{
			if (isConnectNatServer)
			{
				if (isConnectCoordinator)
				{

				}
				else
				{

				}
			}
			else
			{
				if (isConnectCoordinator)
				{

				}
				else
				{
					//连接失败
					isThreadRunning = false;
					OnConnectFailed(address, port, this);
				}
			}

		}

		//private void RaknetUdpPeerServer_OnUnconnectedPong(string address, ushort port)
		//{
		//	//先ping通后再连接
		//	if (address == _natServerAddress.Address && port == _natServerAddress.Port)
		//	{
		//		//连接natServer
		//		var connectResult = rakPeer.Connect(_natServerAddress.Address, _natServerAddress.Port, RaknetConfig.natServerPwd, RaknetConfig.natServerPwd.Length);
		//		if (connectResult == ConnectionAttemptResult.CONNECTION_ATTEMPT_STARTED)//尝试连接穿透服务器开始
		//		{
		//			rakPeer.Ping(_coordinatorAddress.Address, _coordinatorAddress.Port, true);
		//		}
		//	}
		//	else if (address == _coordinatorAddress.Address && port == _coordinatorAddress.Port)
		//	{
		//		//连接coordinator
		//		rakPeer.Connect(_coordinatorAddress.Address, _coordinatorAddress.Port, "", 0);
		//	}
		//	else
		//	{

		//	}
		//}

		public RaknetIPAddress GetMyIpAddress()
		{
			return GetMyAddress();
		}

		public ulong GetMyGuid()
		{
			return rakPeer.GetMyGUID().g;
		}

		/// <summary>
		/// 获取所有连接的Peer列表
		/// </summary>
		/// <returns></returns>
		public List<RaknetIPAddress> GetAllConnectionList()
		{
			List<RaknetIPAddress> list = new List<RaknetIPAddress>();
			ushort numberOfSystemts = rakPeer.NumberOfConnections();
			rakPeer.GetConnectionList(out SystemAddress[] remoteSystems, ref numberOfSystemts);
			foreach (SystemAddress systemAddress in remoteSystems)
			{
				string address = systemAddress.ToString(false);
				ushort port = systemAddress.GetPort();
				list.Add(new RaknetIPAddress(address, port));
			}
			//过滤掉 NAT Server 和 Coordinator
			list.RemoveAll(m => m.Address == _natServerAddress.Address && m.Port == _natServerAddress.Port);
			list.RemoveAll(m => m.Address == _coordinatorAddress.Address && m.Port == _coordinatorAddress.Port);
			return list;
		}

		/// <summary>
		/// 获取Peer的连接个数
		/// </summary>
		/// <returns></returns>
		public int GetConnectionCount()
		{
			return GetAllConnectionList().Count();
		}

		public void Send(string peerAddress, ushort peerPort, byte[] bytes)
		{
			bytes = new byte[] { (byte)DefaultMessageIDTypes.ID_USER_PACKET_ENUM }.Concat(bytes).ToArray();
			rakPeer.Send(bytes, bytes.Length,
				PacketPriority.HIGH_PRIORITY,
				PacketReliability.RELIABLE_ORDERED,
				(char)0,
				new AddressOrGUID(new SystemAddress(peerAddress, peerPort)),
				false); //该值为false表示给指定的peer发送消息
		}

		public void Broadcast(string peerAddress, ushort peerPort, byte[] bytes)
		{
			bytes = new byte[] { (byte)DefaultMessageIDTypes.ID_USER_PACKET_ENUM }.Concat(bytes).ToArray();
			rakPeer.Send(bytes, bytes.Length,
				PacketPriority.HIGH_PRIORITY,
				PacketReliability.RELIABLE_ORDERED,
				(char)0,
				new AddressOrGUID(new SystemAddress(peerAddress, peerPort)),
				true); //该值为true表示广播，即给除此给PeerAddress之外的peer发送消息
		}

		private void RaknetUdpPeerServer_OnRaknetReceive(string address, ushort port, byte[] bytes)
		{
			OnReceive(address, port, bytes, this);
		}

		private void RaknetUdpPeerServer_OnDisconnectionNotification(string address, ushort port)
		{
			OnDisConnect(address, port, this);
		}

		private void RaknetUdpPeerServer_OnNewIncomingConnection(string address, ushort port)
		{
			OnConnect(address, port, this);
		}

		private class MyUDPProxyClientResultHandler : UDPProxyClientResultHandler
		{
			public override void OnForwardingSuccess(string proxyIPAddress, ushort proxyPort, SystemAddress proxyCoordinator, SystemAddress sourceAddress, SystemAddress targetAddress, RakNetGUID targetGuid, UDPProxyClient proxyClientPlugin)
			{
				RaknetExtension.WriteInfo("▲▲▲OnForwardingSuccess");
			}

			public override void OnForwardingInProgress(string proxyIPAddress, ushort proxyPort, SystemAddress proxyCoordinator, SystemAddress sourceAddress, SystemAddress targetAddress, RakNetGUID targetGuid, UDPProxyClient proxyClientPlugin)
			{
				RaknetExtension.WriteInfo("▲▲▲OnForwardingInProgress");
			}
			public override void OnAllServersBusy(SystemAddress proxyCoordinator, SystemAddress sourceAddress, SystemAddress targetAddress, RakNetGUID targetGuid, UDPProxyClient proxyClientPlugin)
			{
				RaknetExtension.WriteInfo("▲▲▲OnAllServersBusy");
			}
			public override void OnForwardingNotification(string proxyIPAddress, ushort proxyPort, SystemAddress proxyCoordinator, SystemAddress sourceAddress, SystemAddress targetAddress, RakNetGUID targetGuid, UDPProxyClient proxyClientPlugin)
			{
				RaknetExtension.WriteInfo("▲▲▲OnForwardingNotification");
			}
			public override void OnNoServersOnline(SystemAddress proxyCoordinator, SystemAddress sourceAddress, SystemAddress targetAddress, RakNetGUID targetGuid, UDPProxyClient proxyClientPlugin)
			{
				RaknetExtension.WriteInfo("▲▲▲OnNoServersOnline");
			}
			public override void OnRecipientNotConnected(SystemAddress proxyCoordinator, SystemAddress sourceAddress, SystemAddress targetAddress, RakNetGUID targetGuid, UDPProxyClient proxyClientPlugin)
			{
				RaknetExtension.WriteInfo("▲▲▲OnRecipientNotConnected");
			}
		}

		// <summary>
		/// 停止
		/// </summary>
		/// <param name="beforeAction"></param>
		public void Stop(Action beforeAction = null)
		{
			beforeAction?.Invoke();
			string myAddress = GetMyAddress().ToString();
			rakPeer.CloseConnection(new AddressOrGUID(new SystemAddress(_natServerAddress.Address, _natServerAddress.Port)), true);
			rakPeer.CloseConnection(new AddressOrGUID(new SystemAddress(_coordinatorAddress.Address, _coordinatorAddress.Port)), true);
			isThreadRunning = false;
			rakPeer.Shutdown(10);
			RakPeerInterface.DestroyInstance(rakPeer);
			RaknetExtension.WriteWarning(string.Format("UdpPeerServer停止了：{0}", myAddress));
		}
	}
}
