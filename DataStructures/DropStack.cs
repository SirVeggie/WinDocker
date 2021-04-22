using Apprentice.Tools;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Apprentice {
    public class DropStack<T> : IEnumerable<T> {

        private T[] array;
        private int head;
        public int Count { get; private set; }
        public int MaxSize => array.Length;
        public int Space => MaxSize - Count;

        public T this[int key] {
            get => GetValue(key);
            set => SetValue(key, value);
        }

        public DropStack(int size) {
            array = new T[size];
            head = 0;
            Count = 0;
        }

        public bool Push(T item) {
            if (Count > 0)
                head = Matht.CyclicalClamp(head + 1, 0, MaxSize);
            if (Count < MaxSize)
                Count++;
            array[head] = item;

            return true;
        }

        public T Pop() {
            if (Count < 1) {
                throw new Exception("Cannot Pop from an empty stack.");
            }

            T item = array[head];
            if (Count <= 1)
                head = Matht.CyclicalClamp(head - 1, 0, MaxSize);
            Count--;

            return item;
        }

        private T GetValue(int index) {
            if (index >= Count) {
                throw new IndexOutOfRangeException();
            }

            index = Matht.CyclicalClamp(head - index, 0, MaxSize);

            return array[index];
        }

        private void SetValue(int index, T value) {
            if (index >= Count) {
                throw new IndexOutOfRangeException();
            }

            index = Matht.CyclicalClamp(head - index, 0, MaxSize);
            array[index] = value;
        }

        public IEnumerator<T> GetEnumerator() {
            for (int i = 0; i < Count; i++) {
                yield return GetValue(i);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
}
