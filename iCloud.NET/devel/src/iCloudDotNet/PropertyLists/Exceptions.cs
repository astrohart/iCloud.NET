using System;

namespace AppleICloudDotNet.PropertyLists
{
    public class ApplePropertyListSerializerException : Exception
    {
        public ApplePropertyListSerializerException(string message)
            : base(message)
        {
        }

        public ApplePropertyListSerializerException()
            : base()
        {
        }
    }
}