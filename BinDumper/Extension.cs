/*
 * Created by SharpDevelop.
 * User: User
 * Date: 13.04.2022
 * Time: 15:56
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Linq;

namespace BinDumper
{
	/// <summary>
	/// Description of Extension.
	/// </summary>
	public static class Extension
	{		
		public static bool IsBeeObfuscated(this string name) {
			// TODO: very simple but should work
			return name.All(char.IsUpper) && (name.Length >= 10 && name.Length <= 15);
		}
	}
}
