using Microsoft.Azure.Cosmos;

namespace BibleTalkAI.CosmosDatabase.Services;

public static class CosmosOptions
{
    public static readonly ItemRequestOptions NoContentOnWrite = new() 
    { 
        EnableContentResponseOnWrite = false 
    };
}
