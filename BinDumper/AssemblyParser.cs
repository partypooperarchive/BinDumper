/*
 * Created by SharpDevelop.
 * User: User
 * Date: 31.10.2021
 * Time: 13:21
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.IO;
using Mono.Cecil;

namespace BinDumper
{
	/// <summary>
	/// Description of AssemblyParser.
	/// </summary>
	public class AssemblyParser
	{
		private AssemblyDefinition assembly = null;
		private NumberFormatInfo nfi = null;
		private TypedefResolver td_resolver = null;
		
		private HashSet<string> disabled_derivation_classes = null;
		
		private const string token_attrib_name = "TokenAttribute";
		
		private int level = 0;
		
		public AssemblyParser(string filename)
		{
			nfi = new NumberFormatInfo();
			nfi.NumberDecimalSeparator = ".";
			
			var resolver = new DefaultAssemblyResolver();
			resolver.AddSearchDirectory(Path.GetDirectoryName(filename));
			
			assembly = AssemblyDefinition.ReadAssembly(filename, new ReaderParameters { AssemblyResolver = resolver });
			
			td_resolver = new TypedefResolver(assembly);
			
			disabled_derivation_classes = new HashSet<string>();
			
			WriteLine("Assembly {0} loaded", assembly);
		}
		
		public void SetDerivationEnabled(string class_name, bool mode) {
			if (mode) {
				disabled_derivation_classes.Remove(class_name);
			} else {
				disabled_derivation_classes.Add(class_name);
			}
		}
		
		public string ParseFile(string filename, string classname, string mode) {
			var cls = td_resolver.FindClassByPrefixedName(classname);
			
			if (cls == null) {
				throw new InvalidDataException(string.Format("Class {0} not found!", classname));
			}
			
			WriteLine("Parsing file {0} for class {1} in {2} mode", filename, cls.FullName, mode);
				
			var reader = new DeReader(filename);
			
			var multiple = !mode.Equals("s");
			
			if (multiple) {			
				ulong length = 0;
				
				if (mode[0] == 'l')
					length = reader.ReadVarUInt();
				else
					length = 1;
				
				WriteLine("Number of elements in file: {0}", length);
			
				var items = new List<string>();
				
				var string_key = assembly.MainModule.ImportReference(typeof(string));
			
				if (mode.Equals("ld")) {
					for (uint i = 0; i < length; i++) {
						items.Add(ParseDictionary(reader, string_key, cls));
					}
			
					return "[" + string.Join(",", items) + "]";
				} else if (mode.Equals("l")) {
					for (uint i = 0; i < length; i++) {
						items.Add(ParseClass(cls, reader));
					}
			
					return "[" + string.Join(",", items) + "]";
				} else if (mode.Equals("d")) {
					return ParseDictionary(reader, string_key, cls);
				} else if (mode.Equals("dl")) {
					var arr_value = assembly.MainModule.ImportReference(new ArrayType(cls));
					return ParseDictionary(reader, string_key, arr_value);
				} else {
					throw new InvalidDataException(string.Format("Invalid mode {0}", mode));
				}
			} else {
				return ParseClass(cls, reader);
			}
		}
		
		private string ParseDictionary(DeReader reader, TypeReference key_type, TypeReference value_type) {
			var size = reader.ReadVarUInt();
			
			WriteLine("Dict size: {0}", size);
			
			var items = new List<string>();
			
			for (uint i = 0; i < size; i++) {
				var key = ParseFieldType(key_type, reader);
				
				WriteLine("Parsing key {0} with type {1}", key, value_type.FullName);
				
				items.Add(string.Format(
					"{0}: {1}", key, ParseFieldType(value_type, reader) //ParseClass(t, reader)
				));
			}
			
			return "{" + string.Join(",", items) + "}";
		}
		
		/*private ulong GetOffset(TypeReference t) {			
			return t.Resolve().IsSealed ? 0x1000000000ul : 0ul;
		}*/
		
		private string ParseClass(TypeReference t, DeReader reader) {
			level++;
			var output = ParseClassWithDerivation(t, reader);
			level--;
			return "{" + string.Join(",", output) + "}";
		}
		
		private ICollection<string> ParseClassWithDerivation(TypeReference t, DeReader reader) {
			TypeReference derived_class = null;
			
			var classes = GetDerivedClasses(t);
			
			var root_classname = t.FullName;
			
			if (classes.Count() > 0 && !disabled_derivation_classes.Contains(t.Name.Split('.').Last())) // TODO: hack!
			{
				var class_id = reader.ReadVarUInt();
					
				// If class_id == 0, that's the current (base) class

				//derived_class = td_resolver.FindClassById(t.FullName, (int)class_id);
				root_classname = td_resolver.GetBasestBase(t);
				derived_class = td_resolver.FindClassById(root_classname, (int)class_id);
					
				WriteLine("Deriving class {0} ({1})", class_id, derived_class == null ? t.FullName : derived_class.FullName);
			}
			
			var output = new List<string>();
			
			if (derived_class != null) {
				output.Add(string.Format("\"$type\": \"{0}\"", derived_class.Name));
				output.AddRange(ParseClassInt(derived_class, reader, root_classname));
			} else {
				output.Add(string.Format("\"$type\": \"{0}\"", t.Name));
				
				// Try to find most base class registered in our hierarchy
				root_classname = td_resolver.GetBasestBase(t);
				
				output.AddRange(ParseClassInt(t, reader, root_classname));
			}
			
			return output;
		}
		
		private ICollection<string> ParseClassInt(TypeReference t, DeReader reader, string hier_bottom_class_name = null) {
			var output = new List<string>();
			
			var tr = t.Resolve();
			
			if (hier_bottom_class_name != null && !tr.FullName.Equals(hier_bottom_class_name) && tr.BaseType != null && !tr.BaseType.FullName.Equals(typeof(object).FullName)) {
				output.AddRange(ParseClassInt(tr.BaseType, reader, hier_bottom_class_name));
			}
			
			var fields = GetAllProperties(tr).ToArray();
			
			WriteLine("Parsing Class {0} ({1} fields)", t.FullName, fields.Length);
			
			var hashes = new HashSet<string>();
			
			if (fields.Length > 0) {
				BitMask bm = new BitMask(reader, fields.Length <= 8);
			
				for (int i = 0, j = 0; i < fields.Length && j < fields.Length; i++) {
					var f = fields[i];
					
					//var ft = f.PropertyType;
					var ft = f.FieldType;
				
					if (bm.TestBit(j)) {
						
						Write("Field (#{0}) {1}, type {2} = ", j, f.Name, ft.Name);	
						var ret = ParseFieldType(ft, reader);
						WriteLine();
					
						output.Add(string.Format("\"{0}\": {1}", TransformName(f.Name), ret));
					} else {
						WriteLine("Skipping field (#{0}) {1}", j, f.Name);
					}
				
					// HACK: two hash fields are treated like one
					if (!f.Name.EndsWith("HashSuffix")) {
						j++;
					}
					
					/*if (f.Name.EndsWith("HashPre") || f.Name.EndsWith("HashSuffix")) { 
						if (hashes.Contains(TransformName(f.Name, true))) {
							j++;
						} else {
							hashes.Add(TransformName(f.Name, true));
						}
					} else {
						j++;
					}*/
				}
			}
			
			return output;
		}
		
		private string ParseFieldType(TypeReference ft, DeReader reader) {
			if (ft.IsArray) {
				var arr = ft as ArrayType;

				var items = new List<string>();
				
				ulong length = 0;
				
				// TODO: dirty hack!
				//if (IsUnsignedCount(arr.ElementType) || arr.ElementType.FullName.EndsWith("MoleMole.Config.ElementType")) {
					length = reader.ReadVarUInt();
				//} else {
				//	length = (ulong)reader.ReadVarInt();
				//}
				
				Write("({0}) [", length);
				
				level++;
				
				for (uint i = 0; i < length; i++) {
					items.Add(ParseFieldType(arr.ElementType, reader));
					Write(" ");
				}
				
				level--;
				
				Write("]");
				
				return "[" + string.Join(",", items) + "]";
			} else if (ft.Resolve().IsEnum) {
				long value = 0;
				if (IsEnumSigned(ft.Resolve()))
					value = reader.ReadVarInt();
				else
					value = (long)reader.ReadVarUInt();
				var s_value = td_resolver.ParseEnumValue(ft.Resolve(), value);
				Write(s_value);
				return "\"" + s_value.ToString() + "\"";
			} else if (ft.FullName.Equals(typeof(string).FullName)) {
				var value = reader.ReadString();
				Write("{0}", value);
				return "\"" + value.ToString() + "\"";
			} else if (ft.FullName.Equals(typeof(uint).FullName)) {
				var value = reader.ReadVarUInt();
				Write(value);
				return value.ToString();
			} else if (ft.FullName.Equals(typeof(Int64).FullName)) {
				var value = reader.ReadVarInt();
				Write(value);
				return value.ToString();
			} else if (ft.FullName.Equals(typeof(Int32).FullName)) {
				var value = reader.ReadVarInt();
				Write(value);
				return value.ToString();
			} else if (ft.FullName.Equals(typeof(UInt64).FullName)) {
				var value = reader.ReadVarUInt();
				Write(value);
				return value.ToString();
			} else if (ft.FullName.Equals(typeof(UInt32).FullName)) {
				var value = reader.ReadVarUInt();
				Write(value);
				return value.ToString();
			} else if (ft.FullName.Equals(typeof(UInt16).FullName)) {
				var value = reader.ReadVarUInt();
				Write(value);
				return value.ToString();
			} else if (ft.FullName.Equals(typeof(byte).FullName) || ft.FullName.EndsWith("SimpleSafeUInt8")) {
				//var value = reader.ReadVarInt();
				var value = reader.ReadU8();
				Write(value);
				return value.ToString();
			} else if (ft.FullName.Equals(typeof(bool).FullName) || ft.FullName.EndsWith("FixedBool")) {
				var value = reader.ReadBool();
				Write(value);
				return value.ToString().ToLower();
			} else if (ft.FullName.Equals(typeof(Single).FullName)) {
				var value = reader.ReadF32();
				Write(value);
				return value.ToString(nfi);
			} else if (ft.FullName.Equals(typeof(Double).FullName)) {
				var value = reader.ReadF64();
				Write(value);
				return value.ToString(nfi);
			} else if (ft.FullName.EndsWith("SimpleSafeFloat")) {
				var value = reader.ReadF32();
				Write(value);
				return value.ToString(nfi);
			} else if (ft.FullName.EndsWith("DynamicFloat")) {
				var value = ReadDynamicFloat(reader);
				Write(value);
				return value;
			} else if (ft.FullName.EndsWith("DynamicInt")) {
				var value = ReadDynamicInt(reader);
				Write(value);
				return value;
			} else if (ft.FullName.EndsWith("DynamicArgument")) {
				var value = ReadDynamicArgument(reader);
				Write(value);
				return value;
			} else if (ft.FullName.EndsWith("DynamicString")) {
				bool isDynamic = reader.ReadBool();
				if (isDynamic) {
					// keke
				}
				var value = reader.ReadString();
				Write(value);
				return "\"" + value + "\"";
			} else if (ft.FullName.EndsWith("SimpleSafeUInt32")) {
				var value = reader.ReadVarUInt();
				Write(value);
				return value.ToString();
			} else if (ft.FullName.EndsWith("SimpleSafeUInt16")) {
				var value = reader.ReadVarUInt();
				Write(value);
				return value.ToString();
			} else if (ft.FullName.EndsWith("SimpleSafeInt32")) {
				var value = reader.ReadVarInt();
				Write(value);
				return value.ToString();
			} else if (ft.FullName.EndsWith("SimpleSafeInt16")) {
				var value = reader.ReadVarInt();
				Write(value);
				return value.ToString();
			} else if (ft.FullName.StartsWith("MoleMole.Config.")) {
				return ParseClass(ft.Resolve(), reader);
			} else if (ft.Name.EndsWith("HashSet`1")) {
				// Treat it as an array
				var dt = ft as GenericInstanceType;
				var e_type = dt.GenericArguments[0];
				
				var items = new List<string>();
				
				ulong length = 0;
				
				length = reader.ReadVarUInt();
				
				Write("({0}) [", length);
				
				level++;
				
				for (uint i = 0; i < length; i++) {
					items.Add(ParseFieldType(e_type, reader));
					Write(" ");
				}
				
				level--;
				
				Write("]");
				
				return "[" + string.Join(",", items) + "]";
			} else if (ft.Name.EndsWith("Dictionary`2")) {
				var dt = ft as GenericInstanceType;
				var key_type = dt.GenericArguments[0];
				var value_type = dt.GenericArguments[1];
				level++;
				var d = ParseDictionary(reader, key_type, value_type);	
				level--;
				return d;
			} else {
				throw new InvalidOperationException(string.Format("Type {0} is not supported", ft.FullName));
			}
		}
		
		private string ReadDynamicArgument(DeReader reader) {
			// Credit goes to Raz
			var type_index = reader.ReadVarUInt();
			switch (type_index) {
				case 1:
					return reader.ReadS8().ToString();
				case 2:
					return reader.ReadU8().ToString();
				case 3:
					return reader.ReadS16().ToString();
				case 4:
					return reader.ReadU16().ToString();
				case 5:
					return reader.ReadS32().ToString();
				case 6:
					return reader.ReadU32().ToString();
				case 7:
					return reader.ReadF32().ToString(nfi);
				case 8:
					return reader.ReadF64().ToString(nfi);
				case 9:
					return reader.ReadBool().ToString().ToLower();
				case 10:
					return "\"" + reader.ReadString() + "\"";
				default:
					throw new InvalidDataException(string.Format("Unhandled DynamicArgument type {0}!", type_index));
			}
		}
		
		private string ReadDynamicInt(DeReader reader) {
			// Credit goes to Raz
			bool isString = reader.ReadBool();
			
			if (isString) {
				return "\"" + reader.ReadString() + "\"";
			} else {
				return reader.ReadVarInt().ToString();
			}
		}
		
		private string ReadDynamicFloat(DeReader reader) {
			// Credit goes to Raz
			bool isFormula = reader.ReadBool();
			
			if (isFormula) {
				long count = reader.ReadVarInt();
				
				var components = new List<string>();
				
				for (int i = 0; i < count; i++) {
					bool isOperator = reader.ReadBool();
					
					if (isOperator) {
						long op = reader.ReadVarInt();
						string s_op = td_resolver.GetDynamicFloatOperator(op);
						components.Add("\"" + s_op + "\"");
					} else {
						bool isString = reader.ReadBool();
						
						if (isString) {
							string s = reader.ReadString();
							components.Add("\"" + s + "\"");
						} else {
							float f = reader.ReadF32();
							components.Add(f.ToString(nfi));
						}
					}
				}
				
				return "[" + string.Join(",", components) + "]"; 
			} else {
				bool isString = reader.ReadBool();
						
				if (isString) {
					string s = reader.ReadString();
					return "\"" + s + "\"";
				} else {
					float f = reader.ReadF32();
					return f.ToString(nfi);
				}
			}
			
			throw new DivideByZeroException("You fucked up");
		}
		
		private bool IsUnsignedCount(TypeReference t) {
			return t.FullName.Equals(typeof(string).FullName) ||
			    t.FullName.Equals(typeof(uint).FullName) ||
				t.FullName.Equals(typeof(Int64).FullName) ||
				t.FullName.Equals(typeof(Int32).FullName) ||
				t.FullName.Equals(typeof(UInt16).FullName) ||
			    t.FullName.Equals(typeof(byte).FullName) ||
			    t.FullName.Equals(typeof(bool).FullName) ||
				t.FullName.Equals(typeof(Single).FullName) ||
			    t.FullName.EndsWith("SimpleSafeFloat") || 
			    t.FullName.EndsWith("SimpleSafeUInt32") ||
				t.FullName.EndsWith("SimpleSafeUInt16") ||
				t.FullName.EndsWith("SimpleSafeInt32") ||
				(t.Resolve().IsEnum && !IsEnumSigned(t.Resolve())) ||
				t.IsArray;
		}
		
		private bool IsEnumSigned(TypeDefinition t) {		
			foreach (var field in t.Fields) {
				if (field.Name == "value__") {
					var n = field.FieldType.FullName;
					// Possible underlying types: byte, sbyte, short, ushort, int, uint, long, ulong
					return (
						n.Equals(typeof(sbyte).FullName) ||
						n.Equals(typeof(short).FullName) ||
						n.Equals(typeof(int).FullName) ||
						n.Equals(typeof(long).FullName)
					);
				}
			}
			
			throw new ArgumentException("Unable to determine signedness of enum {0}!", t.FullName);
		}
		
		private string TransformName(string name, bool transform_hashes = false) {
			if (name.StartsWith("_"))
				name = name.TrimStart('_'); //name.Substring(1);
			
			if (char.IsLower(name[0]))
				name = char.ToUpper(name[0]) + name.Substring(1);
			
			if (name.EndsWith("RawNum"))
				name = name.Substring(0, name.Length - "RawNum".Length);
			
			if (name.EndsWith("TextMapHash"))
				name = name.Substring(0, name.Length - "TextMapHash".Length);
			
			if (transform_hashes) {
				if (name.EndsWith("HashPre"))
					name = name.Substring(0, name.Length - "Pre".Length);
				
				if (name.EndsWith("HashSuffix"))
					name = name.Substring(0, name.Length - "Suffix".Length);
			}
			
			return name;
		}
		
		/*private IEnumerable<PropertyReference> GetAllProperties(TypeDefinition t) {
			var props = new List<PropertyReference>();
			
			props.AddRange(
				t.Properties.Where(p => p.SetMethod != null)
			);
			
			//if (t.BaseType != null)
				//props.AddRange(GetAllProperties(t.BaseType.Resolve()));
			
			return props;
		}*/
		
		private IEnumerable<FieldReference> GetAllProperties(TypeDefinition t) {
			// We only need fields backed up by properties
			var props = new HashSet<string>(t.Properties.Where(p => p.SetMethod != null).Select(p => TransformName(p.Name, true)));
			
			var fields = new List<FieldReference>();
			
			/*
			foreach (var p in t.Fields) {
				Console.WriteLine("{0} {1} {2} {3} {4}", 
				                  (p.IsPrivate || p.Name.EndsWith("TextMapHash")),
				               !p.IsStatic,
				               !IsNotSerialized(p),
				               props.Contains(TransformName(p.Name)),
				               TransformName(p.Name)
				              );
			}*/
			
			fields.AddRange(
				t.Fields.Where(p => (p.IsPrivate || p.Name.EndsWith("TextMapHash")) && // TODO: WTF???
				               !p.IsStatic && 
				               !IsNotSerialized(p) && 
				               (props.Contains(TransformName(p.Name, true)) || p.Name.IsBeeObfuscated() || p.Name.EndsWith("TextMapHash"))) // TODO: add obfuscated properties and hope for the best
			);
			
			return fields;
		}
		
		private IEnumerable<TypeReference> GetDerivedClasses(TypeReference t) {
			return assembly.MainModule.Types.Where(x => t.Equals(x.BaseType));
		}
		
		private uint GetToken(TypeDefinition t)
		{
			foreach (var attrib in t.CustomAttributes)
			{
				if (attrib.AttributeType.Name == token_attrib_name)
				{
					var token = attrib.Fields[0].Argument.Value.ToString();
					return Convert.ToUInt32(token, 16);
				}
			}
			
			throw new ArgumentException();
		}
		
		private bool IsNotSerialized(IMemberDefinition def) {
			foreach (var attrib in def.CustomAttributes)
			{
				if (attrib.AttributeType.Name == "NonSerializedAttribute")
				{
					return true;
				}
				
				if (attrib.AttributeType.Name == "AttributeAttribute")
				{
					foreach (var f in attrib.Fields) {
						if (f.Name == "Name" && f.Argument.Value != null && f.Argument.Value.ToString().Equals("CompilerGeneratedAttribute")) {
							return true;
						}
					}
				}
			}
			
			return false;
		}
		
		private void WriteLine() {
			WriteLine("", null);
		}
		
		private void Write(ulong u) {
			Write(u.ToString(), null);
		}
		
		private void Write(byte u) {
			Write(u.ToString(), null);
		}
		
		private void Write(bool u) {
			Write(u.ToString(), null);
		}
		
		private void Write(float u) {
			Write(u.ToString(), null);
		}
		
		private void Write(double u) {
			Write(u.ToString(), null);
		}
		
		private void Write(string format, params object[] m) {
			#if DEBUG
			Console.Write("".PadLeft(level*2) + format, m);
			#endif
		}
		
		private void WriteLine(string format, params object[] m) {
			#if DEBUG
			Console.WriteLine("".PadLeft(level*2) +format, m);
			#endif
		}
	}
}
