// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Utility
{
    using System.Collections.Generic;

    /// <summary>
    /// Base class that eases setting up a class of related comparers (like StringComparer)
    /// </summary>
    public abstract class ComparerBase<T> : IComparer<T>, IEqualityComparer<T>
    {
        private IComparer<T> _comparer;
        private IEqualityComparer<T> _equalityComparer;

        protected ComparerBase(IComparerImplementation implementation)
        {
            _comparer = implementation;
            _equalityComparer = implementation;
        }

        protected interface IComparerImplementation : IComparer<T>, IEqualityComparer<T>
        {
        }

        public int Compare(T x, T y)
        {
            return _comparer.Compare(x, y);
        }

        public bool Equals(T x, T y)
        {
            return _equalityComparer.Equals(x, y);
        }

        public int GetHashCode(T obj)
        {
            return _equalityComparer.GetHashCode(obj);
        }
    }
}