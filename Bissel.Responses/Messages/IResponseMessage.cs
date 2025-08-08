namespace Bissel.Responses.Messages;

/// <summary>
/// A Response-Message
/// </summary>
public interface IResponseMessage
{
    /// <summary>
    /// Technical Identifier: What caused the Message.
    /// Should be unique in the Project.
    /// </summary>
    string SourceId { get; }
    
    /// <summary>
    /// The human-readable message text
    /// </summary>
    string Message { get; }
}