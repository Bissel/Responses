namespace Bissel.Responses.Messages;

/// <summary>
/// An Error of some sort
/// </summary>
/// <param name="SourceId">What caused the Error</param>
/// <param name="Message">User readable Error Message</param>
/// <param name="Exception">The Error that caused the Exception</param>
public record ExceptionResponseMessage(string SourceId, string Message, Exception Exception)
    : IResponseMessage, IErrorResponseMessage;