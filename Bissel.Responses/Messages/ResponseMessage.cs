namespace Bissel.Response.Messages;

public record ResponseMessage(string SourceId, string Message) : IResponseMessage;