namespace Bissel.Response.Builder;

public sealed class ResponseBuilder : Builder<Response>
{
    internal ResponseBuilder(){}
    public override void MarkAsFailed() => IsSuccess = false;
    internal override Response Build() => new(IsSuccess is not false, ResponseMessages.ToArray());
}