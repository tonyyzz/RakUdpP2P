using RakUdpP2P.BaseCommon.RaknetMng;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RakUdpP2P.Proxy
{
	public class ProxyCrowd
	{
		private List<int> idList = null;
		private Dictionary<RaknetIPAddress, RaknetUdpProxy> proxyDcit = null;
		public ProxyCrowd()
		{
			idList = new List<int>();
			proxyDcit = new Dictionary<RaknetIPAddress, RaknetUdpProxy>();
		}
		public void Start(int proxyCount = 1)
		{
			for (int i = 0; i < proxyCount; i++)
			{
				RaknetUdpProxy raknetUdpProxy = new RaknetUdpProxy();
				var proxyStarted = raknetUdpProxy.Start();
				if (proxyStarted)
				{
					var theProxyAddress = raknetUdpProxy.GetMyIpAddress();
					if (!proxyDcit.Any(kvPair => kvPair.Key.Address == theProxyAddress.Address && kvPair.Key.Port == theProxyAddress.Port))
					{
						proxyDcit.Add(theProxyAddress, raknetUdpProxy);
					}
				}
			}
		}

		public void AddIdItem(int proxyId)
		{
			idList.Add(proxyId);
		}
		public List<int> GetIdList()
		{
			return idList;
		}
		public int GetCount()
		{
			return proxyDcit.Count();
		}
		public List<RaknetIPAddress> GetAddressList()
		{
			return proxyDcit.Keys.ToList();
		}
		public void Stop(Action beforeAction = null)
		{
			beforeAction?.Invoke();
			proxyDcit.ToList().ForEach(kvPair =>
			{
				kvPair.Value.Stop();
			});
		}
	}
}
