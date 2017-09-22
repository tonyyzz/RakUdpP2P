using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using RakUdpP2P.BaseCommon;

namespace RakUdpP2P.DAL
{
	public class BaseDAL
	{
		private static string _connStr
		{
			get
			{
				INIBase ib = new INIBase("config.ini");
				string ip = ib.IniReadValue("sql", "ip");
				string port = ib.IniReadValue("sql", "port");
				string psw = ib.IniReadValue("sql", "psw");
				var connStr = string.Format(@"server={0};user={1};database={2};port={3};password={4};Charset=utf8;",
					ip, "root", "myworld", port, psw);
				return connStr;
			}
		}
		private static IDbConnection _conn = new MySqlConnection(_connStr);

		///// <summary>
		///// 数据库连接对象
		///// </summary>
		protected static IDbConnection Conn { get { return _conn; } }

		/// <summary>
		/// 获取数据库连接对象实例
		/// </summary>
		/// <param name="sqlType">数据库类型，默认mysql</param>
		/// <returns></returns>
		protected static IDbConnection GetConn(int sqlType = 1)
		{
			IDbConnection conn = null;
			switch (sqlType)
			{
				case 1: //mysql数据库
					{
						conn = new MySqlConnection(_connStr);
					}
					break;
				//case 2: //sqlserver数据库
				//	{
				//	}
				//	break;
				default: break;
			}
			return conn;
		}

		/// <summary>
		/// 初始化指定表
		/// </summary>
		/// <returns></returns>
		public static bool TruncateTable<T>(T model) where T : class, new()
		{
			using (var Conn = GetConn())
			{
				Conn.Open();
				string sql = string.Format("truncate table {0};",
					model.GetType().Name.Substring(0, model.GetType().Name.LastIndexOf("Model")));
				return Conn.Execute(sql) > 0;
			}
		}

		/// <summary>
		/// 统一插入方法（使用注意：model类名必须以‘Model’结尾，并且model属性名称和个数必须与数据库表字段名称和个数一致。如果不一致，请使用其中的一个重载方法 T^T）
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="model"></param>
		/// <returns>自增主键Id</returns>
		public static int Insert<T>(T model) where T : class
		{
			using (var Conn = GetConn())
			{
				Conn.Open();
				var propKeys = model.GetAllPropKeys();
				var names = string.Join(",", propKeys.ToArray());
				var values = string.Join(",", propKeys.ToList().ConvertAll(m => "@" + m).ToArray());
				string sql = string.Format(@"insert into {0}({1}) values({2});select @@IDENTITY;",
					model.GetType().Name.Substring(0, model.GetType().Name.LastIndexOf("Model")),
					names,
					values);
				var task = Conn.ExecuteScalarAsync(sql, model);
				return Convert.ToInt32(task.Result);
			}
		}
		/// <summary>
		/// 统一插入方法，只判断是否插入成功（使用注意：model类名必须以‘Model’结尾，并且model属性名称和个数必须与数据库表字段名称和个数一致。如果不一致，请使用其中的一个重载方法 T^T）
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="model"></param>
		/// <returns>自增主键Id</returns>
		public static bool InsertSuccess<T>(T model) where T : class
		{
			using (var Conn = GetConn())
			{
				Conn.Open();
				var propKeys = model.GetAllPropKeys();
				var names = string.Join(",", propKeys.ToArray());
				var values = string.Join(",", propKeys.ToList().ConvertAll(m => "@" + m).ToArray());
				string sql = string.Format(@"insert into {0}({1}) values({2});",
					model.GetType().Name.Substring(0, model.GetType().Name.LastIndexOf("Model")),
					names,
					values);
				return Conn.Execute(sql, model) > 0;
			}
		}
		/// <summary>
		/// 统一插入方法（使用注意：model类名必须以‘Model’结尾，fieldStrs为指明要插入的表字段字符串集合，字段集合可以包含所有，也可以是一部分）
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="model"></param>
		/// <param name="fieldStrs">要插入的字段字符串集合，以半角逗号‘,’分割</param>
		/// <returns>自增主键Id</returns>
		public static int Insert<T>(T model, string fieldStrs) where T : class
		{
			using (var Conn = GetConn())
			{
				Conn.Open();
				string sql = string.Format(@"insert into {0}({1}) values ({2});select @@IDENTITY;",
				model.GetType().Name.Substring(0, model.GetType().Name.LastIndexOf("Model")),
				fieldStrs,
				string.Join(",", fieldStrs.Split(',').ToList().ConvertAll(m => "@" + m.Trim()).ToArray()));
				var task = Conn.ExecuteScalarAsync(sql, model);
				return Convert.ToInt32(task.Result);
			}
		}

		/// <summary>
		/// 统一插入方法，只判断是否插入成功（使用注意：model类名必须以‘Model’结尾，fieldStrs为指明要插入的表字段字符串集合，字段集合可以包含所有，也可以是一部分）
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="model"></param>
		/// <param name="fieldStrs">要插入的字段字符串集合，以半角逗号‘,’分割</param>
		/// <returns>自增主键Id</returns>
		public static bool InsertSuccess<T>(T model, string fieldStrs) where T : class
		{
			using (var Conn = GetConn())
			{
				Conn.Open();
				string sql = string.Format(@"insert into {0}({1}) values ({2});",
				model.GetType().Name.Substring(0, model.GetType().Name.LastIndexOf("Model")),
				fieldStrs,
				string.Join(",", fieldStrs.Split(',').ToList().ConvertAll(m => "@" + m.Trim()).ToArray()));
				return Conn.Execute(sql, model) > 0;
			}
		}

		/// <summary>
		/// 批量插入
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="list"></param>
		/// <returns></returns>
		public static bool BatchInsert<T>(List<T> list) where T : class, new()
		{
			if (!list.Any())
			{
				return false;
			}
			T model = new T();
			var props = model.GetAllPropKeys();
			string names = string.Join(",", props.ToArray());

			List<string> valAllLi = new List<string>();
			foreach (var item in list)
			{
				var itemProps = item.GetType().GetProperties();
				List<string> valList = new List<string>();
				foreach (var itemProp in itemProps)
				{
					var val = itemProp.GetValue(item);
					string valStr = "";
					var typeName = itemProp.PropertyType.FullName;
					switch (typeName)
					{
						case "System.String":
						case "System.DateTime":
							{
								valStr = "'" + val + "'";
							}
							break;
						default:
							{
								valStr = val.ToString();
							}
							break;
					}
					valList.Add(valStr);
				}
				string vals = string.Join(",", valList.ToArray());
				valAllLi.Add("(" + vals + ")");
			}

			string values = string.Join(",", valAllLi.ToArray());
			string sql = string.Format(@"insert into {0} ({1}) values {2}",
				model.GetType().Name.Substring(0, model.GetType().Name.LastIndexOf("Model")),
				names,
				values);

			using (var Conn = GetConn())
			{
				Conn.Open();
				return Conn.Execute(sql) > 0;
			}
		}
	}
}
