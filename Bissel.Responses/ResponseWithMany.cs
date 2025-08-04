using System.Collections.Immutable;
using Bissel.Response.Messages;

namespace Bissel.Response;

public class ResponseMany<T> : Response<IReadOnlyCollection<T>>
{
    internal ResponseMany(IEnumerable<T> results, params IResponseMessage[] messages)
        : base(results.ToImmutableList(), messages) { }

    internal ResponseMany(params IResponseMessage[] messages) : base(messages) { }

    public static implicit operator Task<ResponseMany<T>>(ResponseMany<T> resp) => 
        Task.FromResult(resp);
}