using System.IO;
using System.Threading.Tasks;

namespace Apprentice.Tools {
    public static class FileTool {

        public static async Task WriteAsync(string path, string content) {
            using (StreamWriter w = new StreamWriter(path)) {
                await w.WriteAsync(content);
            }
        }

        public static async Task AppendAsync(string path, string content) {
            using (StreamWriter w = new StreamWriter(path, true)) {
                await w.WriteAsync(content);
            }
        }

        public static async Task<string> ReadAsync(string path) {
            string res;

            using (StreamReader r = new StreamReader(path)) {
                res = await r.ReadToEndAsync();
            }

            return res;
        }
    }
}
