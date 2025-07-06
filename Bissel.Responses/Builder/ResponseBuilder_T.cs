using Bissel.Response.Messages;

namespace Bissel.Response.Builder;

public class ResponseBuilder<T> : ResponseBuilderBase
{
    internal ResponseBuilder()
    {
    }

    private T? Result { get; set; }

    internal override void MarkAsFailed()
    {
        IsSuccess = false;
        Result = default;
    }

    public ResponseBuilder<T> SetResult(T result)
    {
        if (IsSuccess is false)
            return this;

        IsSuccess = true;
        Result = result;
        return this;
    }

    public ResponseBuilder<T> SetResultOrError(T? mayResult, IErrorResponseMessage errorMessages)
    {
        if(mayResult != null) 
            SetResult(mayResult);
        else 
            this.AddMessage(errorMessages);
        return this;
    }
    
    public async Task<ResponseBuilder<T>> SetResultOrError(Task<T?> mayResult, IErrorResponseMessage errorMessages)
    {
        if(await mayResult.ConfigureAwait(false) is {} result) 
            SetResult(result);
        else 
            this.AddMessage(errorMessages);
        return this;
    }

    public static implicit operator ResponseWith<T>(ResponseBuilder<T> builder) =>
        builder.IsSuccess is true
            ? new ResponseWith<T>(builder.Result!, builder.ResponseMessages.ToArray())
            : new ResponseWith<T>(builder.ResponseMessages.ToArray());
    
    public static implicit operator Task<ResponseWith<T>>(ResponseBuilder<T> builder)
        => (ResponseWith<T>)builder;
    
    public static implicit operator Task<ResponseBuilder<T>>(ResponseBuilder<T> builder) 
        => Task.FromResult(builder);

}