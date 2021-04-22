using System;

namespace Apprentice.Tools {
    public class Time {
        public static long Now => DateTimeOffset.Now.ToUnixTimeMilliseconds();
    }
}
