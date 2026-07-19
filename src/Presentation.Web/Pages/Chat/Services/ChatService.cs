using Goodtocode.AgentFramework.Presentation.Web.Components.Auth;
using Goodtocode.AgentFramework.Presentation.Web.Pages.Chat.Models;
using Goodtocode.AgentFramework.Presentation.Web.Services;

namespace Goodtocode.AgentFramework.Presentation.Web.Pages.Chat.Services;

public interface IChatService
{
    Task<List<ChatSessionModel>> GetChatSessionsAsync();
    Task<ChatSessionModel> GetChatSessionAsync(Guid chatSessionId);
    Task<ChatSessionModel> CreateSessionAsync(string firstMessage);
    Task RenameSessionAsync(Guid chatSessionId, string newTitle);
    Task<ChatMessageModel> SendMessageAsync(Guid chatSessionId, string newMessage);
}

public class ChatService(BackendApiClient client, IClaimsReader userInfo) : ApiService, IChatService
{
    private readonly BackendApiClient _apiClient = client;
    private readonly IClaimsReader _UserContext = userInfo;

    public async Task<List<ChatSessionModel>> GetChatSessionsAsync()
    {
        var response = await HandleApiException(() => _apiClient.GetMyChatSessionsPaginatedAsync(
            DateTime.UtcNow.AddDays(-30),
            DateTime.UtcNow,
            1,
            20
        ));

        return ChatSessionModel.Create(response.Items);
    }

    public async Task<ChatSessionModel> GetChatSessionAsync(Guid chatSessionId)
    {
        var response = await HandleApiException(() => _apiClient.GetMyChatSessionAsync(
            chatSessionId));

        return ChatSessionModel.Create(response);
    }

    public async Task<ChatSessionModel> CreateSessionAsync(string firstMessage)
    {
        var command = new CreateMyChatSessionCommand
        {
            Message = firstMessage
        };
        var response = await HandleApiException(() => _apiClient.CreateMyChatSessionAsync(command));

        return ChatSessionModel.Create(response);
    }

    public async Task RenameSessionAsync(Guid chatSessionId, string newTitle)
    {
        await HandleApiException(() => _apiClient.PatchMyChatSessionAsync(chatSessionId, new PatchMyChatSessionCommand { Id = chatSessionId, Title = newTitle }));
    }

    public async Task<ChatMessageModel> SendMessageAsync(Guid chatSessionId, string newMessage)
    {
        var response = await HandleApiException(() => _apiClient.CreateMyChatMessageAsync(
            new CreateMyChatMessageCommand
            {
                ChatSessionId = chatSessionId,
                Message = newMessage
            }));

        return ChatMessageModel.Create(response);
    }
}