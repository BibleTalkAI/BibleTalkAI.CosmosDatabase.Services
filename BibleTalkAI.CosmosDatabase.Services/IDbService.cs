namespace BibleTalkAI.CosmosDatabase.Services;

public interface IDbService<TDocument>
    where TDocument : struct
{
    CosmosContainer CosmosContainer { get; }
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
}
