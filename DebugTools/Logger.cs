using System;
using System.Collections.Generic;
using System.IO;

namespace Apprentice.Debugging {



    public static class Logger {

        #region enums
        [Flags]
        public enum Mode {
            Console = 1 << 0,
            File = 1 << 1,
            Notification = 1 << 2
        }

        [Flags]
        public enum Category {
            None = 0,
            All = ~None,
            Hotkey = 1 << 0,
            Profiles = 1 << 1,
            Server = 1 << 2
        }
        #endregion

        public static Mode Output { get; set; } = Mode.Console;
        public static Category EnabledCategories { get; set; } = Category.None;

        public static string LogFile { get => AppDomain.CurrentDomain.BaseDirectory + "debug.log"; }

        #region printing
        /// <summary>Print a debug message.</summary>
        /// <param name="message">Message to print.</param>
        /// <param name="level">Specify the priority of the message.</param>
        public static void Print(object message, Category c = Category.None) {
            if (message == null) {
                message = "Null";
            }

            if ((EnabledCategories & c) == c) {
                if (Output.HasFlag(Mode.Console))
                    Console.WriteLine(message.ToString());
                if (Output.HasFlag(Mode.File))
                    PrintFile(message);
            }
        }

        public static void PrintFile(object message) {
            File.AppendAllText(LogFile, message.ToString() + "\n");
        }

        public static void PrintNotification(object message) {
            throw new NotImplementedException();
        }
        #endregion

        public static string Stringify(object obj) {
            var enumerable = obj as System.Collections.IEnumerable;

            if (enumerable == null) {
                if (obj == null) {
                    return "Null";
                } else if (IsKeyValuePair(obj)) {
                    dynamic temp = obj;
                    return $"{{Pair: {Stringify(temp.Key)}, {temp.Value}}}";
                } else {
                    return obj.ToString();
                }
            }

            string name = enumerable.GetType().Name;

            if (name.Contains("`")) {
                name = name.Substring(0, name.IndexOf('`'));
            }

            string res = $"{{ {name}";

            foreach (var item in enumerable) {
                object s = item;
                if (!(item is string))
                    s = Stringify(item);
                res += res == $"{{ {name}" ? $": {s}" : $", {s}";
            }

            return res + " }";
        }

        public static bool IsKeyValuePair(object obj) {
            if (obj == null) {
                return false;
            }

            Type type = obj.GetType();

            if (type.IsGenericType) {
                return type.GetGenericTypeDefinition() != null ? type.GetGenericTypeDefinition() == typeof(KeyValuePair<,>) : false;
            }

            return false;
        }

        public static void PrintMatrix<T>(T[,] matrix) {
            var max = 0;
            foreach (var item in matrix) {
                int length = item.ToString().Length;
                if (length > max) {
                    max = length;
                }
            }

            for (int y = 0; y < matrix.GetLength(1); y++) {
                for (int x = 0; x < matrix.GetLength(0); x++)
                    Console.Write(matrix[x, y].ToString().PadRight(max + 1));
                Console.WriteLine();
            }
        }
    }
}
