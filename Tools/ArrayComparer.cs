﻿using System.Collections.Generic;

namespace Apprentice.Tools {
    public class ArrayComparer<T> : IEqualityComparer<T[]> {
        public bool Equals(T[] x, T[] y) {
            if (x.Length != y.Length) {
                return false;
            }

            for (int i = 0; i < x.Length; i++) {
                if (!x[i].Equals(y[i])) {
                    return false;
                }
            }

            return true;
        }

        public int GetHashCode(T[] obj) {
            int result = 17;

            for (int i = 0; i < obj.Length; i++) {
                unchecked {
                    result = result * 23 + (obj[i] == null ? 0 : obj[i].GetHashCode());
                }
            }

            return result;
        }
    }
}
