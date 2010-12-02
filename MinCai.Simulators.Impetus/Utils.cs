/*
 * Utils.cs
 * 
 * Copyright (c) 2010 Min Cai <itecgo@163.com>. 
 * 
 * This file is part of ImpetusSharp - a driver program written in C# for Multi2Sim.
 * 
 * Flexim is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * Flexim is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with ImpetusSharp.  If not, see <http ://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using MinCai.Common;
using System.Reflection;

namespace MinCai.Common
{
	public class StringValue : Attribute
	{
		public StringValue (string value)
		{
			this.Value = value;
		}

		public string Value { get; private set; }
	}

	public static class EnumUtils
	{
		public static string ToStringValue (Enum e)
		{
			FieldInfo fi = e.GetType ().GetField (e.ToString ());
			StringValue[] attributes = (StringValue[])fi.GetCustomAttributes (typeof(StringValue), false);
			if (attributes.Length > 0) {
				return attributes[0].Value;
			} else {
				return e.ToString ();
			}
		}

		public static T Parse<T> (string value)
		{
			Type enumType = typeof(T);
			string[] names = Enum.GetNames (enumType);
			foreach (string name in names) {
				if ((ToStringValue ((Enum)Enum.Parse (enumType, name))).Equals (value)) {
					return (T)(Enum.Parse (enumType, name));
				}
			}
			
			throw new ArgumentException ("The string is not a description or value of the specified enum.");
		}
	}

	public sealed class IniFile
	{
		public sealed class Section
		{
			public Section (string name)
			{
				this.Name = name;
				this.Properties = new Dictionary<string, Property> ();
			}

			public void Register (Property property)
			{
				this[property.Name] = property;
			}

			public Property this[string name] {
				get { return this.Properties[name]; }
				set { this.Properties[name] = value; }
			}

			public string Name { get; set; }

			public Dictionary<string, Property> Properties { get; private set; }
		}

		public sealed class Property
		{
			public Property (string name, string val)
			{
				this.Name = name;
				this.Value = val;
			}

			public string Name { get; set; }
			public string Value { get; set; }
		}

		public IniFile ()
		{
			this.Sections = new Dictionary<string, Section> ();
		}

		public void Load (string fileName)
		{
			string sectionName = "";
			Section section = null;
			
			StreamReader sr = new StreamReader (fileName);
			
			string line;
			while ((line = sr.ReadLine ()) != null) {
				line = line.Trim ();
				
				if (line.Length == 0)
					continue;
				
				if (line[0] == ';' || line[0] == '#')
					continue;
				
				if (line[0] == '[') {
					sectionName = line.Substring (1, line.Length - 2);
					
					section = new Section (sectionName);
					this[section.Name] = section;
					
					continue;
				}
				
				int pos;
				if ((pos = line.IndexOf ('=')) == -1)
					continue;
				
				string name = line.Substring (0, pos - 1).Trim ();
				string val = line.Substring (pos + 1).Trim ();
				
				if (val.Length > 0) {
					if (val[0] == '"')
						val = val.Substring (1);
					if (val[val.Length - 1] == '"')
						val = val.Substring (0, val.Length - 1);
				}
				
				section[name] = new IniFile.Property (name, val);
			}
			
			sr.Close ();
		}

		public void save (string fileName)
		{
			StreamWriter sw = new StreamWriter (fileName);
			
			foreach (KeyValuePair<string, Section> sectionPair in this.Sections) {
				string sectionName = sectionPair.Key;
				Section section = sectionPair.Value;
				
				sw.WriteLine ("[ " + sectionName + " ]");
				foreach (KeyValuePair<string, Property> propertyPair in section.Properties) {
					Property property = propertyPair.Value;
					sw.WriteLine (property.Name + " = " + property.Value);
				}
				sw.WriteLine ();
			}
			
			sw.Close ();
		}

		public void Register (Section section)
		{
			this[section.Name] = section;
		}

		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder ();
			
			foreach (KeyValuePair<string, Section> sectionPair in this.Sections) {
				string sectionName = sectionPair.Key;
				Section section = sectionPair.Value;
				
				sb.Append ("[" + sectionName.Trim () + "]\n");
				
				foreach (KeyValuePair<string, Property> propertyPair in section.Properties) {
					Property property = propertyPair.Value;
					
					sb.Append (string.Format ("{0:s} = {1:s}\n", property.Name, property.Value));
				}
			}
			
			return sb.ToString ();
		}

		public Section this[string name] {
			get { return this.Sections[name]; }
			set {
				value.Name = value.Name.Trim ();
				this.Sections[value.Name] = value;
			}
		}

		public Dictionary<string, Section> Sections { get; private set; }
	}

	public class XmlConfig
	{
		public XmlConfig (string typeName)
		{
			this.TypeName = typeName;
			this.Attributes = new SortedDictionary<string, string> ();
			this.Entries = new List<XmlConfig> ();
		}

		public string this[string index] {
			get { return this.Attributes[index]; }
			set { this.Attributes[index] = value; }
		}

		public string TypeName { get; set; }
		public SortedDictionary<string, string> Attributes { get; set; }
		public List<XmlConfig> Entries { get; set; }

		public bool IsNull {
			get { return this.Attributes.ContainsKey (IS_NULL) && bool.Parse (this[IS_NULL]) == true; }
		}

		public static XmlConfig Null (string typeName)
		{
			XmlConfig xmlConfig = new XmlConfig (typeName);
			xmlConfig[IS_NULL] = true + "";
			return xmlConfig;
		}

		private static string IS_NULL = "IsNull";
	}

	public sealed class XmlConfigFile : XmlConfig
	{
		public XmlConfigFile (string typeName) : base(typeName)
		{
		}
	}

	public delegate XmlConfig SaveEntryDelegate<T> (T entry);
	public delegate T LoadEntryDelegate<T> (XmlConfig xmlConfig);

	public delegate KeyT KeyOf<KeyT, EntryT> (EntryT entry);

	public abstract class XmlConfigSerializer<T>
	{
		public abstract XmlConfig Save (T config);
		public abstract T Load (XmlConfig xmlConfig);

		public static XmlConfig SaveList<K> (string name, List<K> entries, SaveEntryDelegate<K> saveEntry)
		{
			XmlConfig xmlConfig = new XmlConfig (name);
			
			foreach (K entry in entries) {
				xmlConfig.Entries.Add (saveEntry (entry));
			}
			
			return xmlConfig;
		}

		public static List<K> LoadList<K> (XmlConfig xmlConfig, LoadEntryDelegate<K> loadEntry)
		{
			List<K> entries = new List<K> ();
			
			foreach (XmlConfig child in xmlConfig.Entries) {
				entries.Add (loadEntry (child));
			}
			
			return entries;
		}

		public static XmlConfig SaveUintDictionary<K> (string name, SortedDictionary<uint, K> entries, SaveEntryDelegate<K> saveEntry)
		{
			XmlConfig xmlConfig = new XmlConfig (name);
			
			foreach (KeyValuePair<uint, K> pair in entries) {
				XmlConfig child = saveEntry (pair.Value);
				
				xmlConfig.Entries.Add (child);
			}
			
			return xmlConfig;
		}

		public static SortedDictionary<uint, K> LoadUintDictionary<K> (XmlConfig xmlConfig, LoadEntryDelegate<K> loadEntry, KeyOf<uint, K> keyOf)
		{
			SortedDictionary<uint, K> entries = new SortedDictionary<uint, K> ();
			
			foreach (XmlConfig child in xmlConfig.Entries) {
				K entry = loadEntry (child);
				entries[keyOf (entry)] = entry;
			}
			
			return entries;
		}

		public static XmlConfig SaveStringDictionary<ValueT> (string name, SortedDictionary<string, ValueT> entries, SaveEntryDelegate<ValueT> saveEntry)
		{
			XmlConfig xmlConfig = new XmlConfig (name);
			
			foreach (KeyValuePair<string, ValueT> pair in entries) {
				XmlConfig child = saveEntry (pair.Value);
				
				xmlConfig.Entries.Add (child);
			}
			
			return xmlConfig;
		}

		public static SortedDictionary<string, K> LoadStringDictionary<K> (XmlConfig xmlConfig, LoadEntryDelegate<K> loadEntry, KeyOf<string, K> keyOf)
		{
			SortedDictionary<string, K> entries = new SortedDictionary<string, K> ();
			
			foreach (XmlConfig child in xmlConfig.Entries) {
				K entry = loadEntry (child);
				entries[keyOf (entry)] = entry;
			}
			
			return entries;
		}
	}

	public abstract class XmlConfigFileSerializer<T>
	{
		public abstract XmlConfigFile Save (T config);
		public abstract T Load (XmlConfigFile xmlConfigFile);

		public void SaveXML (T config, string cwd, string fileName)
		{
			SaveXML (config, cwd + "/" + fileName);
		}

		public void SaveXML (T config, string xmlFileName)
		{
			XmlConfigFile xmlConfigFile = this.Save (config);
			Serialize (xmlConfigFile, xmlFileName);
		}

		public T LoadXML (string cwd, string fileName)
		{
			return LoadXML (cwd + "/" + fileName);
		}

		public T LoadXML (string xmlFileName)
		{
			XmlConfigFile xmlConfigFile = Deserialize (xmlFileName);
			return this.Load (xmlConfigFile);
		}

		public static void Serialize (XmlConfig xmlConfig, XmlElement rootElement)
		{
			XmlElement element = rootElement.OwnerDocument.CreateElement (xmlConfig.TypeName);
			rootElement.AppendChild (element);
			
			Serialize (xmlConfig, rootElement, element);
		}

		public static void Serialize (XmlConfig xmlConfig, XmlElement rootElement, XmlElement element)
		{
			foreach (KeyValuePair<string, string> pair in xmlConfig.Attributes) {
				element.SetAttribute (pair.Key, pair.Value);
			}
			
			foreach (XmlConfig child in xmlConfig.Entries) {
				Serialize (child, element);
			}
		}

		public static void Serialize (XmlConfigFile xmlConfigFile, string xmlFileName)
		{
			XmlDocument doc = new XmlDocument ();
			
			XmlElement rootElement = doc.CreateElement (xmlConfigFile.TypeName);
			doc.AppendChild (rootElement);
			
			foreach (KeyValuePair<string, string> pair in xmlConfigFile.Attributes) {
				rootElement.SetAttribute (pair.Key, pair.Value);
			}
			
			foreach (XmlConfig child in xmlConfigFile.Entries) {
				Serialize (child, rootElement);
			}
			
			doc.Save (xmlFileName);
		}

		public static void Deserialize (XmlConfig rootEntry, XmlElement rootElement)
		{
			XmlConfig entry = new XmlConfig (rootElement.Name);
			
			foreach (XmlAttribute attribute in rootElement.Attributes) {
				entry[attribute.Name] = attribute.Value;
			}
			
			foreach (XmlNode node in rootElement.ChildNodes) {
				if (node is XmlElement) {
					XmlElement childElement = (XmlElement)node;
					Deserialize (entry, childElement);
				}
			}
			
			rootEntry.Entries.Add (entry);
		}

		public static XmlConfigFile Deserialize (string xmlFileName)
		{
			XmlTextReader reader = new XmlTextReader (xmlFileName);
			XmlDocument doc = new XmlDocument ();
			doc.Load (reader);
			reader.Close ();
			
			XmlConfigFile xmlConfigFile = new XmlConfigFile (doc.DocumentElement.Name);
			
			foreach (XmlAttribute attribute in doc.DocumentElement.Attributes) {
				xmlConfigFile[attribute.Name] = attribute.Value;
			}
			
			foreach (XmlNode node in doc.DocumentElement.ChildNodes) {
				if (node is XmlElement) {
					XmlElement childElement = (XmlElement)node;
					Deserialize (xmlConfigFile, childElement);
				}
			}
			
			return xmlConfigFile;
		}
	}
	
//	public interface XmlConfigSerializable
//	{
//	}
//
//	public interface XmlConfigFileSerializable
//	{
//	}
//
//	public static class XmlConfigExtension
//	{
//		public static void Deserialize<T, K> (this T entity, K serializer, string cwd, string fileName) where T : XmlConfigFileSerializable where K : XmlConfigFileSerializer<T>
//		{
//			entity = serializer.LoadXML (cwd + "/" + fileName);
//		}
//
//		public static void Serialize<T, K> (this T entity, K serializer, string cwd, string fileName) where T : XmlConfigFileSerializable where K : XmlConfigFileSerializer<T>
//		{
//			serializer.SaveXML (entity, cwd + "/" + fileName);
//		}
//
//		public static void Deserialize<T, K> (this T entity, K serializer, XmlConfig xmlConfig) where T : XmlConfigSerializable where K : XmlConfigSerializer<T>
//		{
//			entity = serializer.Load (xmlConfig);
//		}
//
//		public static XmlConfig Serialize<T, K> (this T entity, K serializer) where T : XmlConfigSerializable where K : XmlConfigSerializer<T>
//		{
//			return serializer.Save (entity);
//		}
//	}
}
