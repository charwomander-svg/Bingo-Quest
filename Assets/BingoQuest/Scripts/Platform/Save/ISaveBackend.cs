using System;

namespace BingoQuest.Platform.Save
{
    public interface ISaveBackend
    {
        void Write(string key, string json);
        string Read(string key);
        bool Exists(string key);
        void Delete(string key);
    }

    public sealed class LocalFileSaveBackend : ISaveBackend
    {
        private readonly string _directory;

        public LocalFileSaveBackend(string directory)
        {
            _directory = directory ?? throw new ArgumentNullException(nameof(directory));
            System.IO.Directory.CreateDirectory(_directory);
        }

        public void Write(string key, string json)
        {
            System.IO.File.WriteAllText(Path(key), json, System.Text.Encoding.UTF8);
        }

        public string Read(string key)
        {
            var path = Path(key);
            return System.IO.File.Exists(path) ? System.IO.File.ReadAllText(path, System.Text.Encoding.UTF8) : null;
        }

        public bool Exists(string key) => System.IO.File.Exists(Path(key));

        public void Delete(string key)
        {
            var path = Path(key);
            if (System.IO.File.Exists(path))
                System.IO.File.Delete(path);
        }

        private string Path(string key) => System.IO.Path.Combine(_directory, $"{SanitizeKey(key)}.json");

        private static string SanitizeKey(string key) =>
            string.Concat(key.Split(System.IO.Path.GetInvalidFileNameChars()));
    }

    public sealed class InMemorySaveBackend : ISaveBackend
    {
        private readonly System.Collections.Generic.Dictionary<string, string> _store = new();

        public void Write(string key, string json) => _store[key] = json;
        public string Read(string key) => _store.TryGetValue(key, out var v) ? v : null;
        public bool Exists(string key) => _store.ContainsKey(key);
        public void Delete(string key) => _store.Remove(key);
    }
}
