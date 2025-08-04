using Bissel.Response.Messages;

namespace Bissel.Response.Builder;


public interface IBuilder
{
    bool HasFailed { get; }
    void MarkAsFailed();
    void AddMessage<TMessage>(TMessage message, bool markAsFailed = false, bool markErrorsAsFailed = true)
        where TMessage : IResponseMessage;
    void AddMessages(IEnumerable<IResponseMessage> messages, bool markAsFailed = false, bool markErrorsAsFailed = true);
}

public abstract class Builder<TResponse>: IBuilder
    where TResponse : IResponse
{
    internal Builder(){}

    protected bool? IsSuccess;
    
    public bool HasFailed => IsSuccess == false;
    
    internal List<IResponseMessage> ResponseMessages { get; init; } = [];

    public abstract void MarkAsFailed();

    internal abstract TResponse Build();
    
    public void AddMessage<TMessage>(TMessage message, bool markAsFailed = false, bool markErrorsAsFailed = true)
        where TMessage : IResponseMessage
    {
        ResponseMessages.Add(message);
        if (markAsFailed || (markErrorsAsFailed && message is IErrorResponseMessage))
            MarkAsFailed();
    }
    
    public void AddMessages(IEnumerable<IResponseMessage> messages, bool markAsFailed = false, bool markErrorsAsFailed = true)
    {
        var responseMessages = messages as IResponseMessage[] ?? messages.ToArray();
        ResponseMessages.AddRange(responseMessages);
        if (markAsFailed || (markErrorsAsFailed && responseMessages.Any(m => m is IErrorResponseMessage)))
            MarkAsFailed();
    }
}