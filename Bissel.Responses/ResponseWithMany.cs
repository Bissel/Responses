using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Bissel.Response.Messages;

namespace Bissel.Response;

public class ResponseWithMany<T> : ResponseWith<IReadOnlyCollection<T>>
{
    internal ResponseWithMany(IEnumerable<T> results, params IResponseMessage[] messages)
        : base(results.ToImmutableList(), messages) { }

    internal ResponseWithMany(params IResponseMessage[] messages) : base(messages) { }

    public static implicit operator Task<ResponseWithMany<T>>(ResponseWithMany<T> resp) => 
        Task.FromResult(resp);
}