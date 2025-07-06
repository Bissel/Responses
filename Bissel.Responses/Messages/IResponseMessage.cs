namespace Bissel.Response.Messages;

public interface IResponseMessage
{
    string SourceId { get; }
    string Message { get; }
}