using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RakUdpP2P.BaseCommon.RaknetMng
{
	/// <summary>
	/// Raknet统一代理类（综合ProxyCoordinator和ProxyServer）
	/// </summary>
	public class RaknetUdpProxy
	{
		private RaknetUdpProxyCoordinator raknetUdpProxyCoordinator = null;
		private RaknetUdpProxyServer raknetUdpProxyServer = null;
		public RaknetUdpProxy()
		{
			raknetUdpProxyCoordinator = new RaknetUdpProxyCoordinator();
			raknetUdpProxyServer = new RaknetUdpProxyServer();
		}
		public bool Start(RaknetIPAddress localAddress = null, ushort maxConnCount = ushort.MaxValue)
		{
			var proxyCoordinatorStarted = raknetUdpProxyCoordinator.Start(localAddress, maxConnCount);
			if (proxyCoordinatorStarted)
			{
				var proxyServerStarted = raknetUdpProxyServer.Start().Connect(raknetUdpProxyCoordinator.GetMyIpAddress());
				if (proxyServerStarted)
				{
					return true;
				}
			}
			return false;
		}
		public RaknetIPAddress GetMyIpAddress()
		{
			return raknetUdpProxyCoordinator.GetMyIpAddress();
		}
		public void Stop()
		{
			raknetUdpProxyCoordinator.Stop(() =>
			{
				raknetUdpProxyServer.Stop();
			});
		}
	}
}
