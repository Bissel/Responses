using Bissel.Responses.Messages;

namespace Bissel.Responses.Builder;


public interface IBuilder
{
    /// <summary>
    /// Has the Request(-Builder) Succeeded
    /// </summary>
    bool HasSucceeded { get; }
    
    /// <summary>
    /// Is the Request(-Builder) Undetermined: neither <see cref="HasSucceeded"/> nor <see cref="HasFailed"/> is true
    /// </summary>
    bool IsUndetermined { get; }
    
    /// <summary>
    /// Has the Request(-Builder) Failed
    /// </summary>
    bool HasFailed { get; }

    /// <summary>
    /// Mark the Response as Failed
    /// </summary>
    void MarkAsFailed();
    
    /// <summary>
    /// Adds a Message to the Response(-Builder)
    /// </summary>
    /// <param name="message">The <see cref="IResponseMessage"/> to add</param>
    /// <param name="markAsFailed">Should the Response(-Builder) marked as a failed Response?</param>
    /// <param name="markErrorsAsFailed">Should the Response(-Builder) marked as a failed Response, <b>if</b> the message is an <see cref="IErrorResponseMessage"/></param>
    /// <typeparam name="TMessage"></typeparam>
    void AddMessage<TMessage>(TMessage message, bool markAsFailed = false, bool markErrorsAsFailed = true)
        where TMessage : IResponseMessage;
    /// <summary>
    /// Adds Messages to the Response(-Builder)
    /// </summary>
    /// <param name="messages">The Collection of <see cref="IResponseMessage"/> to add</param>
    /// <param name="markAsFailed">Should the Response(-Builder) marked as a failed Response?</param>
    /// <param name="markErrorsAsFailed">Should the Response(-Builder) marked as a failed Response, <b>if</b> any message in <see cref="messages"/> is an <see cref="IErrorResponseMessage"/></param>
    void AddMessages(IEnumerable<IResponseMessage> messages, bool markAsFailed = false, bool markErrorsAsFailed = true);
}

public abstract class Builder<TResponse>(params IResponseMessage[] messages): IBuilder where TResponse : IResponse
{
    protected bool? IsSuccess;
    
    /// <inheritdoc />
    public bool HasSucceeded => IsSuccess is true;
    
    /// <inheritdoc />
    public bool IsUndetermined => IsSuccess is null;
    
    /// <inheritdoc />
    public bool HasFailed => IsSuccess is false;
    
    internal List<IResponseMessage> ResponseMessages { get; } = messages.ToList();

    public abstract void MarkAsFailed();

    /// <summary>
    /// Builds the Response
    /// </summary>
    internal abstract TResponse Build();
    
    /// <inheritdoc />
    public void AddMessage<TMessage>(TMessage message, bool markAsFailed = false, bool markErrorsAsFailed = true)
        where TMessage : IResponseMessage
    {
        ResponseMessages.Add(message);
        if (markAsFailed || (markErrorsAsFailed && message is IErrorResponseMessage))
            MarkAsFailed();
    }
    
    /// <inheritdoc />
    public void AddMessages(IEnumerable<IResponseMessage> messages, bool markAsFailed = false, bool markErrorsAsFailed = true)
    {
        var responseMessages = messages as IResponseMessage[] ?? messages.ToArray();
        ResponseMessages.AddRange(responseMessages);
        if (markAsFailed || (markErrorsAsFailed && responseMessages.Any(m => m is IErrorResponseMessage)))
            MarkAsFailed();
    }
}