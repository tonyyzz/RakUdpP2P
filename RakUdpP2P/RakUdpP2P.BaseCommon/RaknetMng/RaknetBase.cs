using RakNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RakUdpP2P.BaseCommon.RaknetMng
{
	public class RaknetBase
	{
		protected event Action<string, ushort> OnConnectionRequestAccepted;
		protected event Action<string, ushort> OnNewIncomingConnection;
		protected event Action<string, ushort, byte[]> OnRaknetReceive;
		protected event Action<string, ushort> OnDisconnectionNotification;
		protected event Action<string, ushort> OnConnectionLost;
		protected event Action<string, ushort> OnNatPunchthroughSucceeded;
		protected event Action<string, ushort> OnNatPunchthroughFailed;
		protected event Action<string, ushort> OnConnectionAttemptFailed;
		protected event Action<string, ushort> OnConnectionAttemptFailedCommon;
		protected event Action<string, ushort> OnNoFreeIncomingConnections;
		protected event Action<string, ushort> OnIncompatibleProtocolVersion;
		protected event Action<string, ushort> OnFcm2NewHost;
		protected event Action<string, ushort, byte> OnNatTypeDetectionRequest;
		protected event Action<string, ushort> OnUnconnectedPing;
		protected event Action<string, ushort> OnUnconnectedPingOpenConnections;
		protected event Action<string, ushort> OnUnconnectedPong;
		protected event Action<string, ushort, byte> OnUdpProxyGeneral;

		protected RakPeerInterface rakPeer = null;
		protected bool isThreadRunning = true;

		protected RaknetBase()
		{
			RaknetCSRunTest.JudgeRaknetCanRun();
			rakPeer = RakPeerInterface.GetInstance();
			EventRegister();
		}

		protected RaknetIPAddress GetMyAddress()
		{
			SystemAddress systemAddress = rakPeer.GetMyBoundAddress();
			return new RaknetIPAddress(systemAddress.ToString(false), systemAddress.GetPort());
		}

		protected ulong GetMyRaknetGUID()
		{
			return rakPeer.GetMyGUID().g;
		}

		protected void EventRegister()
		{
			OnConnectionRequestAccepted += Method_OnConnectionRequestAccepted;
			OnNewIncomingConnection += Method_OnNewIncomingConnection;
			OnRaknetReceive += Method_OnRaknetReceive;
			OnDisconnectionNotification += Method_OnDisconnectionNotification;
			OnConnectionLost += Method_OnConnectionLost;
			OnNatPunchthroughSucceeded += Method_OnNatPunchthroughSucceeded;
			OnNatPunchthroughFailed += Method_OnNatPunchthroughFailed;
			OnConnectionAttemptFailed += Method_OnConnectionAttemptFailed;
			OnNoFreeIncomingConnections += Method_OnNoFreeIncomingConnections;
			OnIncompatibleProtocolVersion += Method_OnIncompatibleProtocolVersion;
			OnConnectionAttemptFailedCommon += Method_OnConnectionAttemptFailedCommon;
			OnFcm2NewHost += Method_OnFcm2NewHost;
			OnNatTypeDetectionRequest += Method_OnNatTypeDetectionRequest;
			OnUnconnectedPing += Method_OnUnconnectedPing;
			OnUnconnectedPingOpenConnections += RaknetBase_OnUnconnectedPingOpenConnections;
			OnUnconnectedPong += Method_OnUnconnectedPong;
			OnUdpProxyGeneral += Method_OnUdpProxyGeneral;
		}

		private void RaknetBase_OnUnconnectedPingOpenConnections(string address, ushort port) { }
		private void Method_OnConnectionRequestAccepted(string address, ushort port) { }
		private void Method_OnPortUsed(string address, ushort port) { }
		private void Method_OnUdpProxyGeneral(string address, ushort port, byte typeByte) { }
		private void Method_OnUnconnectedPong(string address, ushort port) { }
		private void Method_OnUnconnectedPing(string address, ushort port) { }
		private void Method_OnNatTypeDetectionRequest(string address, ushort port, byte typeByte) { }
		private void Method_OnFcm2NewHost(string address, ushort port) { }
		private void Method_OnConnectionAttemptFailedCommon(string address, ushort port) { }
		private void Method_OnIncompatibleProtocolVersion(string address, ushort port) { }
		private void Method_OnNoFreeIncomingConnections(string address, ushort port) { }
		private void Method_OnConnectionAttemptFailed(string address, ushort port) { }
		private void Method_OnNatPunchthroughFailed(string address, ushort port) { }
		private void Method_OnNatPunchthroughSucceeded(string address, ushort port) { }
		private void Method_OnRaknetReceive(string address, ushort port, byte[] datas) { }
		private void Method_OnConnectionLost(string address, ushort port) { }
		private void Method_OnDisconnectionNotification(string address, ushort port) { }
		private void Method_OnNewIncomingConnection(string address, ushort port) { }

		protected void ReceiveThreadStart()
		{
			ThreadPool.QueueUserWorkItem(o =>
			{
				Run();
			});
		}

		private void Run()
		{
			isThreadRunning = true;

			string peerAddress = "";
			ushort peerPort = 0;
			Packet testPacket = null;

			while (true)
			{
				Thread.Sleep(1);
				if (!isThreadRunning)
				{
					break;
				}
				if (rakPeer == null)
				{
					continue;
				}
				using (testPacket = rakPeer.Receive())
				{
					if (testPacket == null || testPacket.data.Count() <= 0)
					{
						continue;
					}
					DefaultMessageIDTypes defaultMessageIDType = (DefaultMessageIDTypes)testPacket.data[0];
					peerAddress = testPacket.systemAddress.ToString(false);
					peerPort = testPacket.systemAddress.GetPort();
					switch (defaultMessageIDType)
					{
						case DefaultMessageIDTypes.ID_UNCONNECTED_PING: //1
							{
								defaultMessageIDType.WriteMsgTypeInfo(rakPeer, peerAddress, peerPort, " [ OnUnconnectedPing ]");
								OnUnconnectedPing(peerAddress, peerPort);
							}
							break;
						case DefaultMessageIDTypes.ID_UNCONNECTED_PING_OPEN_CONNECTIONS: //2
							{
								defaultMessageIDType.WriteMsgTypeInfo(rakPeer, peerAddress, peerPort, " [ OnUnconnectedPingOpenConnections ]");
								OnUnconnectedPingOpenConnections(peerAddress, peerPort);
							}
							break;
						case DefaultMessageIDTypes.ID_UNCONNECTED_PONG: //28
							{
								defaultMessageIDType.WriteMsgTypeInfo(rakPeer, peerAddress, peerPort, " [ OnUnconnectedPong ]");
								OnUnconnectedPong(peerAddress, peerPort);
							}
							break;
						case DefaultMessageIDTypes.ID_UDP_PROXY_GENERAL: //92  ID_UDP_PROXY_GENERAL类型
							{
								defaultMessageIDType.WriteMsgTypeInfo(rakPeer, peerAddress, peerPort, "ID_UDP_PROXY_GENERAL类型 [ OnUdpProxyGeneral ]");
								OnUdpProxyGeneral(peerAddress, peerPort, testPacket.data[1]);
							}
							break;
						case DefaultMessageIDTypes.ID_NAT_TYPE_DETECTION_REQUEST: //96 得到路由类型结果
							{
								defaultMessageIDType.WriteMsgTypeInfo(rakPeer, peerAddress, peerPort, "得到路由类型结果 [ OnNatTypeDetectionRequest ]");
								OnNatTypeDetectionRequest(peerAddress, peerPort, testPacket.data[1]);
							}
							break;
						case DefaultMessageIDTypes.ID_FCM2_NEW_HOST: //82
							{
								defaultMessageIDType.WriteMsgTypeInfo(rakPeer, peerAddress, peerPort, " [ OnFcm2NewHost ]");
								OnFcm2NewHost(peerAddress, peerPort);
							}
							break;
						case DefaultMessageIDTypes.ID_INCOMPATIBLE_PROTOCOL_VERSION: //25 - 协议版本不兼容，一般是由于两端AttachPlugin没有对应
							{
								defaultMessageIDType.WriteMsgTypeInfo(rakPeer, peerAddress, peerPort, "协议版本不兼容，一般是由于两端AttachPlugin没有对应 [ OnIncompatibleProtocolVersion ]");
								OnIncompatibleProtocolVersion(peerAddress, peerPort);
							}
							break;
						case DefaultMessageIDTypes.ID_NAT_TARGET_NOT_CONNECTED: //62-客户端：客户端NAT穿透失败
						case DefaultMessageIDTypes.ID_NAT_TARGET_UNRESPONSIVE: //63-客户端：客户端NAT穿透失败
						case DefaultMessageIDTypes.ID_NAT_CONNECTION_TO_TARGET_LOST: //64-客户端：客户端NAT穿透失败
						case DefaultMessageIDTypes.ID_NAT_ALREADY_IN_PROGRESS: //65-客户端：客户端NAT穿透失败
							{
								defaultMessageIDType.WriteMsgTypeInfo(rakPeer, peerAddress, peerPort, "客户端NAT穿透失败 [ OnConnectionAttemptFailedCommon ]");
								OnConnectionAttemptFailedCommon(peerAddress, peerPort);
							}
							break;
						case DefaultMessageIDTypes.ID_NAT_PUNCHTHROUGH_FAILED: //66-客户端NAT穿透失败
							{
								defaultMessageIDType.WriteMsgTypeInfo(rakPeer, peerAddress, peerPort, "客户端NAT穿透失败 [ OnNatPunchthroughFailed ]");
								OnNatPunchthroughFailed(peerAddress, peerPort);
							}
							break;
						case DefaultMessageIDTypes.ID_NAT_PUNCHTHROUGH_SUCCEEDED: //67-客户端NAT穿透成功
							{
								defaultMessageIDType.WriteMsgTypeInfo(rakPeer, peerAddress, peerPort, "客户端NAT穿透成功 [ OnNatPunchthroughSucceeded ]");
								OnNatPunchthroughSucceeded(peerAddress, peerPort);
							}
							break;
						case DefaultMessageIDTypes.ID_CONNECTION_REQUEST_ACCEPTED: //16-客户端：连接请求被接受
							{
								defaultMessageIDType.WriteMsgTypeInfo(rakPeer, peerAddress, peerPort, "连接请求被接受 [ OnConnectionRequestAccepted ]");
								OnConnectionRequestAccepted(peerAddress, peerPort);
							}
							break;
						case DefaultMessageIDTypes.ID_CONNECTION_ATTEMPT_FAILED: //17-连接尝试失败
							{
								defaultMessageIDType.WriteMsgTypeInfo(rakPeer, peerAddress, peerPort, "连接尝试失败 [ OnConnectionAttemptFailed ]");
								OnConnectionAttemptFailed(peerAddress, peerPort);
							}
							break;
						case DefaultMessageIDTypes.ID_NEW_INCOMING_CONNECTION: //19-服务器中有新连接进来
							{
								defaultMessageIDType.WriteMsgTypeInfo(rakPeer, peerAddress, peerPort, "服务器中有新连接进来 [ OnNewIncomingConnection ]");
								OnNewIncomingConnection(peerAddress, peerPort);
							}
							break;
						case DefaultMessageIDTypes.ID_NO_FREE_INCOMING_CONNECTIONS: //20-没有可用的连接，服务器已经达到最大连接数
							{
								defaultMessageIDType.WriteMsgTypeInfo(rakPeer, peerAddress, peerPort, "没有可用的连接，服务器已经达到最大连接数 [ OnNoFreeIncomingConnections ]");
								OnNoFreeIncomingConnections(peerAddress, peerPort);
							}
							break;
						case DefaultMessageIDTypes.ID_DISCONNECTION_NOTIFICATION: //21-客户端主动断开连接的通知
							{
								defaultMessageIDType.WriteMsgTypeInfo(rakPeer, peerAddress, peerPort, "客户端主动断开连接的通知 [ OnDisconnectionNotification ]");
								OnDisconnectionNotification(peerAddress, peerPort);
							}
							break;
						case DefaultMessageIDTypes.ID_CONNECTION_LOST: //22-失去客户端连接
							{
								defaultMessageIDType.WriteMsgTypeInfo(rakPeer, peerAddress, peerPort, "失去客户端连接 [ OnConnectionLost ]");
								OnConnectionLost(peerAddress, peerPort);
							}
							break;
						case DefaultMessageIDTypes.ID_USER_PACKET_ENUM: //134-接收消息
							{
								defaultMessageIDType.WriteMsgTypeInfo(rakPeer, peerAddress, peerPort, "接收消息 [ OnReiceve ]");
								if (testPacket.data.Count() > 1) //过滤掉通过proxy发送过来的用来保持连接的消息
								{
									OnRaknetReceive(peerAddress, peerPort, testPacket.data.Skip(1).ToArray());
								}
							}
							break;
						default: //消息标志异常，如果有需要，再另加case处理
							{
								defaultMessageIDType.WriteMsgTypeError(string.Format(@"[{0}:{1}] [{2}:{3}] 消息标志异常", rakPeer.GetMyBoundAddress().ToString(false), rakPeer.GetMyBoundAddress().GetPort(), peerAddress, peerPort));
							}
							break;
					}
				}
			}
		}
	}
}
