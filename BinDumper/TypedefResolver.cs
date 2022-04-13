/*
 * Created by SharpDevelop.
 * User: User
 * Date: 31.10.2021
 * Time: 13:49
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Cecil;

namespace BinDumper
{
	/// <summary>
	/// Description of TypedefResolver.
	/// </summary>
	public class TypedefResolver
	{
		private AssemblyDefinition assembly = null;
		
		private List<List<TypeReference>> type_mapping = null;
		
		private Dictionary<string, int> class_to_dict_mapping = null;
		
		private TypeDefinition dynamic_float_operator = null;
		
		private HashSet<TypeReference> known_classes = null;
		
		public TypedefResolver(AssemblyDefinition assembly)
		{
			this.assembly = assembly;
			
			class_to_dict_mapping = new Dictionary<string, int>();
			
			type_mapping = new List<List<TypeReference>>();
		
			known_classes = new HashSet<TypeReference>();
			
			dynamic_float_operator = GetDynamicFloatOperatorEnum();
			
			if (dynamic_float_operator == null) {
				throw new MissingMemberException("Failed to retrieve dynamic float operator class!");
			}
			
			TypeReference type_registration = assembly.MainModule.Types.First(t => t.FullName.EndsWith("TypeTool"));
			
			if (type_registration == null) {
				throw new MissingMemberException("Failed to retrieve type registration class!");
			}
			
			var fields = type_registration.Resolve().Fields;
			
			var classes = new List<TypeReference>();
			
			for (int i = 2; i < fields.Count; i++) { // Skip first two fields, they are not relevant for us
				var f = fields[i];
				
				if (f.Name.EndsWith("TypeDict")) {					
					// New "Type dictionary"
					var gt = f.FieldType as GenericInstanceType;
					var current_base = gt.GenericArguments[1];

					class_to_dict_mapping.Add(current_base.FullName, type_mapping.Count);
					
					type_mapping.Add(new List<TypeReference>());
					type_mapping[type_mapping.Count-1].Add(current_base);
					known_classes.Add(current_base);
				} else if (f.Name.EndsWith("Register")) {
					var field_type_name = f.FieldType.FullName;
					var real_type_name = field_type_name.Substring(0, field_type_name.Length - "Register".Length);
					
					var real_type = FindClassByName(real_type_name);
					
					if (real_type == null) {
						throw new InvalidOperationException(string.Format("Failed to resolve type {0}!", real_type_name));
					}
					
					classes.Add(real_type);
					known_classes.Add(real_type);
				} else {
					throw new ArgumentException(string.Format("Unknown field {0}", f.FullName));				
				}
			}
			
			/*for (int i = 0; i < classes.Count; i++) {
				classes[i].Reverse();
			}*/
			
			classes.Reverse();
			
			while (classes.Count > 0) {
				for (int i = 0; i < type_mapping.Count; i++) {
					var bases = new List<TypeReference>(type_mapping[i]);
					
					foreach (var b in bases) {
						for (int j = classes.Count-1; j >= 0; j--) {
							var c = classes[j].Resolve();
							if (c.BaseType.Equals(b)) {
								type_mapping[i].Add(classes[j]);
								classes.RemoveAt(j);
							}
						}
					}
				}
			}
			
			/*bool cont = true;
			
			while (cont) {
				cont = false;
				for (int i = 0; i < type_mapping.Count; i++) {
					var bases = new List<TypeReference>(type_mapping[i]);
					
					foreach (var b in bases) {
						var ts = GetDerivedTypes(b);
						var filtered_ts = ts.Where(t => !bases.Contains(t));
						
						if (filtered_ts.Count() > 0) {
							cont = true;
							type_mapping[i].AddRange(filtered_ts);
						}
					}
				}
			}*/
			
			ApplyHacks();
			
			Console.WriteLine("Found {0} type dictionaries with {1} types", 
			                  class_to_dict_mapping.Count, type_mapping.Select(x => x.Count).Sum());
		}
		
		public void _TypedefResolver(AssemblyDefinition assembly)
		{
			this.assembly = assembly;
			
			class_to_dict_mapping = new Dictionary<string, int>();
			
			type_mapping = new List<List<TypeReference>>();
		
			dynamic_float_operator = GetDynamicFloatOperatorEnum();
			
			if (dynamic_float_operator == null) {
				throw new MissingMemberException("Failed to retrieve dynamic float operator class!");
			}
			
			TypeReference type_registration = assembly.MainModule.Types.First(t => t.FullName.EndsWith("TypeTool"));
			
			if (type_registration == null) {
				throw new MissingMemberException("Failed to retrieve type registration class!");
			}
			
			var fields = type_registration.Resolve().Fields;
			
			TypeReference current_base = null;
			
			var miss = new List<List<TypeReference>>();
			
			var available_bases = new HashSet<TypeReference>();
			
			for (int i = 2; i < fields.Count; i++) { // Skip first two fields, they are not relevant for us
				var f = fields[i];
				
				if (f.Name.EndsWith("TypeDict")) {					
					// New "Type dictionary"
					var gt = f.FieldType as GenericInstanceType;
					current_base = gt.GenericArguments[1];

					class_to_dict_mapping.Add(current_base.FullName, type_mapping.Count);
					type_mapping.Add(new List<TypeReference>());
					type_mapping[class_to_dict_mapping.Count-1].Add(current_base);
					
					miss.Add(new List<TypeReference>());
					
					available_bases.Add(current_base);
				} else if (f.Name.EndsWith("Register")) {
					var field_type_name = f.FieldType.FullName;
					var real_type_name = field_type_name.Substring(0, field_type_name.Length - "Register".Length);
					
					var real_type = FindClassByName(real_type_name);
					
					if (real_type == null) {
						throw new InvalidOperationException(string.Format("Failed to resolve type {0}!", real_type_name));
					}
					
					if (current_base.Equals(real_type.BaseType)) {
						type_mapping[class_to_dict_mapping.Count-1].Add(real_type);
						available_bases.Add(real_type);
					} else
						miss[class_to_dict_mapping.Count-1].Add(real_type);
				} else {
					throw new ArgumentException(string.Format("Unknown field {0}", f.FullName));				
				}
			}
			
			#if false
			for (int i = 0; i < type_mapping.Count; i++)
				type_mapping[i].AddRange(miss[i]);
			#else
			while (miss.Select(l => l.Count).DefaultIfEmpty().Max() > 0) {
				var new_bases = new HashSet<TypeReference>();
				
				for (int i = 0; i < type_mapping.Count; i++) {
					var candidates = new List<TypeReference>();
					//for (int j = miss[i].Count-1; j >= 0; j--) {
					for (int j = 0; j < miss[i].Count; j++) {
						if (available_bases.Contains(miss[i][j].Resolve().BaseType)) {
							candidates.Add(miss[i][j]);
							new_bases.Add(miss[i][j]);
							//available_bases.Add(miss[i][j]);
							//miss[i].RemoveAt(j);
						}
					}
					foreach (var c in candidates) {
						miss[i].Remove(c);
					}
					
					//candidates.Reverse();
					type_mapping[i].AddRange(candidates);
				}
				
				available_bases.UnionWith(new_bases);
			}
			#endif
			
			Console.WriteLine("Found {0} type dictionaries with {1} types", 
			                  class_to_dict_mapping.Count, type_mapping.Select(x => x.Count).Sum());
		}
		
		public string GetDynamicFloatOperator(long id) {
			return ParseEnumValue(dynamic_float_operator, id);
		}
		
		public string ParseEnumValue(TypeDefinition t, long value) {
			foreach (var field in t.Fields) {
				if (field.Name == "value__")
					continue;
				
				string s_value = value.ToString();
				
				if (field.Constant.ToString().Equals(s_value)) {
					return field.Name;
				}
			}
			
			throw new InvalidDataException(string.Format("Failed to resolve value {0} for enum {1}", value, t.FullName));
		}
		
		public TypeDefinition FindClassByName(string classname) {
			return assembly.MainModule.Types.FirstOrDefault(t => t.FullName.EndsWith(classname));
		}
		
		public TypeDefinition FindClassByPrefixedName(string classname) {
			// TODO: decompiler behaves all over the place...
			// First, try to find full name
			var cls = assembly.MainModule.Types.FirstOrDefault(t => t.Name.EndsWith("MoleMole.Config."+classname));
			if (cls != null) 
				return cls;
			// Next, try to find just "classname" with correct namespace
			cls = assembly.MainModule.Types.FirstOrDefault(t => t.Name.EndsWith(classname) && t.Namespace.Equals("MoleMole.Config"));
			
			return cls;
		}
		
		public TypeReference FindClassById(string base_class, int type_index) {
			//if (type_mapping.ContainsKey(type_index))
			//	return FindClassByName(type_mapping[type_index]);
			if (class_to_dict_mapping.ContainsKey(base_class)) {
				var idx = class_to_dict_mapping[base_class];
				var list = type_mapping[idx];
				
				if (list.Count > type_index) {
					return list[type_index];
				}
			}
			
			// TODO: This is kind of hacky, but...
			/*string basest_base = GetBasestBase(FindClassByName(base_class));
			
			if (!basest_base.Equals(base_class))
				return FindClassById(basest_base, type_index);
			
			throw new MissingMemberException(string.Format("Derived class for {0} (id {1}) not found!", base_class, type_index));*/
			return null;
		}
		
		public ulong GetConstructorAddress(TypeReference t) {
			//const string attr_name = "AddressAttribute";
			//const string val_name = "Offset";
			const string attr_name = "TokenAttribute";
			const string val_name = "Token";
			
			//var constructors = t.Resolve().Methods.Where(m => m.IsConstructor).ToList();
			//var attrs = constructors[0].CustomAttributes;
			var attrs = t.Resolve().CustomAttributes;
			
			var addr_attr = attrs.Where(a => a.AttributeType.Name.EndsWith(attr_name)).FirstOrDefault();
			
			var offset = addr_attr.Fields.Where(f => f.Name.EndsWith(val_name)).FirstOrDefault();
			
			var off_str = offset.Argument.Value.ToString();
			
			return Convert.ToUInt64(off_str, 16);
		}
		
		private TypeDefinition GetDynamicFloatOperatorEnum() {
			var df_class = assembly.MainModule.Types.First(t => t.FullName.EndsWith("DynamicFloat"));
			
			if (df_class == null)
				return null;
			
			var op_value_class = df_class.NestedTypes.First(t => t.FullName.EndsWith("OperatorValue"));
			
			if (op_value_class == null)
				return null;
			
			var df_op_enum = op_value_class.NestedTypes.First(t => t.FullName.EndsWith("Operator") && t.IsEnum);
			
			return df_op_enum;
		}
		
		private IEnumerable<TypeReference> GetDerivedTypes(TypeReference b) {
			return assembly.MainModule.Types.Where(t => b.Equals(t.BaseType));
		}
		
		private IEnumerable<TypeReference> GetDerivedTypesRecursively(TypeReference b) {
			var derived = GetDerivedTypes(b);
			var result = new List<TypeReference>(derived);
			
			foreach (var d in derived) {
				result.AddRange(GetDerivedTypesRecursively(d));
			}
			
			return result;
		}
		
		private void ApplyHacks() {
			// Swap CreateGadget and ReviveDeadAvatar
			var tmp = type_mapping[1][255];
			type_mapping[1][255] = type_mapping[1][256];
			type_mapping[1][256] = tmp;
			
			// Move ConfigWindmill and ConfigLocalTrigger to the end
			tmp = type_mapping[50][17]; type_mapping[50].RemoveAt(17); type_mapping[50].Add(tmp);
			tmp = type_mapping[50][17]; type_mapping[50].RemoveAt(17); type_mapping[50].Add(tmp);
		}
		
		public string GetBasestBase(TypeReference t) {
			string b = t.FullName;
			var td = t.Resolve();
			
			while (td.BaseType != null && known_classes.Contains(td.BaseType)/*class_to_dict_mapping.ContainsKey(td.BaseType.FullName)*/ /*!td.BaseType.Name.Equals(typeof(object).Name)*/) {
				b = td.BaseType.FullName;
				td = td.BaseType.Resolve();
			}
			
			return b;
		}
	}
}
