using RakUdpP2P.BaseCommon.RaknetMng;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RakUdpP2P.UdpProxyConsole
{
	class NatServerCrowd
	{
		private Dictionary<RaknetIPAddress, RaknetUdpNATPTServer> natServerDcit = null;
		public NatServerCrowd()
		{
			natServerDcit = new Dictionary<RaknetIPAddress, RaknetUdpNATPTServer>();
		}
		public void Start(int natServerCount = 1)
		{
			for (int i = 0; i < natServerCount; i++)
			{
				RaknetUdpNATPTServer raknetUdpNATPTServer = new RaknetUdpNATPTServer();
				var udpNATPTServerStarted = raknetUdpNATPTServer.Start();
				if (udpNATPTServerStarted)
				{
					var theNatServerAddress = raknetUdpNATPTServer.GetMyIpAddress();
					if (!natServerDcit.Any(kvPair => kvPair.Key.Address == theNatServerAddress.Address && kvPair.Key.Port == theNatServerAddress.Port))
					{
						natServerDcit.Add(theNatServerAddress, raknetUdpNATPTServer);
					}
				}
			}
		}
		public int GetCount()
		{
			return natServerDcit.Count();
		}
		public List<RaknetIPAddress> GetAddressList()
		{
			return natServerDcit.Keys.ToList();
		}
		public void Stop(Action beforeAction = null)
		{
			beforeAction?.Invoke();
			natServerDcit.ToList().ForEach(kvPair =>
			{
				kvPair.Value.Stop();
			});
		}
	}
}
