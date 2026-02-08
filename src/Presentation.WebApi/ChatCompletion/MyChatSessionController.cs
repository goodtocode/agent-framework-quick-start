using Goodtocode.AgentFramework.Core.Application.ChatCompletion;
using Goodtocode.AgentFramework.Core.Application.Common.Models;
using Goodtocode.AgentFramework.Presentation.WebApi.Common;
using Microsoft.AspNetCore.Authorization;

namespace Goodtocode.AgentFramework.Presentation.WebApi.ChatCompletion;

/// <summary>
/// Actor endpoints to create a chat, continue a chat, delete a chat and retrieve chat history
/// </summary>
[ApiController]
[ApiConventionType(typeof(DefaultApiConventions))]
[Route("api/v{version:apiVersion}/my/chat")]
[ApiVersion("1.0")]
[Authorize]
public class MyChatSessionController : ApiControllerBase
{
    /// <summary>
    /// Retrieves all chat sessions for the specified actor.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     "ActorId": 60fb5e99-3a78-43df-a512-7d8ff498499e
    ///     "StartDate": "2024-06-01T00:00:00Z"s
    ///     "EndDate": "2024-12-01T00:00:00Z"
    ///     "api-version":  1.0
    /// </remarks>
    /// <returns>
    /// A collection of ChatSessionDto objects representing the actor's chat sessions.
    /// </returns>
    [HttpGet("ChatSessions", Name = "GetMyChatSessions")]
    [ProducesResponseType<ICollection<ChatSessionDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ICollection<ChatSessionDto>> GetAll()
    {
        return await Mediator.Send(new GetMyChatSessionsQuery());
    }

    /// <summary>
    /// Retrieves paginated chat sessions for the specified actor within an optional date range.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     "StartDate": "2024-06-01T00:00:00Z"
    ///     "EndDate": "2024-12-01T00:00:00Z"
    ///     "PageNumber": 1
    ///     "PageSize" : 10
    ///     "api-version":  1.0
    /// 
    /// </remarks>
    /// <param name="startDate">The start date for filtering sessions (optional).</param>
    /// <param name="endDate">The end date for filtering sessions (optional).</param>
    /// <param name="pageNumber">The page number for pagination (default is 1).</param>
    /// <param name="pageSize">The page size for pagination (default is 10).</param>
    /// <returns>
    /// A paginated list of ChatSessionDto objects.
    /// </returns>
    [HttpGet("Paginated", Name = "GetMyChatSessionsPaginated")]
    [ProducesResponseType<PaginatedList<ChatSessionDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PaginatedList<ChatSessionDto>>> GetPaginated(DateTime? startDate, DateTime? endDate, int pageNumber = 1, int pageSize = 10)
    {
        var query = new GetMyChatSessionsPaginatedQuery()
        {
            StartDate = startDate,
            EndDate = endDate,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
        return await Mediator.Send(query);
    }

    /// <summary>
    /// Retrieves a specific chat session for the actor by session ID.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     "ActorId": 60fb5e99-3a78-43df-a512-7d8ff498499e
    ///     "ChatSessionId": 1efb5e99-3a78-43df-a512-7d8ff498499e
    ///     "api-version":  1.0
    /// </remarks>
    /// <param name="id">The identifier of the chat session.</param>
    /// <returns>
    /// ChatSessionDto representing the chat session details.
    /// </returns>
    [HttpGet("{id}", Name = "GetMyChatSession")]
    [ProducesResponseType<ChatSessionDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ChatSessionDto> Get(Guid id)
    {
        return await Mediator.Send(new GetMyChatSessionQuery() { Id = id });
    }

    /// <summary>
    /// Creates new Chat Session with initial message prompt/response history
    /// </summary>
    /// <remarks>
    /// Types of Chat Completion are:
    ///     1. Informational Prompt: A prompt requesting information
    ///         - Example Prompt: "What's the capital of France?"
    ///         - Example Response: "The capital of France is Paris."
    ///     2. Multiple Choice Prompt: A prompt with instructions for multiple-choice responses.
    ///         - Example Prompt: “Choose an activity for the weekend: a) Hiking b) Movie night c) Cooking class d) Board games”
    ///         - Example Response: “I'd recommend hiking! It's a great way to enjoy nature and get some exercise.”
    /// Sample request:
    ///
    ///     HttpPost Body
    ///     {
    ///        "Id": 00000000-0000-0000-0000-000000000000,
    ///        "Message":  "Hi, I am interested in learning about Agent Framework."
    ///     }
    ///
    ///     "version":  1.0
    /// </remarks>
    /// <param name="command"></param>
    /// <returns>
    /// ChatSessionDto
    ///     { Id: 1efb5e99-3a78-43df-a512-7d8ff498499e
    ///     ActorId: 4dfb5e99-3a78-43df-a512-7d8ff498499e
    ///     Messages: [
    ///         {
    ///             "Id": 60fb5e99-3a78-43df-a512-7d8ff498499e,
    ///             "Content": "Certainly! Agent Framework is a great framework for AI.",
    ///         }
    ///     }]
    /// </returns>
    [HttpPost(Name = "CreateMyChatSession")]
    [ProducesResponseType<ChatSessionDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Post(CreateMyChatSessionCommand command)
    {
        var response = await Mediator.Send(command);
        return CreatedAtAction(nameof(Get), new { response.Id }, response);
    }
}