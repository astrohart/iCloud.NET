using System;
using System.Collections.Generic;
using System.IO;

namespace AppleICloudDotNet.Tests.PropertyLists
{
    public static class PropertyListTestSamples
    {
        public static readonly IList<Tuple<object, string>> Samples;

        static PropertyListTestSamples()
        {
            Samples = new List<Tuple<object, string>>();
            AddAllSamples();
        }

        private static void AddAllSamples()
        {
            AddSample(
                new Dictionary<string, object>()
                {
                    { "Year of Birth", 1900 },
                    { "Pets", new string[] { } },
                    { "Picture", new byte[] { 60, 66, 129, 165, 129, 165, 153, 129, 66, 60 } },
                    { "City of Birth", "Blogsville" },
                    { "Name", "Joe Bloggs" },
                    { "Children", new string[] { "John", "Bill" } },
                });

            AddSample(
                new object[]
                {
                    true,
                    false,
                    new Dictionary<string, object>()
                    {
                        { "Label", "Dictionary" },
                        { "Content",
                            new Dictionary<string, object>()
                            {
                                { "Item1", 1 },
                                { "Item2", 2.5 },
                            }
                        }
                    }
                });
        }

        private static void AddSample(object graph)
        {
            var xml = File.ReadAllText(Path.Combine("Data/PropertyLists/",
                string.Format("sample{0:#0}.plist", Samples.Count)));
            Samples.Add(Tuple.Create(graph, xml));
        }
    }
}