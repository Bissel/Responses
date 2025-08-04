using Bissel.Response.Messages;

namespace Bissel.Response.Builder;

public sealed class ResponseBuilder<T> : Builder<Response<T>>
{
    internal ResponseBuilder(){}
    
    private T? Result { get; set; }
    
    public override void MarkAsFailed()
    {
        IsSuccess = false;
        Result = default;
    }
    
    public bool SetResult(T result)
    {
        if (IsSuccess is false)
            return false;

        IsSuccess = true;
        Result = result;
        return true;
    }

    public bool SetResultOrError(T? mayResult, IErrorResponseMessage errorMessages)
    {
        if(mayResult != null)
            return SetResult(mayResult);
        
        AddMessage(errorMessages);
        return false;
    }
    
    public async Task<bool> SetResultOrError(Task<T?> mayResult, IErrorResponseMessage errorMessages)
    {
        if(await mayResult.ConfigureAwait(false) is {} result) 
            return SetResult(result);
        
        AddMessage(errorMessages);
        return false;
    }

    internal override Response<T> Build() => 
        IsSuccess is true
            ? new Response<T>(Result!, ResponseMessages.ToArray())
            : new Response<T>(ResponseMessages.ToArray());
}