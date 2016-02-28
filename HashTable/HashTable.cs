namespace Hash_Table
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;

    public class HashTable<TKey, TValue> : IEnumerable<KeyValue<TKey, TValue>>
    {
        private const int DefaultCapacity = 16;
        private const float ResizeFactor = 0.75f;

        private LinkedList<KeyValue<TKey, TValue>>[] slots;
        private int capacity;

        public HashTable(int capacity = DefaultCapacity)
        {
            this.Capacity = capacity;
            this.slots = new LinkedList<KeyValue<TKey, TValue>>[this.Capacity];
        }

        public int Count { get; private set; }

        public int Capacity
        {
            get
            {
                return this.capacity;
            }

            private set
            {
                if (value <= 0)
                {
                    throw new InvalidEnumArgumentException("Capacity should be a positive integer.");
                }

                this.capacity = value;
            }
        }

        public void Add(TKey key, TValue value)
        {
            this.GrowIfNeeded();
            var slotNumber = this.GetSlotNumber(key);

            if (this.slots[slotNumber] == null)
            {
                this.slots[slotNumber] = new LinkedList<KeyValue<TKey, TValue>>();
            }

            foreach (var element in this.slots[slotNumber])
            {
                if (element.Key.Equals(key))
                {
                    throw new ArgumentException($"Key already exists: {key}");
                }
            }

            var newElement = new KeyValue<TKey, TValue>(key, value);
            this.slots[slotNumber].AddLast(newElement);

            this.Count++;
        }

        public void AddOrReplace(TKey key, TValue value)
        {
            this.GrowIfNeeded();
            var slotNumber = this.GetSlotNumber(key);

            if (this.slots[slotNumber] == null)
            {
                this.slots[slotNumber] = new LinkedList<KeyValue<TKey, TValue>>();
            }

            foreach (var element in this.slots[slotNumber])
            {
                if (element.Key.Equals(key))
                {
                    element.Key = key;
                    element.Value = value;
                    return;
                }
            }

            var newElement = new KeyValue<TKey, TValue>(key, value);
            this.slots[slotNumber].AddLast(newElement);
            this.Count++;
        }

        public TValue Get(TKey key)
        {
            var element = this.Find(key);
            if (element != null)
            {
                return element.Value;
            }

            throw new KeyNotFoundException("Key Not found");
        }

        public TValue this[TKey key]
        {
            get
            {
                return this.Get(key);
            }

            set
            {
                this.AddOrReplace(key, value);
            }
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            var element = this.Find(key);
            if (element != null)
            {
                value = element.Value;
                return true;
            }

            value = default(TValue);
            return false;
        }

        public KeyValue<TKey, TValue> Find(TKey key)
        {
            var slotNUmber = this.GetSlotNumber(key);
            if (this.slots[slotNUmber] != null)
            {
                var elements = this.slots[slotNUmber];
                foreach (var element in elements)
                {
                    if (element.Key.Equals(key))
                    {
                        return element;
                    }
                }
            }

            return null;
        }

        public bool ContainsKey(TKey key)
        {
            var result = this.Find(key) != null;

            return result;
        }

        public bool Remove(TKey key)
        {
            var slotNumber = this.GetSlotNumber(key);
            var elements = this.slots[slotNumber];
            if (elements != null)
            {
                var element = elements.First;
                while (element != null)
                {
                    if (element.Value.Key.Equals(key))
                    {
                        elements.Remove(element);
                        this.Count--;
                        return true;
                    }

                    element = element.Next;
                }
            }

            return false;
        }

        public void Clear()
        {
            this.slots = new LinkedList<KeyValue<TKey, TValue>>[this.Capacity];
            this.Count = 0;
        }

        public IEnumerable<TKey> Keys
        {
            get
            {
                return this.Select(i => i.Key);
            }
        }

        public IEnumerable<TValue> Values
        {
            get
            {
                return this.Select(i => i.Value);
            }
        }

        public IEnumerator<KeyValue<TKey, TValue>> GetEnumerator()
        {
            foreach (var item in this.slots)
            {
                if (item != null)
                {
                    foreach (var keyValue in item)
                    {
                        yield return keyValue;
                    }
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        private void GrowIfNeeded()
        {
            if ((float)(this.Count + 1) / this.Capacity > ResizeFactor)
            {
                this.Grow();
            }
        }

        private void Grow()
        {
            this.Capacity *= 2;
            var newSlots = new LinkedList<KeyValue<TKey, TValue>>[this.Capacity];

            foreach (var element in this)
            {
                var slotIndex = this.GetSlotNumber(element.Key);
                if (newSlots[slotIndex] == null)
                {
                    newSlots[slotIndex] = new LinkedList<KeyValue<TKey, TValue>>();
                }

                newSlots[slotIndex].AddLast(element);
            }
            
            this.slots = newSlots;
        }

        private int GetSlotNumber(TKey key)
        {
            var slotNumber = Math.Abs(key.GetHashCode() % this.capacity);

            return slotNumber;
        }
    }
}
