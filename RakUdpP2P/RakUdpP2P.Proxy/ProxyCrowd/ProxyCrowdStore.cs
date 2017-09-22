using RakUdpP2P.BaseCommon;
using RakUdpP2P.DAL;
using RakUdpP2P.Model;
using RakUdpP2P.Model.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RakUdpP2P.Proxy
{
	public class ProxyCrowdStore
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
				//存到数据库
				NatProxyModel natProxyModel = new NatProxyModel
				{
					Type = (int)NatProxyTypeEnum.Proxy,
					AddressInnerNet = item.Address,
					Port = item.Port,
					IsUsable = true,
					StartUpTime = DateTime.Now,
					AddressOuterNet = IPAddressUtils.GetOuterNatIP()
				};
				var id = BaseDAL.Insert(natProxyModel);
				proxyCrowd.AddIdItem(id);

				Console.WriteLine("启动成功！PrimaryId：{0}，AddressInnerNetAndPort：{1}:{2}", id, natProxyModel.AddressInnerNet, natProxyModel.Port);

			});


			ConsoleCloseHandler.proxyCrowd = proxyCrowd;
			ConsoleCloseHandler.SetConsoleCtrlHandler(ConsoleCloseHandler.cancelHandler, true);
		}
	}
}
