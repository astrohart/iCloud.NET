using System;
using System.Xml;

namespace AppleICloudDotNet
{
    public static class XmlUtilities
    {
        public static void ReadWhile(this XmlReader reader, Func<bool> predicate)
        {
            while (reader.Read() && predicate())
                ;
        }
    }
}