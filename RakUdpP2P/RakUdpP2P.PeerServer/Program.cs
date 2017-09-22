using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RakUdpP2P.PeerServer
{
	class Program
	{
		static void Main(string[] args)
		{
			new Test().Do();
			Console.WriteLine("按任意键退出");
			Console.ReadKey();
		}
	}
}
