﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RakUdpP2P.Proxy
{
	class Program
	{
		static void Main(string[] args)
		{
			new ProxyCrowdStore().Do();
			Console.WriteLine("ProxyCrowd启动成功");
			Console.ReadKey();
		}
	}
}
