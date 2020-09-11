using System;
using System.Collections;
using System.Collections.Generic;
using ArchivalTibiaV71WorldServer.Entities;

namespace ArchivalTibiaV71WorldServer.Utilities
{
    public class CreatureContainer : IList<Creature>
    {
        private readonly Dictionary<uint, Creature> _dict = new Dictionary<uint, Creature>();
        private readonly List<Creature> _list = new List<Creature>();

        public IEnumerator<Creature> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(Creature item)
        {
            if(item == null)
                throw new ArgumentNullException(nameof(item));
            if (_dict.ContainsKey(item.Id))
                throw new InvalidOperationException("Items have to be unique.");
            _dict.Add(item.Id, item);
            _list.Add(item);
        }

        public void Clear()
        {
            _dict.Clear();
            _list.Clear();
        }

        public bool Contains(Creature item)
        {
            if(item == null)
                throw new ArgumentNullException(nameof(item));
            return _dict.ContainsKey(item.Id);
        }

        public bool Contains(uint id)
        {
            return _dict.ContainsKey(id);
        }
        

        public void CopyTo(Creature[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }

        public bool Remove(Creature item)
        {
            if(item == null)
                throw new ArgumentNullException(nameof(item));
            if (!_dict.ContainsKey(item.Id))
                return false;
            _dict.Remove(item.Id);
            _list.Remove(item);
            return true;
        }
        
        public bool Remove(uint id)
        {
            if (!_dict.ContainsKey(id))
                return false;
            var removing = _dict[id];
            _dict.Remove(id);
            _list.Remove(removing);
            return true;
        }

        public int Count => _list.Count;
        public bool IsReadOnly => false;

        public int IndexOf(Creature item)
        {
            if(item == null)
                throw new ArgumentNullException(nameof(item));
            if (!_dict.ContainsKey(item.Id))
                return -1;
            return _list.IndexOf(item);
        }

        public void Insert(int index, Creature item)
        {
            if(item == null)
                throw new ArgumentNullException(nameof(item));
            if (_dict.ContainsKey(item.Id))
                throw new InvalidOperationException("Items have to be unique.");
            _list.Insert(index, item);
            _dict.Add(item.Id, item);
        }

        public void RemoveAt(int index)
        {
            if (index >= _list.Count)
                throw new ArgumentOutOfRangeException(nameof(index), "Index was outside of array bounds.");
            var item = _list[index];
            _dict.Remove(item.Id);
            _list.RemoveAt(index);
        }

        public Creature this[int index]
        {
            get => _list[index];
            set
            {
                if(value == null)
                    throw new ArgumentNullException(nameof(value));
                if (value.Id != _list[index].Id && _dict.ContainsKey(value.Id))
                    throw new InvalidOperationException("Items have to be unique.");

                var oldValue = _list[index];
                _dict.Remove(oldValue.Id);
                _list[index] = value;
                _dict.Add(value.Id, value);
            }
        }

        public bool TryGetValue(uint key, out Creature value)
        {
            return _dict.TryGetValue(key, out value);
        }

        public Creature this[uint key]
        {
            get => _dict[key];
            set
            {
                var contains = _dict.ContainsKey(key);
                if (value.Id != key && contains)
                    throw new InvalidOperationException("Items have to be unique.");

                if (contains)
                {
                    var oldValue = _dict[key];
                    _dict[key] = value;
                    var idx = IndexOf(oldValue);
                    _list[idx] = value;
                }
                else
                {
                    Add(value);
                }
            }
        }
    }
}