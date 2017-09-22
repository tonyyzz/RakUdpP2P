using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.IO;

namespace RakUdpP2P.BaseCommon
{
	public class INIBase
	{
		[DllImport("kernel32")]
		private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
		[DllImport("kernel32")]
		private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

		protected string m_filepath;

		public INIBase(string filepath)
		{
			m_filepath = Path.Combine(Directory.GetCurrentDirectory(), filepath);
		}

		//! 写ini
		//! Section 字段
		//! Key key
		//! Value value
		public void IniWriteValue(string Section, string Key, string Value)
		{
			WritePrivateProfileString(Section, Key, Value, this.m_filepath);
		}

		//! 读ini
		//! Section 字段
		//! Key key
		public string IniReadValue(string Section, string Key)
		{
			StringBuilder temp = new StringBuilder(255);
			int i = GetPrivateProfileString(Section, Key, "", temp, 255, this.m_filepath);
			return temp.ToString();
		}

		public bool ExistINIFile()
		{
			return File.Exists(m_filepath);
		}
	}
}
