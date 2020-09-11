using System;
using System.Collections;
using System.Collections.Generic;

namespace ArchivalTibiaV71WorldServer.Utilities
{
    /// <summary>
    /// A container of unique items, with (hopefully) a good balance of enumeration and searching performance
    /// </summary>
    public class KnownCreaturesContainer : IList<uint>
    {
        private readonly HashSet<uint> _hashSet = new HashSet<uint>();
        private readonly List<uint> _list = new List<uint>();

        public IEnumerator<uint> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(uint item)
        {
            if (_hashSet.Contains(item))
                throw new InvalidOperationException("Items have to be unique.");
            _hashSet.Add(item);
            _list.Add(item);
        }

        public void Clear()
        {
            _hashSet.Clear();
            _list.Clear();
        }

        public bool Contains(uint item)
        {
            return _hashSet.Contains(item);
        }

        public void CopyTo(uint[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }

        public bool Remove(uint item)
        {
            if (!_hashSet.Contains(item))
                return false;
            _hashSet.Remove(item);
            _list.Remove(item);
            return true;
        }

        public int Count => _hashSet.Count;
        public bool IsReadOnly => false;

        public int IndexOf(uint item)
        {
            if (!_hashSet.Contains(item))
                return -1;
            return _list.IndexOf(item);
        }

        public void Insert(int index, uint item)
        {
            if (_hashSet.Contains(item))
                throw new InvalidOperationException("Items have to be unique.");
            _list.Insert(index, item);
            _hashSet.Add(item);
        }

        public void RemoveAt(int index)
        {
            if (index >= _list.Count)
                throw new ArgumentOutOfRangeException(nameof(index), "Index was outside of array bounds.");
            var item = _list[index];
            _hashSet.Remove(item);
            _list.RemoveAt(index);
        }

        public uint this[int index]
        {
            get => _list[index];
            set
            {
                if (_hashSet.Contains(value))
                    throw new InvalidOperationException("Items have to be unique.");
                    
                var oldValue = _list[index];
                _hashSet.Remove(oldValue);
                _list[index] = value;
                _hashSet.Add(value);
            }
        }
    }
}