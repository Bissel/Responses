using Bissel.Response.Builder;
using Bissel.Response.Messages;

namespace Bissel.Response;

public enum PipePreviousBehavior
{
    StopOnError,
    IgnoreError,
    SkipOnError
}
public enum PipeContinueBehavior
{
    StopOnError,
    IgnoreError
}

public static class ResponsePipeExtensions
{
    public static ResponsePipe<TBuilder> CheckObject<TBuilder, TObject>(this ResponsePipe<TBuilder> pipe,
        TObject @object) where TBuilder : ResponseBuilderBase
    {
        return pipe;
    }

    public static Task<Response> Do(this ResponsePipe<ResponseBuilder> pipe) =>
        pipe.Invoke().ContinueWith<Response>(r => r.Result);
    
    public static Task<ResponseWith<T>> Do<T>(this ResponsePipe<ResponseBuilder<T>> pipe) =>
        pipe.Invoke().ContinueWith<ResponseWith<T>>(r => r.Result);
    
    public static Task<ResponseWithMany<T>> Do<T>(this ResponsePipe<ResponseManyBuilder<T>> pipe) =>
        pipe.Invoke().ContinueWith<ResponseWithMany<T>>(r => r.Result);
}

public sealed class ResponsePipe<TBuilder> where TBuilder 
    : ResponseBuilderBase
{
    private record Step(
        Func<TBuilder, Task> Call,
        PipePreviousBehavior PreviousBehavior, 
        PipeContinueBehavior ContinueBehavior
    );

    private readonly TBuilder _builder;
    
    private readonly List<Step> _steps = [];
    private readonly Dictionary<Type, Func<Exception, IResponseMessage>> _exceptionHandlers = [];
    
    private Func<Exception, IResponseMessage> _defaultCatchHandler =
        e => new ExceptionResponseMessage(
            "86F08156-AA22-416A-915B-6A1654513CB0",
            "Something went wrong",
            e
        );

    public ResponsePipe(TBuilder builder)
    {
        _builder = builder;
    }
    
    private Func<Exception, IResponseMessage> GetExceptionHandler(Type exceptionType)
    {
        return _exceptionHandlers.GetValueOrDefault(exceptionType, _defaultCatchHandler);
    }
    
    public ResponsePipe<TBuilder> Then(
        Func<TBuilder, Task> task, 
        PipePreviousBehavior previousBehavior = PipePreviousBehavior.StopOnError,
        PipeContinueBehavior continueBehavior = PipeContinueBehavior.StopOnError
    )
    {
        _steps.Add(new Step(task, previousBehavior, continueBehavior));
        return this;
    }
    
    public ResponsePipe<TBuilder> Then(
        Action<TBuilder> action, 
        PipePreviousBehavior previousBehavior = PipePreviousBehavior.StopOnError,
        PipeContinueBehavior continueBehavior = PipeContinueBehavior.StopOnError
    ) =>
        Then(builder =>
            {
                action(builder);
                return Task.CompletedTask;
            },
            previousBehavior,
            continueBehavior
        );

    public ResponsePipe<TBuilder> Catch(string sourceId, string message)
    {
        _defaultCatchHandler = e => new ExceptionResponseMessage(sourceId, message, e);
        return this;
    }

    public ResponsePipe<TBuilder> Catch<T>(Func<T, IResponseMessage> handler) where T : Exception
    {
        _exceptionHandlers[typeof(T)] = e => handler((e as T)!);
        return this;
    }

    public async Task<TBuilder> Invoke()
    {
        try
        {
            await InvokeList(_steps).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _builder.AddMessage(GetExceptionHandler(ex.GetType()).Invoke(ex));
        }

        return _builder;
    }
    
    public static implicit operator Task<TBuilder>(ResponsePipe<TBuilder> pipe) => pipe.Invoke();

    public static ResponsePipe<TBuilder> operator |(ResponsePipe<TBuilder> pipe, Action<TBuilder> task) => 
        pipe.Then(task);
    
    public static ResponsePipe<TBuilder> operator |(ResponsePipe<TBuilder> pipe, Func<TBuilder, Task> task) => 
        pipe.Then(task);
    
    private async Task InvokeList(List<Step> steps)
    {
        foreach (var step in steps)
        {
            if(_builder.HasFailed)
            {
                if (step.PreviousBehavior == PipePreviousBehavior.StopOnError)
                    return;

                if (step.PreviousBehavior == PipePreviousBehavior.SkipOnError)
                    continue;
            }
                
            await step.Call.Invoke(_builder).ConfigureAwait(false);
                
            if (_builder.HasFailed && step.ContinueBehavior == PipeContinueBehavior.StopOnError)
                return;
        }
    }
}