using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AppleICloudDotNet.Tests.PropertyLists
{
    public class ObjectGraphEqualityComparer : EqualityComparer<object>
    {
        /// <summary>
        /// When overridden in a derived class, determines whether two objects of type
        /// <paramref name="T"/> are equal.
        /// </summary>
        /// <returns>true if the specified objects are equal; otherwise, false.</returns>
        /// <param name="x">The first object to compare.</param>
        /// <param name="y">The second object to compare.</param>
        public override bool Equals(object x, object y)
        {
            if (x is string || x is double || x is float ||
                x is byte || x is short || x is int || x is long || x is DateTime || x is bool)
            {
                return x.Equals(y);
            }

            if (x is IList && y is IList)
            {
                var xList = (IList)x;
                var yList = (IList)y;
                if (xList.Count != yList.Count)
                    return false;
                return !xList.Cast<object>().Where((t, i) => !Equals(t, yList[i])).Any();
            }

            if (!(x is IDictionary) || !(x is IDictionary))
            {
                return false;
            }

            var xDict = (IDictionary)x;
            var yDict = (IDictionary)y;

            return xDict.Count == yDict.Count && xDict.Keys.Cast<object>().All(key => Equals(xDict[key], yDict[key]));
        }

        public override int GetHashCode(object obj)
        {
            throw new NotSupportedException();
        }
    }
}