using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlothSockets.Internal
{
    /// <summary>
    /// This class brings down the memory usage of List<>, especially with valuetypes/structs,
    /// by stringing small arrays together rather than doubling one big one when the capacity is reached.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class LowMemList<T> : IList<T>
    {
        // Base parameters
        /// <summary> Marks the total capacity of all combined arrays. </summary>
        int capacity;
        /// <summary> The length of each individual array. </summary>
        const int BLOCK_SIZE = 256;
        /// <summary> The size the initial list will reach before segmenting. </summary>
        const int INITIAL_LIST_SIZE = 16384; // = 2^14 (should be some exponential of 2)
        /// <summary> The first list that comes before the arrays list. </summary>
        List<T> initial = new(INITIAL_LIST_SIZE);
        /// <summary> The combined arrays. </summary>
        List<T[]> blocks = new();
        /// <summary> Count of all stored objects. </summary>
        int count;

        // Optimization values
        bool initial_filled = false;
        int current_block = -1;
        int block_index = 0;

        public int Count => count;

        public bool IsReadOnly => throw new NotImplementedException();

        public T this[int index] {
            get {
                var offset_i = index - INITIAL_LIST_SIZE;
                if (offset_i >= 0) {
                    return blocks[offset_i / BLOCK_SIZE][offset_i % BLOCK_SIZE];
                }
                return initial[index];
            }
            set {
                var offset_i = index - INITIAL_LIST_SIZE;
                if (offset_i >= 0) {
                    blocks[offset_i / BLOCK_SIZE][offset_i % BLOCK_SIZE] = value;
                    return;
                }
                initial[index] = value;
            }
        }

        public void Add(T item) {
            if (initial_filled) {
                // Here we are currently adding to the array segments.
                blocks[current_block][block_index] = item;
                count++;
                if (++block_index == BLOCK_SIZE) {
                    blocks.Add(new T[BLOCK_SIZE]);
                    current_block++;
                    block_index = 0;
                }
            }
            else {
                // Here we are below the value where we start creating segments.
                initial.Add(item);
                count++;
                if (++capacity == INITIAL_LIST_SIZE) {
                    initial_filled = true;
                    blocks.Add(new T[BLOCK_SIZE]);
                    current_block++;
                }
            }
        }

        // Very slow, needs speedup, testing
        public void RemoveAt(int index) {
            count--;
            block_index--;
            for (int i = index; i < count; i++) this[i] = this[i + 1];
            this[count] = default;

            // Handle disposing the last block if one exists
            if (count == INITIAL_LIST_SIZE) {
                blocks.Clear();
                return;
            }

            if (count < INITIAL_LIST_SIZE) {
                initial.RemoveAt(initial.Count - 1);
                return;
            }

            if ((count - INITIAL_LIST_SIZE) % BLOCK_SIZE == 0) {
                blocks.RemoveAt(blocks.Count - 1);
                current_block--;
            }
        }

        public int IndexOf(T item) {
            int i_c = 0;
            foreach (var i in this) {
                if (i.Equals(item)) return i_c;
                i_c++;
            }
            return -1;
        }

        public void Insert(int index, T item) {
            throw new NotImplementedException();
        }

        public void Clear() {
            count = 0;
            initial_filled = false;
            current_block = -1;
            block_index = 0;
            initial.Clear();
            blocks.Clear();
        }

        public bool Contains(T item) =>
            IndexOf(item) != -1;

        public void CopyTo(T[] array, int arrayIndex) {
            throw new NotImplementedException();
        }

        public bool Remove(T item) {
            var i = IndexOf(item);
            if (i == -1) return false;
            RemoveAt(i);
            return true;
        }

        public IEnumerator<T> GetEnumerator() {
            int c = 0;
            for (int i = 0; i < initial.Count; i++) {
                c++;
                yield return initial[i];
            }
            for (int b = 0; b < blocks.Count; b++) {
                for (int i = 0; i < blocks[i].Length; i++) {
                    yield return blocks[i][b];
                    if (c++ < count) yield break;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() =>
            GetEnumerator();
    }
}
