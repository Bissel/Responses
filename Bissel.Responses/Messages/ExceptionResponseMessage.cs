namespace Bissel.Response.Messages;

public record ExceptionResponseMessage(string SourceId, string Message, Exception Exception)
    : IResponseMessage, IErrorResponseMessage;