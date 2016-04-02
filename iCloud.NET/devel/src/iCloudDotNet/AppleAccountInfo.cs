using System.Collections.Generic;

namespace AppleICloudDotNet
{
    public class AppleAccountInfo
    {
        public string AppleId { get; internal set; }

        public IList<string> AppleIdAliases { get; internal set; }

        public string FullName { get; internal set; }

        public string Id { get; internal set; }

        public bool IsLocked { get; internal set; }

        public string LastName { get; internal set; }

        public bool PrimaryEmailVerified { get; internal set; }

        public int Status { get; internal set; }
    }
}