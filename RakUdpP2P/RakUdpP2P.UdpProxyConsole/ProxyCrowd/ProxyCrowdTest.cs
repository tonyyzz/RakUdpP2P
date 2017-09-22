using RakUdpP2P.BaseCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RakUdpP2P.UdpProxyConsole
{
	class ProxyCrowdTest
	{
		public void Do()
		{
			ProxyCrowd proxyCrowd = new ProxyCrowd();
			proxyCrowd.Start();
			int count = proxyCrowd.GetCount();
			var list = proxyCrowd.GetAddressList();
			Console.WriteLine("代理IP列表：");
			list.ForEach(item =>
			{
				Console.WriteLine(item.ToString());
			});

			Console.WriteLine("外网地址：{0}", IPAddressUtils.GetOuterNatIP());
		}
	}
}
