using RakNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RakUdpP2P.BaseCommon.RaknetMng
{
	public class RaknetUdpPeerClient : RaknetBase
	{
		private NatPunchthroughClient natPunchthroughClient = null;
		//private NatTypeDetectionClient natTypeDetectionClient = null;

		private RaknetAddress _peerServerAddress = null;
		private ulong _udpPeerServerGuid = 0;
		public RaknetUdpPeerClient()
		{
			natPunchthroughClient = new NatPunchthroughClient();
			//natTypeDetectionClient = new NatTypeDetectionClient();
		}

		public RaknetUdpPeerClient Start(RaknetAddress localAddress = null, ushort maxConnCount = ushort.MaxValue)
		{
			rakPeer.AttachPlugin(natPunchthroughClient);
			//rakPeer.AttachPlugin(natTypeDetectionClient);
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
			}
			return this;
		}

		public bool Connect(RaknetAddress peerServerAddress, ulong udpPeerServerGuid)
		{
			_peerServerAddress = peerServerAddress;
			_udpPeerServerGuid = udpPeerServerGuid;
			var connectResult = rakPeer.Connect(RaknetConfig.natServerAddress.Address, RaknetConfig.natServerAddress.Port,
				RaknetConfig.natServerPwd, RaknetConfig.natServerPwdLength);
			if (connectResult == ConnectionAttemptResult.CONNECTION_ATTEMPT_STARTED)
			{
				//natTypeDetectionClient.DetectNATType(new SystemAddress(RaknetConfig.natServerAddress.Address, RaknetConfig.natServerAddress.Port));
				OnConnectionRequestAccepted += RaknetUdpPeerClient_OnConnectionRequestAccepted;
				OnNatPunchthroughSucceeded += RaknetUdpPeerClient_OnNatPunchthroughSucceeded;
				ReceiveThreadStart();
				return true;
			}
			return false;
		}

		private void RaknetUdpPeerClient_OnConnectionRequestAccepted(string address, ushort port)
		{
			if (address == RaknetConfig.natServerAddress.Address && port == RaknetConfig.natServerAddress.Port)
			{
				//OpenNAT
				RakNetGUID peerServerRakNetGUID = new RakNetGUID(_udpPeerServerGuid);
				RakNetGUID myGuid = rakPeer.GetMyGUID();
				if (myGuid.g != _udpPeerServerGuid)
				{
					natPunchthroughClient.OpenNAT(peerServerRakNetGUID, new SystemAddress(address, port));
				}
			}
			else
			{
				//通过nat连接成功后，立即断开与natServer的连接
				rakPeer.CloseConnection(new AddressOrGUID(new SystemAddress(RaknetConfig.natServerAddress.Address, RaknetConfig.natServerAddress.Port)), true);
			}
		}

		private void RaknetUdpPeerClient_OnNatPunchthroughSucceeded(string arg1, ushort arg2)
		{
			//穿透成功后连接peerServer
			rakPeer.Connect(_peerServerAddress.Address, _peerServerAddress.Port, "", 0);
		}
	}
}
