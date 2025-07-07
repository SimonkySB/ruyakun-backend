using Azure;
using Azure.Messaging.EventGrid;

namespace AdoptionManagerMS.Services;


public class EventGridService : IEventGridService
{
    private readonly EventGridPublisherClient _client;

    public EventGridService(IConfiguration configuration)
    {
        var endpoint = new Uri(configuration["EventGrid:TopicEndpoint"]);
        var credential = new AzureKeyCredential(configuration["EventGrid:AccessKey"]);
        _client = new EventGridPublisherClient(endpoint, credential);
    }
    
    public async Task PublishEventAsync<T>(string eventType, T data, string? subject = null)
    {
        var eventGridEvent = new EventGridEvent(
            subject: subject ?? typeof(T).Name,
            eventType: eventType,
            dataVersion: "1.0",
            data: BinaryData.FromObjectAsJson(data));

        await _client.SendEventAsync(eventGridEvent);
    }
}