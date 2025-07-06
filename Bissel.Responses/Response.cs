using System.Collections.ObjectModel;
using Bissel.Response.Messages;

namespace Bissel.Response;

public record Response : IResponse
{
    internal Response(bool isSuccess, params IResponseMessage[] messages)
    {
        IsSuccess = isSuccess;
        ResponseMessages = messages.ToList().AsReadOnly();
    }

    public bool IsSuccess { get; }

    public ReadOnlyCollection<IResponseMessage> ResponseMessages { get; }

    public static implicit operator Task<Response>(Response resp) => 
        Task.FromResult(resp);
}