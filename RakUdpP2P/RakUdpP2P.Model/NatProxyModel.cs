using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RakUdpP2P.Model
{
	public class NatProxyModel
	{
		/// <summary>
		/// 
		/// </summary>
		public NatProxyModel()
		{
			Id = 0;
			Type = 0;
			AddressOuterNet = "";
			AddressInnerNet = "";
			Port = 0;
			StartUpTime = new DateTime(1900, 1, 1);
			IsUsable = false;
		}
		/// <summary>
		/// 主键Id
		/// </summary>
		public int Id { get; set; }
		/// <summary>
		/// 类型（1：Nat，2：Proxy）
		/// </summary>
		public int Type { get; set; }
		/// <summary>
		/// 外网IpAddress
		/// </summary>
		public string AddressOuterNet { get; set; }
		/// <summary>
		/// 内网IpAddress
		/// </summary>
		public string AddressInnerNet { get; set; }
		/// <summary>
		/// 端口
		/// </summary>
		public int Port { get; set; }
		/// <summary>
		/// 启动时间
		/// </summary>
		public DateTime StartUpTime { get; set; }
		/// <summary>
		/// 是否可用
		/// </summary>
		public bool IsUsable { get; set; }
	}
}
