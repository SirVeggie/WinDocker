using System;
using System.Collections.Generic;
using System.Linq;

namespace Apprentice.Tools.Extensions {
    public static class TypeExtensions {

        public static List<Type> GetSubclasses(this Type type) {
            return type.Assembly.GetLoadableTypes().Where(t => t.IsSubclassOf(type)).ToList();
        }
    }
}
