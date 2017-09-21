using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RakUdpP2P.BaseCommon.RaknetMng
{
	public class RaknetConfig
	{
		public static string COORDINATOR_PASSWORD = "#$$#4745$%*(fghfg%^*&%^45";

		public static RaknetIPAddress natServerAddress = new RaknetIPAddress("127.0.0.1", 50001);
		public static RaknetIPAddress coordinatorAddress = new RaknetIPAddress("127.0.0.1", 50002);

		public static string natServerPwd = "";
	}
}
