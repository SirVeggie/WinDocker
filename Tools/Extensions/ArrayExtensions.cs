namespace Apprentice.Tools.Extensions {
    public static class ArrayExtensions {
        public static int IndexOfNext<T>(this T[] ar, T item, int start) {
            for (int i = start; i < ar.Length; i++) {
                if (ar[i].Equals(item)) {
                    return i;
                }
            }

            return -1;
        }
    }
}
