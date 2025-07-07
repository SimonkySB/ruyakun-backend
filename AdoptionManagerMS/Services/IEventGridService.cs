namespace AdoptionManagerMS.Services;

public interface IEventGridService
{
    Task PublishEventAsync<T>(string eventType, T data, string? subject = null);
}