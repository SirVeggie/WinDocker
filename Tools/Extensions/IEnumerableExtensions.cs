using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apprentice.Tools.Extensions {
    public static class IEnumerableExtensions {

        public static string StringJoin<T>(this IEnumerable<T> e, string separator = "") => string.Join(separator, e);
    }
}
