using System;
using System.IO;
using System.Xml;
using AppleICloudDotNet.PropertyLists;
using Microsoft.XmlDiffPatch;
using NUnit.Framework;

namespace AppleICloudDotNet.Tests.PropertyLists
{
    [TestFixture]
    public class PropertyListTestFixture
    {
        private ApplePropertyListSerializer serializer;

        private XmlDiff xmlDiff;

        [SetUp]
        public void SetUp()
        {
            serializer = new ApplePropertyListSerializer();

            xmlDiff = new XmlDiff(XmlDiffOptions.IgnoreWhitespace | XmlDiffOptions.IgnoreXmlDecl);
        }

        [TearDown]
        public void TearDown()
        {
        }

        [Test]
        public void TestDeserialize(Tuple<object, string> sample)
        {
            var reader = new StringReader(sample.Item2);
            var graph = AssertDeserialize(new XmlTextReader(reader));
            Assert.IsTrue(new ObjectGraphEqualityComparer().Equals(sample.Item1, graph),
                "Object graph equality check failed.");
        }

        [Test]
        public void TestSerialize(Tuple<object, string> sample)
        {
            var writer = new StringWriter();
            AssertSerialize(new XmlTextWriter(writer), sample.Item1);
            var diffTextWriter = new StringWriter();
            var isMatch = xmlDiff.Compare(
                new XmlTextReader(new StringReader(sample.Item2)),
                new XmlTextReader(new StringReader(writer.ToString())),
                new XmlTextWriter(diffTextWriter));
            Assert.IsTrue(isMatch, "XML equality check failed.\n\n{0}", diffTextWriter);
        }

        [Test]
        public void TestSerializeThenDeserialize(Tuple<object, string> sample)
        {
            var writer = new StringWriter();
            AssertSerialize(new XmlTextWriter(writer), sample.Item1);
            var reader = new StringReader(writer.ToString());
            var graph = AssertDeserialize(new XmlTextReader(reader));
            Assert.IsTrue(new ObjectGraphEqualityComparer().Equals(sample.Item1, graph),
               "Object graph equality check failed.");
        }

        private object AssertDeserialize(XmlReader reader)
        {
            object graph = null;
            Assert.DoesNotThrow(() => graph = this.serializer.Deserialize(reader));
            return graph;
        }

        private void AssertSerialize(XmlWriter writer, object graph)
        {
            Assert.DoesNotThrow(() => this.serializer.Serialize(writer, graph));
        }
    }
}