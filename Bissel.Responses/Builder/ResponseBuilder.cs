using Bissel.Responses.Messages;

namespace Bissel.Responses.Builder;

public sealed class ResponseBuilder(params IResponseMessage[] messages) : Builder<Response>(messages)
{
    /// <inheritdoc />
    public override void MarkAsFailed() => IsSuccess = false;
    
    /// <inheritdoc />
    internal override Response Build() => new(HasSucceeded || IsUndetermined, ResponseMessages.ToArray());
}