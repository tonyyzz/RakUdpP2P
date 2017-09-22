using RakUdpP2P.BaseCommon;
using RakUdpP2P.DAL;
using RakUdpP2P.Model;
using RakUdpP2P.Model.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RakUdpP2P.NatService
{
	class NatServerCrowdStore
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
				//存到数据库
				NatProxyModel natProxyModel = new NatProxyModel
				{
					Type = (int)NatProxyTypeEnum.Nat,
					AddressInnerNet = item.Address,
					Port = item.Port,
					IsUsable = true,
					StartUpTime = DateTime.Now,
					AddressOuterNet = IPAddressUtils.GetOuterNatIP()
				};
				var id = BaseDAL.Insert(natProxyModel);
				natServerCrowd.AddIdItem(id);

				Console.WriteLine("启动成功！PrimaryId：{0}，AddressInnerNetAndPort：{1}:{2}", id, natProxyModel.AddressInnerNet, natProxyModel.Port);

			});


			ConsoleCloseHandler.natServerCrowd = natServerCrowd;
			ConsoleCloseHandler.SetConsoleCtrlHandler(ConsoleCloseHandler.cancelHandler, true);
		}
	}
}
