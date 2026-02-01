using Goodtocode.AgentFramework.Core.Domain.Actor;
using Goodtocode.AgentFramework.Core.Domain.Audio;
using Goodtocode.AgentFramework.Core.Domain.ChatCompletion;
using Goodtocode.AgentFramework.Core.Domain.Image;
using Goodtocode.AgentFramework.Core.Domain.TextGeneration;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Goodtocode.AgentFramework.Core.Application.Abstractions;

public interface IAgentFrameworkContext
{
    DbSet<ChatMessageEntity> ChatMessages { get; }
    DbSet<ChatSessionEntity> ChatSessions {get; }
    DbSet<TextPromptEntity> TextPrompts { get; }
    DbSet<TextResponseEntity> TextResponses { get; }
    DbSet<TextImageEntity> TextImages { get; }
    DbSet<TextAudioEntity> TextAudio { get; }
    DbSet<ActorEntity> Actors { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
#pragma warning disable CA1716 // Identifiers should not match keywords
    DbSet<TEntity> Set<TEntity>() where TEntity : class;
#pragma warning restore CA1716
    IModel Model { get; }
}