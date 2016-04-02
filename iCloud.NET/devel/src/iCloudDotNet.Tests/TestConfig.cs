using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace AppleICloudDotNet.Tests
{
    public static class TestConfig
    {
        private static readonly Regex regexSplit = new Regex(@"(?<!\\)=");

        private static readonly Regex regexUnescape = new Regex(@"\\(\\|=)");

        public static IDictionary<string, string> Get()
        {
            return Get(Path.ChangeExtension(ReflectionUtilities.GetProgramPath(), "cfg"));
        }

        public static IDictionary<string, string> Get(string path)
        {
            return Get(File.OpenRead(path));
        }

        public static IDictionary<string, string> Get(Stream stream)
        {
            using (var reader = new StreamReader(stream))
            {
                var dict = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var parts = regexSplit.Split(line, 2);
                    var key = parts[0].Trim();
                    var value = regexUnescape.Replace(parts[1],
                        match => match.Groups[1].Value).Trim();
                    dict.Add(key, value);
                }
                return dict;
            }
        }
    }
}