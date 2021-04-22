using Apprentice.Debugging;
using Apprentice.Tools;
using Apprentice.Tools.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WinUtilities;

namespace Apprentice.Hotkeys {
    public static class Command {

        public static HashSet<Key> DefaultEndKeys { get; private set; } = new HashSet<Key> { Key.Enter, Key.NumpadEnter };
        public static HashSet<Key> DefaultCancelKeys { get; private set; } = new HashSet<Key> { Key.Escape };

        public static async Task<Job<List<Key>>> Record(int? timeout = null, int? maxLength = null, HashSet<Key> endKeys = null, HashSet<Key> cancelKeys = null, CommandContainer container = null) {
            if (IsNullOrEmpty(endKeys) && container == null) {
                throw new Exception($"{nameof(container).ToUpperFirst()} should not be null if no end keys are given");
            }

            KeyHandler.LockKeyboard(true);
            var watch = Stopwatch.StartNew();
            var inputs = new List<Key>();
            var success = false;

            Input.SendUp(KeyHandler.DownKeysVirtual.ToArray());

            while (watch.ElapsedMilliseconds < timeout) {
                var res = await KeyHandler.WaitKeyDown(timeout - (int) watch.ElapsedMilliseconds, true);
                if (res == Key.None)
                    break;
                if (res.IsMouse())
                    continue;
                if (!IsNullOrEmpty(cancelKeys) && cancelKeys.Contains(res)) {
                    break;

                } else if (!IsNullOrEmpty(endKeys) && endKeys.Contains(res)) {
                    success = true;
                    break;

                } else if (res == Key.Backspace) {
                    if (inputs.Count > 0)
                        inputs.RemoveAt(inputs.Count - 1);

                } else {
                    inputs.Add(res);

                    if (maxLength != null && inputs.Count >= maxLength) {
                        success = true;
                        break;

                    } else if (IsNullOrEmpty(endKeys) && container.Commands.ContainsKey(inputs)) {
                        success = true;
                        break;
                    }
                }
            }

            KeyHandler.LockKeyboard(false);

            if (success) {
                return Job.Completed(inputs);
            } else {
                return Job.Failed();
            }

            bool IsNullOrEmpty(HashSet<Key> keyset) {
                if (keyset == null || keyset.Count == 0)
                    return true;
                return false;
            }
        }

        #region helpers
        public static void Validate(List<Key> keys) {
            foreach (Key key in keys) {
                if (key.IsMouse()) {
                    throw new Exception("Mouse keys are not allowed");
                }
            }
        }

        public static List<Key> ToKeyList(string s) {
            char[] chars = s.ToCharArray();
            List<Key> res = new List<Key>();

            for (int i = 0; i < chars.Length; i++) {
                if (chars[i] == Input.ParseOpen) {
                    if (i + 1 >= chars.Length) {
                        throw new Exception($"No pair found for {Input.ParseOpen}");

                    } else {
                        var next = chars.IndexOfNext(Input.ParseClose, i + 1);

                        if (next < 0)
                            throw new Exception($"No pair found for {Input.ParseOpen}");
                        var length = next - (i + 1);

                        if (length < 1)
                            throw new Exception("A parse pair cannot be empty");
                        var temp = s.Substring(i + 1, length).ToLower();

                        res.AddRange(new KeyParseObject(temp).Parse());

                        i = next;
                    }

                } else if (chars[i] == Input.ParseClose) {
                    throw new Exception($"Unexpected closing {Input.ParseClose}");

                } else {
                    string keyString = chars[i].ToString();

                    if ('0' <= chars[i] && chars[i] <= '9') {
                        keyString = "d" + chars[i];
                    }

                    if (!Enum.TryParse(keyString, true, out Key key))
                        throw new Exception($"Illegal character '{chars[i]}' in command");
                    res.Add(key);
                }
            }

            return res;
        }

