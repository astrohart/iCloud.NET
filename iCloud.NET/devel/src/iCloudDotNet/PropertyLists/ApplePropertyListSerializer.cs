using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;

namespace AppleICloudDotNet.PropertyLists
{
    public class ApplePropertyListSerializer
    {
        private const string doctypeName = "plist";

        // Supported versions of plist format for reading
        private static readonly ICollection<string> formatReadVersions = new string[]
            {
                "1.0"
            };

        // Version of plist format for writing
        private const string formatWriteVersion = "1.0";

        private static readonly IList<string> validDoctypePubids = new[]
            {
                "-//Apple//DTD PLIST 1.0//EN",
                "-//Apple Computer//DTD PLIST 1.0//EN",
            };

        private static readonly IList<string> validDoctypeSubsets = new[] {
                (string)null,
            };

        private static readonly IList<string> validDoctypeSysids = new[]
            {
                "http://www.apple.com/DTDs/PropertyList-1.0.dtd",
            };

        // Dictionary of types and their reader methods.
        private static readonly IDictionary<string, ValueReader> valueReaders =
            new Dictionary<string, ValueReader>(StringComparer.InvariantCultureIgnoreCase)
            {
                { "string", ReadString },
                { "real", ReadReal },
                { "integer", ReadInteger },
                { "date", ReadDate },
                { "true", ReadBoolean },
                { "false", ReadBoolean },
                { "data", ReadData },
                { "array", ReadArray },
                { "dict", ReadDictionary },
            };

        // Dictionary of types and their writer methods.
        private static readonly IDictionary<Type, ValueWriter> valueWriters =
            new Dictionary<Type, ValueWriter>(new TypeCompatibilityComparer())
            {
                { typeof(string), WriteString },
                { typeof(double), WriteReal },
                { typeof(float), WriteReal },
                { typeof(byte), WriteInteger },
                { typeof(short), WriteInteger },
                { typeof(int), WriteInteger },
                { typeof(DateTime), WriteDate },
                { typeof(bool), WriteBoolean },
                { typeof(byte[]), WriteData },
                { typeof(IList<>), WriteArray },
                { typeof(IDictionary<,>), WriteDictionary },
            };

        public ApplePropertyListSerializer()
            : base()
        {
            this.Context = new StreamingContext(StreamingContextStates.All);

            this.XmlWriterSettings = new XmlWriterSettings()
                {
                    CloseOutput = false,
                    Encoding = Encoding.UTF8,
                    Indent = true,
                    IndentChars = new string(' ', 2),
                };
        }

        private delegate object ValueReader(XmlReader reader);

        private delegate void ValueWriter(XmlWriter writer, object value);

        public SerializationBinder Binder
        {
            get;
            set;
        }

        public StreamingContext Context
        {
            get;
            set;
        }

        public ISurrogateSelector SurrogateSelector
        {
            get;
            set;
        }

        public XmlWriterSettings XmlWriterSettings
        {
            get;
            private set;
        }

        public object Deserialize(Stream stream)
        {
            return Deserialize(new XmlTextReader(stream));
        }

        public object Deserialize(XmlReader reader)
        {
            reader.ReadWhile(() => reader.NodeType != XmlNodeType.XmlDeclaration);
            reader.ReadWhile(() => reader.NodeType != XmlNodeType.DocumentType);
            if (!(reader.Name == doctypeName && validDoctypePubids.Contains(reader.GetAttribute(0)) &&
                validDoctypeSysids.Contains(reader.GetAttribute(1))))
            {
                throw new ApplePropertyListSerializerException("Invalid plist DOCTYPE.");
            }

            reader.MoveToContent();
            if (reader.Name != "plist")
            {
                throw new ApplePropertyListSerializerException("Expected plist element.");
            }
            var plistVersion = reader.GetAttribute("version");
            if (!formatReadVersions.Contains(plistVersion))
            {
                throw new InvalidOperationException(string.Format(
                    "Unsupported plist version number '{0}'.", plistVersion));
            }
            reader.ReadStartElement();
            var graph = ReadValue(reader);
            reader.ReadEndElement();

            return graph;
        }

        public void Serialize(Stream stream, object graph)
        {
            Serialize(new XmlTextWriter(stream, Encoding.UTF8)
                {
                    Formatting = Formatting.Indented,
                }
                , graph);
        }

        public void Serialize(XmlWriter writer, object graph)
        {
            writer.WriteStartDocument();
            writer.WriteDocType(doctypeName, validDoctypePubids.First(),
                validDoctypeSysids.First(), validDoctypeSubsets.First());

            writer.WriteStartElement("plist");
            writer.WriteStartAttribute("version");
            writer.WriteValue(formatWriteVersion);
            writer.WriteEndAttribute();
            WriteValue(writer, graph);
            writer.WriteEndElement();

            writer.WriteEndDocument();

            writer.Flush();
        }

        private static object ReadArray(XmlReader reader)
        {
            var array = new List<object>();

