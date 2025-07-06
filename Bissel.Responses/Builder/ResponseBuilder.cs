namespace Bissel.Response.Builder;

public sealed class ResponseBuilder : ResponseBuilderBase
{
    internal ResponseBuilder()
    {
    }
    
    internal override void MarkAsFailed()
    {
        IsSuccess = false;
    }

    public static implicit operator Response(ResponseBuilder builder) => 
        new(builder.IsSuccess is not false, builder.ResponseMessages.ToArray());

    public static implicit operator Task<Response>(ResponseBuilder builder) => (Response)builder;
}