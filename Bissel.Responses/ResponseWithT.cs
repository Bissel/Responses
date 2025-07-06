using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using Bissel.Response.Messages;

namespace Bissel.Response;

public class ResponseWith<T> : IResponse
{
    internal ResponseWith(T result, params IResponseMessage[] messages)
    {
        IsSuccess = true;
        Result = result;
        ResponseMessages = messages.ToList().AsReadOnly();
    }

    internal ResponseWith(params IResponseMessage[] messages)
    {
        IsSuccess = false;
        Result = default;
        ResponseMessages = messages.ToList().AsReadOnly();
    }

    public T? Result { get; }
    public bool IsSuccess { get; }
    public ReadOnlyCollection<IResponseMessage> ResponseMessages { get; }

    public static implicit operator Task<ResponseWith<T>>(ResponseWith<T> resp) => 
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