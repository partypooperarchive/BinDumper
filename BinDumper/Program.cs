/*
 * Created by SharpDevelop.
 * User: User
 * Date: 31.10.2021
 * Time: 13:19
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.IO;

namespace BinDumper
{
	class Program
	{
		const string assembly_name = "Assembly-CSharp.dll";
		const string default_mode = "s";
		
		public static void Main(string[] args) 
		{
			#if DEBUG
			DebugMain(args);
			#else
			ReleaseMain(args);
			#endif
		}
		
		public static void ReleaseMain(string[] args)
		{
			string dll_path = null;
			string filename = null;
			string output_file = null;
			string mode = default_mode;
			string class_name = null;
			bool derivation = true;
			
			if (args.Length < 1)
			{
				Usage();
				return;
			}
			dll_path = args[0];
			
			var parser = new AssemblyParser(Path.Combine(dll_path, assembly_name));
			
			string line;
			
			while ((line = Console.ReadLine()) != null && line != "") {
				try {
					var data = line.Split();
					
					filename = data[0];
					output_file = data[1];
					
					if (data.Length > 2)
						mode = data[2];
					
					if (data.Length > 3)
						class_name = data[3];
					
					if (class_name == null)
						class_name = DeriveClassName(filename);
			
					if (class_name == null)
						throw new ArgumentNullException("Failed to derive classname! Please specify manually");
					
					string output = null;
				
					if (mode.EndsWith("-")) {
						mode = mode.Substring(0, mode.Length-1);
						derivation = false;
					}
					
					if (mode != "ld" && mode != "s" && mode != "l" && mode != "d" && mode != "dl")
						throw new ArgumentException(string.Format("Mode {0} is invalid!", mode));
					
					Console.WriteLine("Parsing {0} => {1} as {2} ({3})", filename, output_file, class_name, mode);
					
					parser.SetDerivationEnabled(class_name, derivation);
					output = parser.ParseFile(filename, class_name, mode);		
					parser.SetDerivationEnabled(class_name, true);					
			
					File.WriteAllText(output_file, output);
				} catch (Exception e) {
					Console.WriteLine("Exception: {0}", e);
					Console.WriteLine("Stacktrace: {0}", e.StackTrace);
					Console.WriteLine("At {0}", e.TargetSite);
					Console.WriteLine();
				}
			}
		}
		
		public static void DebugMain(string[] args)
		{
			string dll_path = null;
			string filename = null;
			string output_file = null;
			string mode = default_mode;
			string class_name = null;
			bool derivation = true;
			
			#if DEBUG
			//dll_path = @"Z:\Il2CppDumper\OUT-2.2.0-dev\DummyDll";
			dll_path = @"Z:\Il2CppDumper\OUT-2.2.0-Rel\DummyDll";
			//filename = @"Z:\DD\ConfigAbility_Amber.bin";
			//filename = @"Z:\DD\ConfigAbility_Avatar_Albedo.bin";
			//filename = @"Z:\DD\ConfigAbility_Avatar_Eula.bin";
			//filename = @"Z:\DD\ConfigMonster_Hili_None_01";
			//filename = @"Z:\DD\ConfigMonster_Oceanid_Boar_02";
			filename = @"Z:\DD\LevelMetaData"; class_name = "ConfigLevelMeta"; mode = "s";
			output_file = @"Z:\DD\output.json";
			#else
			Console.WriteLine("THIS IS A TRIAL VERSION!");
			Console.WriteLine("To acquire a full-featured build, send your ID to <recruit@kgb.su> and wait for further instructions");
			Console.WriteLine("");
			
			if (args.Length < 3)
			{
				Usage();
				return;
			}
			dll_path = args[0];
			filename = args[1];
			output_file = args[2];

			if (args.Length > 3)
				mode = args[3];

			if (args.Length > 4)
				class_name = args[4];
			#endif
			
			if (class_name == null)
				class_name = DeriveClassName(filename);
			
			if (class_name == null)
				throw new ArgumentNullException("Failed to derive classname! Please specify manually");
			
			var parser = new AssemblyParser(Path.Combine(dll_path, assembly_name));
			
			string output = null;
			
			if (mode.EndsWith("-")) {
				mode = mode.Substring(0, mode.Length-1);
				derivation = false;
			}
			
			if (mode != "ld" && mode != "s" && mode != "l" && mode != "d" && mode != "dl")
				throw new ArgumentException(string.Format("Mode {0} is invalid!", mode));
			
			parser.SetDerivationEnabled(class_name, derivation);
			output = parser.ParseFile(filename, class_name, mode);		
			parser.SetDerivationEnabled(class_name, true);			
			
			File.WriteAllText(output_file, output);
			
			#if DEBUG
			Console.Write("Press any key to continue . . . ");
			Console.ReadKey(true);
			#endif
		}
		
		public static void Usage() {
			var param_string = "\t{0,-15} {1}";
			
			var usage = string.Join(
				Environment.NewLine,
				"BinData dumper tool",
				"",
				"Usage:",
				string.Format("\t{0} input_dir input_file output_file [mode [class_name]]", AppDomain.CurrentDomain.FriendlyName),
				"",
				"Parameters:",
				string.Format(param_string, "input_dir", "Directory where Assembly-CSharp.dll is located"),
				string.Format(param_string, "input_file", "Binary input file (decrypted)"),
				string.Format(param_string, "output_file", "Path to the output file (beware of overwriting!)"),
				string.Format(param_string, "mode", "Mode of parsing:"),
				string.Format(param_string, "", "s\tSingle object in file"),
				string.Format(param_string, "", "l\tList of objects in file"),
				string.Format(param_string, "", "d\tDictionary of objects in file"),
				string.Format(param_string, "", "ld\tList of Dictionaries of objects in file"),
				string.Format(param_string, "", "dl\tDictionary of Lists of objects in file"),
				string.Format(param_string, "", string.Format("Defaults to '{0}'", default_mode)),
				string.Format(param_string, "class_name", "Name of the class to deserialize"),
				string.Format(param_string, "", string.Format("If omitted, tool will try to derive it from input_file (and may fail)")),
				""
			);
			Console.WriteLine(usage);
		}
		
		public static string DeriveClassName(string filepath) {
			var filename = Path.GetFileNameWithoutExtension(filepath);
			var idx = filename.IndexOf('_');
			
			if (idx < 0)
				return null;
			
			return filename.Substring(0, idx);
		}
	}
}