using System.Collections.Immutable;
using Bissel.Response.Messages;

namespace Bissel.Response.Builder;

public class ResponseManyBuilder<T> : ResponseBuilderBase
{
    internal ResponseManyBuilder()
    {
    }

    private List<T> Results { get; set; } = [];

    internal override void MarkAsFailed()
    {
        IsSuccess = false;
        Results = [];
    }

    public ResponseManyBuilder<T> AddResult(T result)
    {
        if (IsSuccess is false)
            return this;

        IsSuccess = true;
        Results.Add(result);
        return this;
    }

    public ResponseManyBuilder<T> AddResults(params T[] results)
    {
        if (IsSuccess is false)
            return this;

        IsSuccess = true;
        Results.AddRange(results);
        return this;
    }
    
    public ResponseManyBuilder<T> SetResultOrError(IEnumerable<T>? mayResults, IErrorResponseMessage errorMessages)
    {
        if(mayResults != null) 
            AddResults(mayResults.ToArray());
        else 
            this.AddMessage(errorMessages);
        return this;
    }
    
    public async Task<ResponseManyBuilder<T>> SetResultOrError(Task<IEnumerable<T>?> mayResult, IErrorResponseMessage errorMessages)
    {
        if(await mayResult.ConfigureAwait(false) is {} results) 
            AddResults(results.ToArray());
        else 
            this.AddMessage(errorMessages);
        return this;
    }

    public static implicit operator ResponseWithMany<T>(ResponseManyBuilder<T> builder) =>
        builder.IsSuccess is not false
            ? new ResponseWithMany<T>(builder.Results.ToImmutableList(), builder.ResponseMessages.ToArray())
            : new ResponseWithMany<T>(builder.ResponseMessages.ToArray());

    public static implicit operator Task<ResponseWithMany<T>>(ResponseManyBuilder<T> builder) =>
        (ResponseWithMany<T>)builder;
}