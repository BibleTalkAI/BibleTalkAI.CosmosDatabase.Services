using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;

namespace BibleTalkAI.CosmosDatabase.Services;

public class CosmosDbCache<TDocument>
    (IDbService<TDocument> db, IMemoryCache cache)
    : IDbCache<TDocument>
    where TDocument : struct
{
    private readonly CosmosContainer _container = db.CosmosContainer;

    public async ValueTask<TDocument?> Get(Guid id)
    {
        object cacheKey = CacheKey(id);

        if (cache.TryGetValue(cacheKey, out TDocument? cached))
        {
            if (cached != null)
            {
                return cached;
            }
        }

        TDocument? value = await db.Get(id);

        if (value != null)
        {
            cache.Set(cacheKey, value);
            return value;
        }

        return null;
    }

    public async ValueTask<TDocument?> Get(string id)
    {
        object cacheKey = CacheKey(id);

        if (cache.TryGetValue(cacheKey, out TDocument? cached))
        {
            if (cached != null)
            {
                return cached;
            }
        }

        TDocument? value = await db.Get(id);

        if (value != null)
        {
            cache.Set(cacheKey, value);
            return value;
        }

        return null;
    }

    public async ValueTask Create(TDocument item, Guid id)
    {
        await db.Create(item, id);
        cache.Set(CacheKey(id), item);
    }

    public async ValueTask Create(TDocument item, string id)
    {
        await db.Create(item, id);
        cache.Set(CacheKey(id), item);
    }

    public async ValueTask Remove(Guid id)
    {
        await db.Remove(id);
        RemoveCached(id);
    }

    public async ValueTask Remove(string id)
    {
        await db.Remove(id);
        RemoveCached(id);
    }

    public async ValueTask Replace(TDocument item, Guid id)
    {
        await db.Replace(item, id);
        cache.Set(CacheKey(id), item);
    }

    public async ValueTask Replace(TDocument item, string id)
    {
        await db.Replace(item, id);
        cache.Set(CacheKey(id), item);
    }

    public async ValueTask Upsert(TDocument item, Guid id)
    {
        await db.Upsert(item, id);
        cache.Set(CacheKey(id), item);
    }

    public async ValueTask Upsert(TDocument item, string id)
    {
        await db.Upsert(item, id);
        cache.Set(CacheKey(id), item);
    }

    public void SetCache(Guid id, TDocument item) => cache.Set(CacheKey(id), item);
    public void SetCache(string id, TDocument item) => cache.Set(CacheKey(id), item);

    public void RemoveCached(Guid id) => cache.Remove(CacheKey(id));
    public void RemoveCached(string id) => cache.Remove(CacheKey(id));

    private ConcurrentDictionary<Guid, object>? _cacheKeyCacheGuid;
    private ConcurrentDictionary<string, object>? _cacheKeyCacheString;

    private object CacheKey(Guid id)
    {
        _cacheKeyCacheGuid ??= [];

        if (_cacheKeyCacheGuid.TryGetValue(id, out object? value))
        {
            if (value != null)
            {
                return value;
            }
        }

        (CosmosContainer, Guid) key = (_container, id);

        _cacheKeyCacheGuid.TryAdd(id, key);

        return key;
    }

    private object CacheKey(string id)
    {
        _cacheKeyCacheString ??= [];

        if (_cacheKeyCacheString.TryGetValue(id, out object? value))
        {
            if (value != null)
            {
                return value;
            }
        }

        (CosmosContainer, string) key = (_container, id);

        _cacheKeyCacheString.TryAdd(id, key);

        return key;
    }
}
