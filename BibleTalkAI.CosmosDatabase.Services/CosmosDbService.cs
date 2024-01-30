using Microsoft.Azure.Cosmos;

namespace BibleTalkAI.CosmosDatabase.Services;

public class CosmosDbService<TDocument>
    (CosmosClient db, string databaseName, CosmosContainer container, string containerId)
    : IDbService<TDocument>
    where TDocument : struct
{
    private readonly Container _container = db.GetContainer(databaseName, containerId);
    private readonly CosmosSerializer _serializer = db.ClientOptions.Serializer;

    public CosmosContainer CosmosContainer => container;

    public ValueTask<TDocument?> Get(Guid id)
        => Get(id.ToString());

    public async ValueTask<TDocument?> Get(string id)
    {
        using ResponseMessage responseMessage = await _container.ReadItemStreamAsync(id, new PartitionKey(id));
        if (responseMessage.IsSuccessStatusCode)
        {
            return _serializer.FromStream<TDocument>(responseMessage.Content);
        }
        return null;
    }

    public ValueTask Create(TDocument item, Guid id)
        => Create(item, id.ToString());

    public async ValueTask Create(TDocument item, string id)
        => await _container.CreateItemAsync(item, new PartitionKey(id),
            requestOptions: CosmosOptions.NoContentOnWrite);

    public ValueTask Remove(Guid id)
        => Remove(id.ToString());

    public async ValueTask Remove(string id)
        => await _container.DeleteItemAsync<TDocument>(id, new PartitionKey(id),
            requestOptions: CosmosOptions.NoContentOnWrite);

    public ValueTask Replace(TDocument item, Guid id)
        => Replace(item, id.ToString());

    public async ValueTask Replace(TDocument item, string id)
        => await _container.ReplaceItemAsync(item, id, new PartitionKey(id),
            requestOptions: CosmosOptions.NoContentOnWrite);
}
