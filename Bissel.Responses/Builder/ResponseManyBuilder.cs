using System.Collections.Immutable;
using Bissel.Response.Messages;

namespace Bissel.Response.Builder;

public sealed class ResponseManyBuilder<T> : Builder<ResponseMany<T>>
{
    internal ResponseManyBuilder(){}
    
    private List<T> _results = [];
    
    public override void MarkAsFailed()
    {
        IsSuccess = false;
        _results = [];
    }
    
    public bool AddResult(T result)
    {
        if (IsSuccess is false)
            return false;

        IsSuccess = true;
        _results.Add(result);
        return true;
    }

    public bool AddResults(params T[] results)
    {
        if (IsSuccess is false)
            return false;

        IsSuccess = true;
        _results.AddRange(results);
        return true;
    }
    
    public bool SetResultOrError(IEnumerable<T>? mayResults, IErrorResponseMessage errorMessages)
    {
        if(mayResults != null) 
            return AddResults(mayResults.ToArray());
        
        AddMessage(errorMessages);
        return false;
    }
    
    public async Task<bool> SetResultOrError(Task<IEnumerable<T>?> mayResult, IErrorResponseMessage errorMessages)
    {
        if(await mayResult.ConfigureAwait(false) is {} results) 
            return AddResults(results.ToArray());
        
        AddMessage(errorMessages);
        return false;
    }
    
    internal override ResponseMany<T> Build() => 
        IsSuccess is not false
            ? new ResponseMany<T>(_results.ToImmutableList(), ResponseMessages.ToArray())
            : new ResponseMany<T>(ResponseMessages.ToArray());
}