using System;

namespace AppleICloudDotNet
{
    public static class CollectionUtilities
    {
        public static TTo[] Convert<TTo>(this Array array)
        {
            return Convert<object, TTo>((object[])array);
        }

        public static TTo[] Convert<TFrom, TTo>(this TFrom[] array)
        {
            return Array.ConvertAll(array, e => (TTo)(object)e);
        }
    }
}