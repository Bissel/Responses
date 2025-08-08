using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using Bissel.Responses.Messages;

namespace Bissel.Responses;

public class Response<T> : IResponse
{
    internal Response(T result, params IResponseMessage[] messages)
    {
        IsSuccess = true;
        Result = result;
        ResponseMessages = messages.ToList().AsReadOnly();
    }

    internal Response(params IResponseMessage[] messages)
    {
        IsSuccess = false;
        Result = default;
        ResponseMessages = messages.ToList().AsReadOnly();
    }

    private T? Result { get; }
    
    public bool IsSuccess { get; }
    
    public ReadOnlyCollection<IResponseMessage> ResponseMessages { get; }

    public static implicit operator Task<Response<T>>(Response<T> resp) => 
        Task.FromResult(resp);

    public bool TryGetData([NotNullWhen(true)] out T? data)
    {
        data = Result;
        return IsSuccess;
    }
    
    public bool TryGetNullableData(out T? data)
    {
        data = Result;
        return IsSuccess;
    }
}