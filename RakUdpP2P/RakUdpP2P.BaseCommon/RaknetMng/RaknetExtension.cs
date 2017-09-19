using RakNet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RakUdpP2P.BaseCommon.RaknetMng
{
	public static class RaknetExtension
	{
		public static string GetMessageIDTypeStr(this DefaultMessageIDTypes defaultMessageIDType)
		{
			return string.Format(@"[{0}-{1}]", (int)defaultMessageIDType, defaultMessageIDType.ToString());
		}
		public static void WriteMsgTypeInfo(this DefaultMessageIDTypes defaultMessageIDType, RakPeerInterface rakPeer, string address, ushort port, string msg)
		{
			Console.ForegroundColor = ConsoleColor.Green;
			string msgStr = string.Format(@"[{0}:{1}]-[{2}]-[{3}:{4}]-{5}：{6}",
				rakPeer.GetMyBoundAddress().ToString(false),
				rakPeer.GetMyBoundAddress().GetPort(),
				DateTime.Now.GetDefaultFormat(),
				address,
				port,
				defaultMessageIDType.GetMessageIDTypeStr(),
				msg);
			Console.WriteLine(msgStr);
			Console.ResetColor();
			Debug.WriteLine(msgStr);
		}

		public static void WriteMsgTypeError(this DefaultMessageIDTypes defaultMessageIDType, string errorMsg)
		{
			Console.ForegroundColor = ConsoleColor.Yellow;
			string msgStr = string.Format(@"{0} - {1}  {2}", defaultMessageIDType.GetMessageIDTypeStr(), errorMsg, DateTime.Now.GetDefaultFormat());
			Console.WriteLine(msgStr);
			Console.ResetColor();
			Debug.WriteLine(msgStr);
		}

		public static string GetDefaultFormat(this DateTime time)
		{
			return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
		}


		public static void WriteWarning(string msg)
		{
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.WriteLine(msg);
			Console.ResetColor();
			Debug.WriteLine(msg);
			throw new Exception(msg);
		}
	}
}
