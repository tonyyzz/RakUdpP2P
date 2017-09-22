using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RakUdpP2P.BaseCommon
{
	public class IPAddressUtils
	{
		public static string GetOuterNatIP(string url = "http://1212.ip138.com/ic.asp")
		{
			//重复获取n次
			int index = 1;
			string ipStr = "";
			while (index <= 10)
			{
				try
				{
					Uri uri = new Uri(url);
					System.Net.HttpWebRequest req = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(uri);
					req.Method = "get";
					using (Stream s = req.GetResponse().GetResponseStream())
					{
						using (StreamReader reader = new StreamReader(s))
						{
							char[] ch = { '[', ']' };
							string str = reader.ReadToEnd();
							System.Text.RegularExpressions.Match m = System.Text.RegularExpressions.Regex.Match(str, @"\[(?<IP>[0-9\.]*)\]");
							ipStr = m.Value.Trim(ch);
							break;
						}
					}
				}
				catch (Exception)
				{
					index += 1;
				}
			}
			return ipStr;
		}
	}
}
