using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RakUdpP2P.BaseCommon.RaknetMng
{
	public class RaknetIPAddress
	{
		private RaknetIPAddress() { }
		public RaknetIPAddress(string address, ushort port)
		{
			_address = address;
			_port = port;
		}
		private string _address = "";
		private ushort _port = 0;

		public string Address
		{
			get { return _address; }
		}
		public ushort Port
		{
			get { return _port; }
		}

		public override string ToString()
		{
			return string.Format(@"{0}:{1}", _address, _port);
		}
	}
}
