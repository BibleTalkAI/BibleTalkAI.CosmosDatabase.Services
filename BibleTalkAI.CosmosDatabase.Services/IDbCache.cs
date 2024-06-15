namespace BibleTalkAI.CosmosDatabase.Services;

public interface IDbCache<TDocument>
    where TDocument : struct
{
    ValueTask<TDocument?> Get(Guid id);
    ValueTask<TDocument?> Get(string id);
    ValueTask Create(TDocument item, Guid id);
    ValueTask Create(TDocument item, string id);
    ValueTask Remove(Guid id);
    ValueTask Remove(string id);
    ValueTask Replace(TDocument item, Guid id);
    ValueTask Replace(TDocument item, string id);
    ValueTask Upsert(TDocument item, Guid id);
    ValueTask Upsert(TDocument item, string id);
    void SetCache(Guid id, TDocument item);
    void SetCache(string id, TDocument item);
    void RemoveCached(Guid id);
    void RemoveCached(string id);
}
