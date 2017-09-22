using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{
	public static class ModelHelper
	{
		/// <summary>
		/// 获取类的所有属性名称
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="model"></param>
		/// <returns></returns>
		public static IEnumerable<string> GetAllPropKeys<T>(this T model) where T : class
		{
			return model.GetType().GetProperties().ToList().Select(m => m.Name);
		}
	}
}
