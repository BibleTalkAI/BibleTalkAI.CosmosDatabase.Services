using Microsoft.Azure.Cosmos;
using Polly;
using Polly.Retry;
using System.Net;

namespace BibleTalkAI.CosmosDatabase.Services;

public class CosmosDbService<TDocument>
    (CosmosClient db, string databaseName, CosmosContainer container, string containerId)
    : IDbService<TDocument>
    where TDocument : struct
{
    private static readonly HashSet<HttpStatusCode> _nonRetryableStatusCodes =
    [
        HttpStatusCode.Unauthorized,
        HttpStatusCode.Forbidden,
        HttpStatusCode.BadRequest
    ];

    private static readonly ResiliencePipeline _resilience = new ResiliencePipelineBuilder()
        .AddRetry(new RetryStrategyOptions
        {
            ShouldHandle = new PredicateBuilder().Handle<Exception>((exception) =>
            {
                if (exception is HttpRequestException httpException && httpException.StatusCode != null)
                {
                    // Retry on transient errors (skip non-retryable status codes)
                    return !_nonRetryableStatusCodes.Contains(httpException.StatusCode.Value);
                }

                // Retry on all other exceptions
                return true;
            }),
            Delay = TimeSpan.FromMilliseconds(700),
            MaxRetryAttempts = 3,
            BackoffType = DelayBackoffType.Exponential,
            UseJitter = true
        })
        .AddTimeout(TimeSpan.FromSeconds(30))
        .Build();

    private readonly Container _container = db.GetContainer(databaseName, containerId);
    private readonly CosmosSerializer _serializer = db.ClientOptions.Serializer;

    public CosmosContainer CosmosContainer => container;

    public ValueTask<TDocument?> Get(Guid id)
        => Get(id.ToString());

    public async ValueTask<TDocument?> Get(string id)
    {
        using ResponseMessage responseMessage = await _resilience.ExecuteAsync(async cancellationToken
            => await _container.ReadItemStreamAsync(id, new PartitionKey(id), cancellationToken: cancellationToken));

        if (responseMessage.IsSuccessStatusCode)
        {
            return _serializer.FromStream<TDocument>(responseMessage.Content);
        }
        return null;
    }

    public ValueTask Create(TDocument item, Guid id)
        => Create(item, id.ToString());

    public async ValueTask Create(TDocument item, string id)
        => await _resilience.ExecuteAsync(async cancellationToken
            => await _container.CreateItemAsync(item, new PartitionKey(id),
                requestOptions: CosmosOptions.NoContentOnWrite,
                cancellationToken: cancellationToken));

    public ValueTask Remove(Guid id)
        => Remove(id.ToString());

    public async ValueTask Remove(string id)
        => await _resilience.ExecuteAsync(async cancellationToken
            => await _container.DeleteItemAsync<TDocument>(id, new PartitionKey(id),
                requestOptions: CosmosOptions.NoContentOnWrite,
                cancellationToken: cancellationToken));

    public ValueTask Replace(TDocument item, Guid id)
        => Replace(item, id.ToString());

    public async ValueTask Replace(TDocument item, string id)
        => await _resilience.ExecuteAsync(async cancellationToken
            => await _container.ReplaceItemAsync(item, id, new PartitionKey(id),
                requestOptions: CosmosOptions.NoContentOnWrite,
                cancellationToken: cancellationToken));

    public ValueTask Upsert(TDocument item, Guid id)
        => Upsert(item, id.ToString());

    public async ValueTask Upsert(TDocument item, string id)
        => await _resilience.ExecuteAsync(async cancellationToken
            => await _container.UpsertItemAsync(item, new PartitionKey(id),
                requestOptions: CosmosOptions.NoContentOnWrite,
                cancellationToken: cancellationToken));
}
