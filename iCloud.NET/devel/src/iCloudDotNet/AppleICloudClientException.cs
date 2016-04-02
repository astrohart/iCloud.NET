using System;

namespace AppleICloudDotNet
{
    public class AppleICloudClientException : Exception
    {
        public AppleICloudClientException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public AppleICloudClientException(string message)
            : base(message)
        {
        }

        public AppleICloudClientException()
            : base()
        {
        }
    }
}