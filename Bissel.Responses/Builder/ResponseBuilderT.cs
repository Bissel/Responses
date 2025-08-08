using Bissel.Responses.Messages;

namespace Bissel.Responses.Builder;

public sealed class ResponseBuilder<T>(params IResponseMessage[] messages) : Builder<Response<T>>(messages)
{
    
    private T? Result { get; set; }
    
    /// <inheritdoc />
    public override void MarkAsFailed()
    {
        IsSuccess = false;
        Result = default;
    }
    
    /// <inheritdoc />
    internal override Response<T> Build() => 
        IsSuccess is true
            ? new Response<T>(Result!, ResponseMessages.ToArray())
            : new Response<T>(ResponseMessages.ToArray());
    
    /// <summary>
    /// Set the result, if <see cref="IBuilder.HasFailed"/> is false
    /// </summary>
    /// <param name="result">The result that should be added</param>
    /// <returns>false if the <see cref="MarkAsFailed"/> was invoked, at least once</returns>
    public bool SetResult(T result)
    {
        if (HasFailed)
            return false;

        IsSuccess = true;
        Result = result;
        return true;
    }

    /// <summary>
    /// Tries to set the result, if <see cref="mayResult"/> is null then error with message <see cref="errorMessages"/>
    /// </summary>
    /// <param name="mayResult">The result that should be added if not null</param>
    /// <param name="errorMessages">The Error Message</param>
    /// <returns>false if the <see cref="MarkAsFailed"/> was invoked, at least once</returns>
    public bool SetResultOrError(T? mayResult, IErrorResponseMessage errorMessages)
    {
        if(mayResult != null)
            return SetResult(mayResult);
        
        AddMessage(errorMessages);
        return false;
    }
    
    /// <summary>
    /// Tries to set the result, if <see cref="mayResult"/> is null then error with message <see cref="errorMessages"/>
    /// </summary>
    /// <param name="mayResult">The result that should be added if evaluates to not null</param>
    /// <param name="errorMessages">The Error Message</param>
    /// <returns>false if the <see cref="MarkAsFailed"/> was invoked, at least once</returns>
    public async Task<bool> SetResultOrError(Task<T?> mayResult, IErrorResponseMessage errorMessages)
    {
        if(await mayResult.ConfigureAwait(false) is {} result) 
            return SetResult(result);
        
        AddMessage(errorMessages);
        return false;
    }
}