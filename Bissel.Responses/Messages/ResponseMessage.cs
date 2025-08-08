namespace Bissel.Responses.Messages;

/// <summary>
/// A Response-Message
/// <param name="SourceId"><inheritdoc cref="IResponseMessage.SourceId"/></param>
/// <param name="Message"><inheritdoc cref="IResponseMessage.Message"/></param>
/// </summary>
public record ResponseMessage(string SourceId, string Message) : IResponseMessage;