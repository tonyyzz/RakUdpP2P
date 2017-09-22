using RakUdpP2P.BaseCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RakUdpP2P.UdpProxyConsole
{
	class NatServerCrowdTest
	{
		public void Do()
		{
			NatServerCrowd natServerCrowd = new NatServerCrowd();
			natServerCrowd.Start();
			int count = natServerCrowd.GetCount();
			var list = natServerCrowd.GetAddressList();
			Console.WriteLine("NatServer IP列表：");
			list.ForEach(item =>
			{
				Console.WriteLine(item.ToString());
			});

			Console.WriteLine("外网地址：{0}", IPAddressUtils.GetOuterNatIP());
		}
	}
}
