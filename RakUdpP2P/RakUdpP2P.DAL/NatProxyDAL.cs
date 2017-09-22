using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace RakUdpP2P.DAL
{
	public class NatProxyDAL : BaseDAL
	{
		public static bool Remove(int proxyId)
		{
			using (var Conn = GetConn())
			{
				Conn.Open();
				string sql = string.Format(@"delete from NatProxy where Id = {0}", proxyId);
				return Conn.Execute(sql) > 0;
			}
		}
	}
}
