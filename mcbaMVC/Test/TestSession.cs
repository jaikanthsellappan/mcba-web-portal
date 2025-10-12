using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace mcbaMVC.Test;

internal class TestSession : ISession
{
    private readonly Dictionary<string, byte[]> _data = new();
    public IEnumerable<string> Keys => _data.Keys;
    public string Id { get; } = System.Guid.NewGuid().ToString();
    public bool IsAvailable => true;

    public void Clear() => _data.Clear();
    public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    public void Remove(string key) => _data.Remove(key);
    public void Set(string key, byte[] value) => _data[key] = value;
    public bool TryGetValue(string key, out byte[] value) => _data.TryGetValue(key, out value!);
}
