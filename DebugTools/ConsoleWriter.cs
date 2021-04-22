using System;
using System.IO;
using System.Text;

namespace Apprentice.Debugging {

    public class ConsoleWriter : TextWriter {

        public override Encoding Encoding => Encoding.UTF8;
        public event Action<string> WriteEvent;

        public override void Write(char c) {
            if (c != '\r') {
                WriteEvent?.Invoke(c.ToString());
            }
        }

        public override void Write(string value) {
            WriteEvent?.Invoke(value);
        }

        public override void WriteLine(string value) {
            WriteEvent?.Invoke(value + NewLine);
        }
    }
}
