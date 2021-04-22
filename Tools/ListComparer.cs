using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apprentice.Tools {
    public class ListComparer<T> : IEqualityComparer<List<T>> {
        public bool Equals(List<T> x, List<T> y) {
            if (x.Count != y.Count) {
                return false;
            }

            for (int i = 0; i < x.Count; i++) {
                if (!x[i].Equals(y[i])) {
                    return false;
                }
            }

            return true;
        }

        public int GetHashCode(List<T> obj) {
            int result = 17;

            for (int i = 0; i < obj.Count; i++) {
                unchecked {
                    result = result * 23 + (obj[i] == null ? 0 : obj[i].GetHashCode());
                }
            }

            return result;
        }
    }
}
