using System;

namespace AppleICloudDotNet
{
    internal static class ReflectionUtilities
    {
        public static Type GetAsGeneric(this Type type)
        {
            if (type.IsGenericType)
            {
                return type.GetGenericTypeDefinition();
            }
            else
            {
                return type;
            }
        }
    }
}