
using System.Collections.Concurrent;
using System.Text.Json;

namespace ToggleHub.Core;

public interface IFlagStore
{
    IReadOnlyCollection<Flag> List();
    Flag? Get(string key);
    void Upsert(Flag flag);
    bool Delete(string key);
    void Toggle(string key, bool enabled);
    void Import(IEnumerable<Flag> flags);
}

public class FileFlagStore : IFlagStore
{
    private readonly ConcurrentDictionary<string, Flag> _flags = new(StringComparer.OrdinalIgnoreCase);
    private readonly string _path;
    private readonly object _ioLock = new();

    public FileFlagStore(string path)
    {
        _path = path;
        Load();
    }

    public IReadOnlyCollection<Flag> List() => _flags.Values.OrderBy(f => f.Key).ToList();
    public Flag? Get(string key) => _flags.TryGetValue(key, out var f) ? f : null;

    public void Upsert(Flag flag)
    {
        _flags[flag.Key] = flag with { UpdatedAt = DateTimeOffset.UtcNow };
        Save();
    }

    public bool Delete(string key)
    {
        var ok = _flags.TryRemove(key, out _);
        if (ok) Save();
        return ok;
    }

    public void Toggle(string key, bool enabled)
    {
        if (_flags.TryGetValue(key, out var f))
        {
            _flags[key] = f with { Enabled = enabled, UpdatedAt = DateTimeOffset.UtcNow };
            Save();
        }
    }

    public void Import(IEnumerable<Flag> flags)
    {
        foreach (var f in flags)
            _flags[f.Key] = f with { UpdatedAt = DateTimeOffset.UtcNow };
        Save();
    }

    private void Load()
    {
        try
        {
            if (!File.Exists(_path)) return;
            var json = File.ReadAllText(_path);
            var list = JsonSerializer.Deserialize<List<Flag>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
            foreach (var f in list) _flags[f.Key] = f;
        }
        catch { }
    }

    private void Save()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
            var json = JsonSerializer.Serialize(_flags.Values.OrderBy(f => f.Key).ToList(), new JsonSerializerOptions { WriteIndented = true });
            lock (_ioLock)
            {
                File.WriteAllText(_path, json);
            }
        }
        catch { }
    }
}