            var isEmpty = reader.IsEmptyElement;
            reader.ReadStartElement();
            if (!isEmpty)
            {
                while (reader.IsStartElement())
                {
                    array.Add(ReadValue(reader));
                }
                reader.ReadEndElement();
            }
            return array.ToArray();
        }

        private static object ReadBoolean(XmlReader reader)
        {
            var name = reader.Name;
            reader.Read();
            if (name == "true")
            {
                return true;
            }
            else if (name == "false")
            {
                return false;
            }
            else
            {
                throw new ApplePropertyListSerializerException(
                    "Invalid boolean value.");
            }
        }

        private static object ReadData(XmlReader reader)
        {
            return Convert.FromBase64String(reader.ReadElementContentAsString());
        }

        private static object ReadDate(XmlReader reader)
        {
            return reader.ReadElementContentAsDateTime();
        }

        private static object ReadDictionary(XmlReader reader)
        {
            var dict = new Dictionary<string, object>();

            var isEmpty = reader.IsEmptyElement;
            reader.ReadStartElement();
            if (!isEmpty)
            {
                while (reader.IsStartElement())
                {
                    reader.ReadStartElement("key");
                    var key = reader.ReadContentAsString();
                    reader.ReadEndElement();
                    dict.Add(key, ReadValue(reader));
                }
                reader.ReadEndElement();
            }
            return dict;
        }

        private static object ReadInteger(XmlReader reader)
        {
            return reader.ReadElementContentAsInt();
        }

        private static object ReadReal(XmlReader reader)
        {
            return reader.ReadElementContentAsDouble();
        }

        private static object ReadString(XmlReader reader)
        {
            return reader.ReadElementContentAsString();
        }

        private static object ReadValue(XmlReader reader)
        {
            reader.MoveToContent();
            ValueReader valueReader;
            if (!valueReaders.TryGetValue(reader.Name, out valueReader))
            {
                throw new ApplePropertyListSerializerException(string.Format(
                    "Invalid plist value type '{0}'", reader.Name));
            }

            return valueReader(reader);
        }

        private static void WriteArray(XmlWriter writer, object value)
        {
            writer.WriteStartElement("array");
            var array = (Array)value;
            foreach (var item in array)
            {
                WriteValue(writer, item);
            }
            writer.WriteEndElement();
        }

        private static void WriteBoolean(XmlWriter writer, object value)
        {
            writer.WriteStartElement((bool)value ? "true" : "false");
            writer.WriteEndElement();
        }

        private static void WriteData(XmlWriter writer, object value)
        {
            writer.WriteStartElement("data");
            writer.WriteValue(Convert.ToBase64String((byte[])value));
            writer.WriteEndElement();
        }

        private static void WriteDate(XmlWriter writer, object value)
        {
            writer.WriteStartElement("date");
            writer.WriteValue((DateTime)value);
            writer.WriteEndElement();
        }

        private static void WriteDictionary(XmlWriter writer, object value)
        {
            writer.WriteStartElement("dict");
            var dict = (dynamic)value;
            foreach (var item in dict)
            {
                writer.WriteStartElement("key");
                writer.WriteValue(item.Key);
                writer.WriteEndElement();
                WriteValue(writer, item.Value);
            }
            writer.WriteEndElement();
        }

        private static void WriteInteger(XmlWriter writer, object value)
        {
            writer.WriteStartElement("integer");
            writer.WriteValue((int)value);
            writer.WriteEndElement();
        }

        private static void WriteReal(XmlWriter writer, object value)
        {
            writer.WriteStartElement("real");
            writer.WriteValue((double)value);
            writer.WriteEndElement();
        }

        private static void WriteString(XmlWriter writer, object value)
        {
            writer.WriteStartElement("string");
            writer.WriteValue((string)value);
            writer.WriteEndElement();
        }

        private static void WriteValue(XmlWriter writer, object value)
        {
            ValueWriter valueWriter;
            if (!valueWriters.TryGetValue(value.GetType(), out valueWriter))
            {
                throw new ApplePropertyListSerializerException(string.Format(
                    "Invalid type '{0}'", value.GetType()));
            }

            valueWriter(writer, value);
        }

        private class TypeCompatibilityComparer : EqualityComparer<Type>
        {
            public override bool Equals(Type x, Type y)
            {
                return GetFundamentalType(x).Equals(GetFundamentalType(y));
            }

            public override int GetHashCode(Type obj)
            {
                return GetFundamentalType(obj).GetHashCode();
            }

            private Type GetFundamentalType(Type type)
            {
                var genericInterfaces = type.GetInterfaces()
                    .Select(t => t.IsGenericType ? t.GetGenericTypeDefinition() : t);

                if (type == typeof(byte[]))
                    return type;

                if (genericInterfaces.Any(t => t == typeof(IDictionary<,>)))
                {
                    return typeof(IDictionary<,>);
                }
                else if (genericInterfaces.Any(t => t == typeof(IList<>)))
                {
                    return typeof(IList<>);
                }
                else
                {
                    return type;
                }
            }
        }
    }
}