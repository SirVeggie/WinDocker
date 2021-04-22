using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apprentice {
    public class TSList<T> : IList<T> {

        private List<T> list;
        private readonly object locker = new object();

        public int Count { get { lock (locker) { return list.Count; } } }
        public bool IsReadOnly => false;

        public T this[int index] {
            get { lock (locker) { return list[index]; } }
            set { lock (locker) { list[index] = value; } }
        }

        public TSList() {
            list = new List<T>();
        }

        public TSList(List<T> list) {
            this.list = list;
        }

        public List<T> Clone() {
            lock (locker) {
                return new List<T>(list);
            }
        }

        public void Add(T item) {
            lock (locker) {
                list.Add(item);
            }
        }

        public void Clear() {
            lock (locker) {
                list.Clear();
            }
        }

        public bool Contains(T item) {
            lock (locker) {
                return list.Contains(item);
            }
        }

        public void CopyTo(T[] array, int arrayIndex) {
            lock (locker) {
                list.CopyTo(array, arrayIndex);
            }
        }

        public int IndexOf(T item) {
            lock (locker) {
                return list.IndexOf(item);
            }
        }

        public void Insert(int index, T item) {
            lock (locker) {
                list.Insert(index, item);
            }
        }

        public bool Remove(T item) {
            lock (locker) {
                return list.Remove(item);
            }
        }

        public void RemoveAt(int index) {
            lock (locker) {
                list.RemoveAt(index);
            }
        }

        public IEnumerator<T> GetEnumerator() {
            return Clone().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return Clone().GetEnumerator();
        }
    }
}
