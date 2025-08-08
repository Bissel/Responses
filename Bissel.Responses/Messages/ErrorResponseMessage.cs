namespace Bissel.Responses.Messages;

/// <summary>
/// An Error of some sort
/// </summary>
/// <param name="SourceId">What caused the Error</param>
/// <param name="Message">User readable Error Message</param>
/// <param name="Details">More details</param>
public record ErrorResponseMessage(string SourceId, string Message, string? Details = null)
    : IResponseMessage, IErrorResponseMessage;