        private class KeyParseObject {
            public int Count { get; }
            public Key Key { get; }
            public List<Key> Inputs { get; private set; }

            public KeyParseObject(string s) {
                if (!Regex.IsMatch(s, @"^.+?( (\d+))?$")) {
                    throw new Exception($"Unknown signature with '{s}'");
                }

                var data = s.Split(' ');

                if (data.Length > 1)
                    Count = int.Parse(data[1]);
                else
                    Count = 1;
                Key = GetKey(data[0]);
            }

            private static Key GetKey(string keyString) {
                keyString = Regex.Replace(keyString, "Control", "Ctrl", RegexOptions.IgnoreCase);
                keyString = Regex.Replace(keyString, @"^\d$", "D" + keyString);

                if (Enum.TryParse(keyString, true, out Key key))
                    return key;
                throw new Exception($"Key named {keyString} not found");
            }

            public List<Key> Parse() {
                if (Inputs != null)
                    return Inputs;
                Inputs = new List<Key>();

                for (int i = 0; i < Count; i++) {
                    Inputs.Add(Key);
                }

                return Inputs;
            }
        }
        #endregion
    }

    public class CommandContainer {

        public Dictionary<List<Key>, Action> Commands { get; }
        public HashSet<Key> EndKeys { get; set; }
        public HashSet<Key> CancelKeys { get; set; }

        public CommandContainer() {
            Commands = new Dictionary<List<Key>, Action>(new ListComparer<Key>());
            EndKeys = Command.DefaultEndKeys;
            CancelKeys = Command.DefaultCancelKeys;
        }

        public CommandContainer(HashSet<Key> endKeys) {
            Commands = new Dictionary<List<Key>, Action>(new ListComparer<Key>());
            EndKeys = endKeys;
            CancelKeys = Command.DefaultCancelKeys;
        }

        public CommandContainer(HashSet<Key> endKeys, HashSet<Key> cancelKeys) {
            Commands = new Dictionary<List<Key>, Action>(new ListComparer<Key>());
            EndKeys = endKeys;
            CancelKeys = cancelKeys;
        }

        public void Add(string command, Action action) {
            if (action == null)
                throw new ArgumentNullException($"{nameof(action).ToUpperFirst()} cannot be null");
            if (string.IsNullOrEmpty(command))
                throw new ArgumentNullException($"{nameof(command).ToUpperFirst()} cannot be null");

            var list = Command.ToKeyList(command);

            if (Commands.ContainsKey(list))
                throw new Exception($"A command with '{command}' combination has already been made");
            Command.Validate(list);

            Commands.Add(list, action);
        }

        public void Add(List<Key> keys, Action action) {
            if (action == null)
                throw new ArgumentNullException($"{nameof(action).ToUpperFirst()} cannot be null");
            if (keys == null || keys.Count == 0)
                throw new ArgumentException($"{nameof(keys).ToUpperFirst()} cannot be null or empty");

            if (Commands.ContainsKey(keys))
                throw new Exception($"A command with '{Logger.Stringify(keys)}' combination has already been made");
            Command.Validate(keys);

            Commands.Add(keys, action);
        }

        public Task<bool> Start() => Start(null);
        public async Task<bool> Start(int? timeout) {
            var job = await Command.Record(timeout, null, EndKeys, CancelKeys, EndKeys == null || EndKeys.Count == 0 ? this : null);

            if (!job.Success)
                return false;
            if (!Commands.ContainsKey(job.Result))
                return false;
            Commands[job.Result].Invoke();
            return true;
        }
    }

    public static class GlobalCommands {
        public static int Timeout { get; } = 5000;
        public static CommandContainer Container { get; } = new CommandContainer(new HashSet<Key>(Command.DefaultEndKeys).Append(Key.Space).ToHashSet());
        public static void Add(string command, Action action) => Container.Add(command, action);
        public static Task<bool> Start() => Container.Start(Timeout);
    }
}
