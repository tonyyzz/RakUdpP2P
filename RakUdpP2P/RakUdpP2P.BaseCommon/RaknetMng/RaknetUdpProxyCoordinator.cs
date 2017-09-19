using RakNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RakUdpP2P.BaseCommon.RaknetMng
{
	public class RaknetUdpProxyCoordinator
	{
		public event Action<string, ushort> OnNewIncomingConnection;
		public event Action<string, ushort, byte[]> OnReiceve;
		public event Action<string, ushort> OnDisconnectionNotification;
		public event Action<string, ushort> OnConnectionLost;
		public event Action<string, ushort> OnNatPunchthroughSucceeded;
		public event Action<string, ushort> OnNatPunchthroughFailed;
		public event Action<string, ushort> OnConnectionAttemptFailed;
		public event Action<string, ushort> OnConnectionAttemptFailedCommon;
		public event Action<string, ushort> OnNoFreeIncomingConnections;
		public event Action<string, ushort> OnIncompatibleProtocolVersion;
		public event Action<string, ushort> OnFcm2NewHost;
		public event Action<string, ushort, byte> OnNatTypeDetectionRequest;
		public event Action<string, ushort> OnUnconnectedPing;
		public event Action<string, ushort> OnUnconnectedPong;
		public event Action<string, ushort, byte> OnUdpProxyGeneral;


		private RakPeerInterface rakPeer = null;
		private UDPProxyCoordinator udpProxyCoordinator = null;
		private bool isThreadRunning = true;

		public RaknetUdpProxyCoordinator()
		{
			rakPeer = RakPeerInterface.GetInstance();
			udpProxyCoordinator = new UDPProxyCoordinator();
			rakPeer.AttachPlugin(udpProxyCoordinator);
			udpProxyCoordinator.SetRemoteLoginPassword(new RakString(RaknetConfig.COORDINATOR_PASSWORD));

			OnNewIncomingConnection += RaknetUdpProxyCoordinator_OnNewIncomingConnection;
			OnReiceve += RaknetUdpProxyCoordinator_OnReiceve;
			OnDisconnectionNotification += RaknetUdpProxyCoordinator_OnDisconnectionNotification;
			OnConnectionLost += RaknetUdpProxyCoordinator_OnConnectionLost;
			OnNatPunchthroughSucceeded += RaknetUdpProxyCoordinator_OnNatPunchthroughSucceeded;
			OnNatPunchthroughFailed += RaknetUdpProxyCoordinator_OnNatPunchthroughFailed;
			OnConnectionAttemptFailed += RaknetUdpProxyCoordinator_OnConnectionAttemptFailed;
			OnNoFreeIncomingConnections += RaknetUdpProxyCoordinator_OnNoFreeIncomingConnections;
			OnIncompatibleProtocolVersion += RaknetUdpProxyCoordinator_OnIncompatibleProtocolVersion;
			OnConnectionAttemptFailedCommon += RaknetUdpProxyCoordinator_OnConnectionAttemptFailedCommon;
			OnFcm2NewHost += RaknetUdpProxyCoordinator_OnFcm2NewHost;
			OnNatTypeDetectionRequest += RaknetUdpProxyCoordinator_OnNatTypeDetectionRequest;
			OnUnconnectedPing += RaknetUdpProxyCoordinator_OnUnconnectedPing;
			OnUnconnectedPong += RaknetUdpProxyCoordinator_OnUnconnectedPong;
			OnUdpProxyGeneral += RaknetUdpProxyCoordinator_OnUdpProxyGeneral;


		}

		private void RaknetUdpProxyCoordinator_OnUdpProxyGeneral(string address, ushort port, byte typeByte) { }
		private void RaknetUdpProxyCoordinator_OnUnconnectedPong(string address, ushort port) { }
		private void RaknetUdpProxyCoordinator_OnUnconnectedPing(string address, ushort port) { }
		private void RaknetUdpProxyCoordinator_OnNatTypeDetectionRequest(string address, ushort port, byte typeByte) { }
		private void RaknetUdpProxyCoordinator_OnFcm2NewHost(string address, ushort port) { }
		private void RaknetUdpProxyCoordinator_OnConnectionAttemptFailedCommon(string address, ushort port) { }
		private void RaknetUdpProxyCoordinator_OnIncompatibleProtocolVersion(string address, ushort port) { }
		private void RaknetUdpProxyCoordinator_OnNoFreeIncomingConnections(string address, ushort port) { }
		private void RaknetUdpProxyCoordinator_OnConnectionAttemptFailed(string address, ushort port) { }
		private void RaknetUdpProxyCoordinator_OnNatPunchthroughFailed(string address, ushort port) { }
		private void RaknetUdpProxyCoordinator_OnNatPunchthroughSucceeded(string address, ushort port) { }
		private void RaknetUdpProxyCoordinator_OnReiceve(string address, ushort port, byte[] datas) { }
		private void RaknetUdpProxyCoordinator_OnConnectionLost(string address, ushort port) { }
		private void RaknetUdpProxyCoordinator_OnDisconnectionNotification(string address, ushort port) { }
		private void RaknetUdpProxyCoordinator_OnNewIncomingConnection(string address, ushort port) { }


		public bool Start(RaknetAddress localAddress = null, ushort maxConnCount = ushort.MaxValue)
		{
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
				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.WriteLine(string.Format(@"{0}端口被占用", socketDescriptor.port));
				Console.ResetColor();
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

		private void ReceiveThreadStart()
		{
			ThreadPool.SetMaxThreads(10, 10);
			ThreadPool.QueueUserWorkItem(o =>
			{
				isThreadRunning = true;

				string peerAddress = "";
				ushort peerPort = 0;

				while (true)
				{
					if (!isThreadRunning)
					{
						break;
					}
					if (rakPeer == null)
					{
						continue;
					}
					using (Packet testPacket = rakPeer.Receive())
					{
						if (testPacket == null || testPacket.data.Count() <= 0)
						{
							continue;
						}
						DefaultMessageIDTypes defaultMessageIDType = (DefaultMessageIDTypes)testPacket.data[0];
						Console.WriteLine(defaultMessageIDType.GetMessageIDTypeStr());
						peerAddress = testPacket.systemAddress.ToString(false);
						peerPort = testPacket.systemAddress.GetPort();
						switch (defaultMessageIDType)
						{
							case DefaultMessageIDTypes.ID_UNCONNECTED_PING: //1
								{
									defaultMessageIDType.WriteMsgTypeInfo(peerAddress, peerPort, "");
									OnUnconnectedPing(peerAddress, peerPort);
								}
								break;
							case DefaultMessageIDTypes.ID_UNCONNECTED_PONG: //28
								{
									defaultMessageIDType.WriteMsgTypeInfo(peerAddress, peerPort, "");
									OnUnconnectedPong(peerAddress, peerPort);
								}
								break;
							case DefaultMessageIDTypes.ID_UDP_PROXY_GENERAL: //92  ID_UDP_PROXY_GENERAL类型
								{
									defaultMessageIDType.WriteMsgTypeInfo(peerAddress, peerPort, "ID_UDP_PROXY_GENERAL类型");
									OnUdpProxyGeneral(peerAddress, peerPort, testPacket.data[1]);
								}
								break;
							case DefaultMessageIDTypes.ID_NAT_TYPE_DETECTION_REQUEST: //96 得到路由类型结果
								{
									defaultMessageIDType.WriteMsgTypeInfo(peerAddress, peerPort, "得到路由类型结果");
									OnNatTypeDetectionRequest(peerAddress, peerPort, testPacket.data[1]);
								}
								break;
							case DefaultMessageIDTypes.ID_FCM2_NEW_HOST: //82
								{
									defaultMessageIDType.WriteMsgTypeInfo(peerAddress, peerPort, "");
									OnFcm2NewHost(peerAddress, peerPort);
								}
								break;
							case DefaultMessageIDTypes.ID_INCOMPATIBLE_PROTOCOL_VERSION: //25 - 协议版本不兼容，一般是由于两端AttachPlugin没有对应
								{
									defaultMessageIDType.WriteMsgTypeInfo(peerAddress, peerPort, "协议版本不兼容，一般是由于两端AttachPlugin没有对应");
									OnIncompatibleProtocolVersion(peerAddress, peerPort);
								}
								break;
							case DefaultMessageIDTypes.ID_NAT_PUNCHTHROUGH_SUCCEEDED: //67-客户端NAT穿透成功
								{
									defaultMessageIDType.WriteMsgTypeInfo(peerAddress, peerPort, "客户端NAT穿透成功");
									OnNatPunchthroughSucceeded(peerAddress, peerPort);
								}
								break;
							case DefaultMessageIDTypes.ID_NAT_PUNCHTHROUGH_FAILED: //66-客户端NAT穿透失败
								{
									defaultMessageIDType.WriteMsgTypeInfo(peerAddress, peerPort, "客户端NAT穿透失败");
									OnNatPunchthroughFailed(peerAddress, peerPort);
								}
								break;
							case DefaultMessageIDTypes.ID_NAT_TARGET_NOT_CONNECTED: //62-客户端：客户端NAT穿透失败
							case DefaultMessageIDTypes.ID_NAT_TARGET_UNRESPONSIVE: //63-客户端：客户端NAT穿透失败
							case DefaultMessageIDTypes.ID_NAT_CONNECTION_TO_TARGET_LOST: //64-客户端：客户端NAT穿透失败
							case DefaultMessageIDTypes.ID_NAT_ALREADY_IN_PROGRESS: //65-客户端：客户端NAT穿透失败
								{
									defaultMessageIDType.WriteMsgTypeInfo(peerAddress, peerPort, "客户端NAT穿透失败");
									OnConnectionAttemptFailedCommon(peerAddress, peerPort);
								}
								break;
							case DefaultMessageIDTypes.ID_NO_FREE_INCOMING_CONNECTIONS: //20-没有可用的连接，服务器已经达到最大连接数
								{
									defaultMessageIDType.WriteMsgTypeInfo(peerAddress, peerPort, "没有可用的连接，服务器已经达到最大连接数");
									OnNoFreeIncomingConnections(peerAddress, peerPort);
								}
								break;
							case DefaultMessageIDTypes.ID_CONNECTION_ATTEMPT_FAILED: //17-连接尝试失败
								{
									defaultMessageIDType.WriteMsgTypeInfo(peerAddress, peerPort, "连接尝试失败");
									OnConnectionAttemptFailed(peerAddress, peerPort);
								}
								break;
							case DefaultMessageIDTypes.ID_NEW_INCOMING_CONNECTION: //19-服务器中有新连接进来
								{
									defaultMessageIDType.WriteMsgTypeInfo(peerAddress, peerPort, "服务器中有新连接进来");
									OnNewIncomingConnection(peerAddress, peerPort);
								}
								break;
							case DefaultMessageIDTypes.ID_USER_PACKET_ENUM: //134-接收消息
								{
									defaultMessageIDType.WriteMsgTypeInfo(peerAddress, peerPort, "接收消息");
									OnReiceve(peerAddress, peerPort, testPacket.data.Skip(1).ToArray());
								}
								break;
							case DefaultMessageIDTypes.ID_DISCONNECTION_NOTIFICATION: //21-客户端主动断开连接的通知
								{
									defaultMessageIDType.WriteMsgTypeInfo(peerAddress, peerPort, "客户端主动断开连接的通知");
									OnDisconnectionNotification(peerAddress, peerPort);
								}
								break;
							case DefaultMessageIDTypes.ID_CONNECTION_LOST: //22-失去客户端连接
								{
									defaultMessageIDType.WriteMsgTypeInfo(peerAddress, peerPort, "失去客户端连接");
									OnConnectionLost(peerAddress, peerPort);
								}
								break;
							default: //消息标志异常，如果有需要，再另加case处理
								{
									defaultMessageIDType.WriteMsgTypeError("消息标志异常");
								}
								break;
						}
					}
					Thread.Sleep(1);
				}
			});
		}
	}
}
