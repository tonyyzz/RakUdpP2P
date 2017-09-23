using RakUdpP2P.BaseCommon.RaknetMng;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace RakUdpP2P.PeerServer
{
	public class ConsoleCloseHandler
	{
		public static RaknetUdpPeerServer raknetUdpPeerServer = null;

		public delegate bool ControlCtrlDelegate(int CtrlType);
		[DllImport("kernel32.dll")]
		public static extern bool SetConsoleCtrlHandler(ControlCtrlDelegate HandlerRoutine, bool Add);
		public static ControlCtrlDelegate cancelHandler = new ControlCtrlDelegate(HandlerRoutine);

		public static bool HandlerRoutine(int CtrlType)
		{

			raknetUdpPeerServer.Stop();

			//switch (CtrlType)
			//{
			//	case 0:
			//		Console.WriteLine("0工具被强制关闭"); //Ctrl+C关闭
			//		testClient.CloseConnection(new AddressOrGUID(new SystemAddress("127.0.0.1", serverPort)), true);
			//		Thread.Sleep(10);
			//		RakPeerInterface.DestroyInstance(testClient);
			//		break;
			//	case 2:
			//		Console.WriteLine("2工具被强制关闭");//按控制台关闭按钮关闭
			//		testClient.CloseConnection(new AddressOrGUID(new SystemAddress("127.0.0.1", serverPort)), true);
			//		Thread.Sleep(10);
			//		RakPeerInterface.DestroyInstance(testClient);
			//		break;
			//}
			return false;
		}
	}
}
