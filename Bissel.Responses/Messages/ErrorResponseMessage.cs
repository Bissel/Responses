namespace Bissel.Response.Messages;

public record ErrorResponseMessage(string SourceId, string Message, string? Details = null)
    : IResponseMessage, IErrorResponseMessage;