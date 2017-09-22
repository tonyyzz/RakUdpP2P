using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RakUdpP2P.NatService
{
	class Program
	{
		static void Main(string[] args)
		{
			new NatServerCrowdStore().Do();
			Console.WriteLine("NatServerCrowd启动成功");
			Console.ReadKey();
		}
	}
}
