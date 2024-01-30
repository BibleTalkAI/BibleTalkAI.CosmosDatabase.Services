using Microsoft.Azure.Cosmos;
using System.Net;

namespace BibleTalkAI.CosmosDatabase.Services;

public class CosmosDbService<TDocument>
    (CosmosClient db, string databaseName, CosmosContainer container)
    : IDbService<TDocument>
    where TDocument : struct
{
    private readonly Container _container = db.GetContainer(databaseName, container.ToString());

    public CosmosContainer CosmosContainer => container;

    public ValueTask<TDocument?> Get(Guid id)
        => Get(id.ToString());

    public async ValueTask<TDocument?> Get(string id)
    {
        try
        {
            ItemResponse<TDocument> response = await _container.ReadItemAsync<TDocument>(id,
                new PartitionKey(id));

            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
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
