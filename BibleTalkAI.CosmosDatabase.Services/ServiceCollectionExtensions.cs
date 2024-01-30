using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Extensions.DependencyInjection;

namespace BibleTalkAI.CosmosDatabase.Services;

public static class ServiceCollectionExtensions
{
    public static CosmosClient AddDatabaseServices(
        this IServiceCollection services, 
        string connectionString, 
        string databaseName, 
        string? applicationRegion = null,
        CosmosSerializer? customSerializer = null)
    {
        services.AddMemoryCache();

        if (string.IsNullOrEmpty(applicationRegion))
        {
            applicationRegion = Regions.EastUS;
        }

        CosmosClientBuilder clientBuilder = new CosmosClientBuilder(connectionString)
            .WithApplicationName(databaseName)
            .WithApplicationRegion(applicationRegion)
            .WithConnectionModeDirect();

        if (customSerializer != null)
        {
            clientBuilder = clientBuilder.WithCustomSerializer(customSerializer);
        }

        CosmosClient cosmosClient = clientBuilder.Build();

        services.AddSingleton(cosmosClient);
        services.AddSingleton(typeof(IDbCache<>), typeof(CosmosDbCache<>));

        return cosmosClient;
    }

    public static IServiceCollection AddCosmosDbService<TDocument>(this IServiceCollection services, CosmosClient cosmosClient, string databaseName, CosmosContainer container, string containerId)
        where TDocument : struct
    {
        var cosmosService = new CosmosDbService<TDocument>(cosmosClient, databaseName, container, containerId);
        services.AddSingleton<IDbService<TDocument>>(cosmosService);

        return services;
    }
}