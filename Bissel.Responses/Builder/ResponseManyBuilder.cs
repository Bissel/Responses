using System.Collections.Immutable;
using Bissel.Responses.Messages;

namespace Bissel.Responses.Builder;

public sealed class ResponseManyBuilder<T>(params IResponseMessage[] messages) : Builder<ResponseMany<T>>(messages)
{
    private List<T> _results = [];
    
    /// <inheritdoc />
    public override void MarkAsFailed()
    {
        IsSuccess = false;
        _results = [];
    }
    
    /// <inheritdoc />
    internal override ResponseMany<T> Build() => 
        HasSucceeded || IsUndetermined
            ? new ResponseMany<T>(_results.ToImmutableList(), ResponseMessages.ToArray())
            : new ResponseMany<T>(ResponseMessages.ToArray());
    
    /// <summary>
    /// Adds all items to the Result, if <see cref="IBuilder.HasFailed"/> is false
    /// </summary>
    /// <param name="items"></param>
    /// <returns>false if the <see cref="MarkAsFailed"/> was invoked, at least once</returns>
    public bool AddResults(params T[] items)
    {
        if (HasFailed)
            return false;

        IsSuccess = true;
        _results.AddRange(items);
        return true;
    }
    
    /// <summary>
    /// Tries to add many items to the result List, if <see cref="mayResults"/> is null then error with message <see cref="errorMessages"/>
    /// </summary>
    /// <param name="mayResults">The Items that should be added if not null</param>
    /// <param name="errorMessages">The Error Message </param>
    /// <returns>false if the <see cref="MarkAsFailed"/> was invoked, at least once</returns>
    public bool SetResultOrError(IEnumerable<T>? mayResults, IErrorResponseMessage errorMessages)
    {
        if(mayResults != null) 
            return AddResults(mayResults.ToArray());
        
        AddMessage(errorMessages);
        return false;
    }
    
    /// <summary>
    /// Tries to add many items to the result List, if <see cref="mayResults"/> is null then error with message <see cref="errorMessages"/>
    /// </summary>
    /// <param name="mayResults">The Items that should be added if the task evaluates with not null</param>
    /// <param name="errorMessages">The Error Message </param>
    /// <returns>false if the <see cref="MarkAsFailed"/> was invoked, at least once</returns>
    public async Task<bool> SetResultOrError(Task<IEnumerable<T>?> mayResults, IErrorResponseMessage errorMessages)
    {
        if(await mayResults.ConfigureAwait(false) is {} results) 
            return AddResults(results.ToArray());
        
        AddMessage(errorMessages);
        return false;
    }
}