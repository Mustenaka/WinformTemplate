using System.Collections.Concurrent;
using System.Globalization;
using System.Reflection;
using System.Text.Json;
using WinformTemplate.Logger;
using WinformTemplate.Serialize;

namespace WinformTemplate.Common.DataAccess;

/// <summary>
/// JSON-backed local repository. Seed files are loaded once per process and CRUD changes stay in memory.
/// The local data source intentionally does not write back to disk so demo data stays resettable.
/// </summary>
public abstract class LocalRepositoryBase<T> : IRepository<T> where T : class
{
    private static readonly ConcurrentDictionary<string, Store> Stores = new(StringComparer.OrdinalIgnoreCase);

    private readonly Store _store;

    protected LocalRepositoryBase(string fileName, string? seedRoot = null)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentException("Seed file name is required.", nameof(fileName));
        }

        var path = ResolveSeedPath(fileName, seedRoot);
        _store = Stores.GetOrAdd(path, LoadStore);
    }

    public virtual Task<T?> GetByIdAsync(object id)
    {
        lock (_store.SyncRoot)
        {
            return Task.FromResult(_store.Items.FirstOrDefault(item => IdEquals(GetEntityId(item), id)));
        }
    }

    public virtual Task<PagedResult<T>> QueryAsync(QueryRequest req)
    {
        ArgumentNullException.ThrowIfNull(req);

        List<T> snapshot;
        lock (_store.SyncRoot)
        {
            snapshot = _store.Items.ToList();
        }

        var query = ApplyQuery(snapshot, req);
        var total = query.Count();
        query = ApplySorting(query, req);

        var page = Math.Max(req.Page, 1);
        var pageSize = Math.Max(req.PageSize, 1);
        var items = query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Task.FromResult(new PagedResult<T>
        {
            Items = items,
            Total = total
        });
    }

    public virtual Task<T> AddAsync(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        lock (_store.SyncRoot)
        {
            var id = GetEntityId(entity);
            if (IsEmptyId(id))
            {
                SetEntityId(entity, _store.NextLongId++);
            }
            else if (TryToInt64(id, out var numericId))
            {
                _store.NextLongId = Math.Max(_store.NextLongId, numericId + 1);
            }

            _store.Items.Add(entity);
            return Task.FromResult(entity);
        }
    }

    public virtual Task<bool> UpdateAsync(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        lock (_store.SyncRoot)
        {
            var id = GetEntityId(entity);
            var index = _store.Items.FindIndex(item => IdEquals(GetEntityId(item), id));
            if (index < 0)
            {
                return Task.FromResult(false);
            }

            _store.Items[index] = entity;
            return Task.FromResult(true);
        }
    }

    public virtual Task<bool> DeleteAsync(object id)
    {
        lock (_store.SyncRoot)
        {
            var index = _store.Items.FindIndex(item => IdEquals(GetEntityId(item), id));
            if (index < 0)
            {
                return Task.FromResult(false);
            }

            _store.Items.RemoveAt(index);
            return Task.FromResult(true);
        }
    }

    protected abstract object? GetEntityId(T entity);

    protected virtual void SetEntityId(T entity, long id)
    {
    }

    protected virtual IEnumerable<T> ApplyQuery(IEnumerable<T> query, QueryRequest req)
    {
        return query;
    }

    protected virtual IEnumerable<T> ApplySorting(IEnumerable<T> query, QueryRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.SortBy))
        {
            return query;
        }

        var property = typeof(T).GetProperty(
            req.SortBy,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
        if (property == null)
        {
            return query;
        }

        return req.Desc
            ? query.OrderByDescending(entity => property.GetValue(entity))
            : query.OrderBy(entity => property.GetValue(entity));
    }

    protected IReadOnlyList<T> Snapshot()
    {
        lock (_store.SyncRoot)
        {
            return _store.Items.ToList();
        }
    }

    protected TResult Read<TResult>(Func<IReadOnlyList<T>, TResult> read)
    {
        lock (_store.SyncRoot)
        {
            return read(_store.Items);
        }
    }

    protected TResult Write<TResult>(Func<List<T>, TResult> write)
    {
        lock (_store.SyncRoot)
        {
            var result = write(_store.Items);
            _store.NextLongId = Math.Max(_store.NextLongId, GetNextLongId(_store.Items));
            return result;
        }
    }

    protected static bool TextContains(string? value, string keyword)
    {
        return value?.Contains(keyword, StringComparison.OrdinalIgnoreCase) == true;
    }

    protected static long? TryLong(IReadOnlyDictionary<string, string>? filters, string key)
    {
        return filters != null && filters.TryGetValue(key, out var value) && long.TryParse(value, out var parsed)
            ? parsed
            : null;
    }

    protected static int? TryInt(IReadOnlyDictionary<string, string>? filters, string key)
    {
        return filters != null && filters.TryGetValue(key, out var value) && int.TryParse(value, out var parsed)
            ? parsed
            : null;
    }

    protected static decimal? TryDecimal(IReadOnlyDictionary<string, string>? filters, string key)
    {
        return filters != null && filters.TryGetValue(key, out var value) && decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : null;
    }

    protected static DateTime? TryDateTime(IReadOnlyDictionary<string, string>? filters, string key)
    {
        return filters != null && filters.TryGetValue(key, out var value) && DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed)
            ? parsed
            : null;
    }

    private Store LoadStore(string path)
    {
        List<T> items;
        if (!File.Exists(path))
        {
            Debug.Warn($"Local seed file not found: {path}");
            items = new List<T>();
        }
        else
        {
            var json = File.ReadAllText(path);
            items = JsonSerializer.Deserialize<List<T>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<T>();
        }

        return new Store
        {
            Items = items,
            NextLongId = GetNextLongId(items)
        };
    }

    private long GetNextLongId(IEnumerable<T> items)
    {
        var max = 0L;
        foreach (var item in items)
        {
            if (TryToInt64(GetEntityId(item), out var id))
            {
                max = Math.Max(max, id);
            }
        }

        return max + 1;
    }

    private static string ResolveSeedPath(string fileName, string? seedRoot)
    {
        var root = string.IsNullOrWhiteSpace(seedRoot)
            ? GlobalProjectConfig.Instance.Config?.Local.SeedPath ?? "./Resources/MockData"
            : seedRoot;

        if (Path.IsPathRooted(root))
        {
            return Path.GetFullPath(Path.Combine(root, fileName));
        }

        var candidates = new[]
        {
            Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, root, fileName)),
            Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), root, fileName))
        };

        return candidates.FirstOrDefault(File.Exists) ?? candidates[0];
    }

    private static bool IdEquals(object? left, object? right)
    {
        if (left == null || right == null)
        {
            return left == right;
        }

        if (TryToInt64(left, out var leftLong) && TryToInt64(right, out var rightLong))
        {
            return leftLong == rightLong;
        }

        return string.Equals(Convert.ToString(left, CultureInfo.InvariantCulture), Convert.ToString(right, CultureInfo.InvariantCulture), StringComparison.Ordinal);
    }

    private static bool IsEmptyId(object? id)
    {
        if (id == null)
        {
            return true;
        }

        return TryToInt64(id, out var numericId) && numericId == 0;
    }

    private static bool TryToInt64(object? value, out long result)
    {
        switch (value)
        {
            case long longValue:
                result = longValue;
                return true;
            case int intValue:
                result = intValue;
                return true;
            case short shortValue:
                result = shortValue;
                return true;
            case string text when long.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed):
                result = parsed;
                return true;
            default:
                result = 0;
                return false;
        }
    }

    private sealed class Store
    {
        public object SyncRoot { get; } = new();

        public List<T> Items { get; init; } = new();

        public long NextLongId { get; set; }
    }
}
