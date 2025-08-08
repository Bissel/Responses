using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Bissel.Responses.Builder;
using Bissel.Responses.Messages;

namespace Bissel.Responses;

public delegate void NextAction<TResponse, in TBuilder>(TBuilder builder)
    where TResponse : IResponse
    where TBuilder : Builder<TResponse>;


public class Pipe<TResponse, TBuilder> 
    where TResponse : IResponse
    where TBuilder : Builder<TResponse>
{
    public static implicit operator Task<TResponse>(Pipe<TResponse, TBuilder> pipe) => pipe.InvokeAsync();
    
    public static Pipe<TResponse, TBuilder> operator |(Pipe<TResponse, TBuilder>pipe, NextAction<TResponse, TBuilder> action) => 
        pipe.Then(action);
    
    public static Pipe<TResponse, TBuilder> operator /(Pipe<TResponse, TBuilder>pipe, NextAction<TResponse, TBuilder> action) => 
        pipe.Then(action, Pipe.Behavior.SkipOnError);
    
    public static Pipe<TResponse, TBuilder> operator *(Pipe<TResponse, TBuilder>pipe, NextAction<TResponse, TBuilder> action) => 
        pipe.Then(action, Pipe.Behavior.IgnoreError);
    
    public static Pipe<TResponse, TBuilder> operator >>>(Pipe<TResponse, TBuilder>pipe, NextAction<TResponse, TBuilder> action) => 
        pipe.Continue().Then(action);
    
    public static Pipe<TResponse, TBuilder> operator --(Pipe<TResponse, TBuilder>pipe) => 
        pipe.Yield();
    
    private readonly Func<TBuilder> _createBuilder;
    private readonly List<PipeStepBase> _steps;
    private readonly Dictionary<Type, ExceptionHandler> _exceptionHandlers;
    private ExceptionHandler _defaultCatchHandler;

    public Pipe(Func<TBuilder> createBuilder) : this(createBuilder, null){}

    private Pipe(
        Func<TBuilder> createBuilder, 
        IEnumerable<PipeStepBase>? steps = null,
        ExceptionHandler? defaultCatchHandler = null,
        Dictionary<Type, ExceptionHandler>? exceptionHandlers = null)
    {
        _createBuilder = createBuilder;
        _steps = (steps ?? []).Select(s => s with {}).ToList();
        _defaultCatchHandler = defaultCatchHandler ?? Pipe.DefaultExceptionHandler;
        _exceptionHandlers = exceptionHandlers?.ToDictionary(kv => kv.Key, kv => kv.Value) 
                             ?? new Dictionary<Type, ExceptionHandler>();
    }
    
    public Pipe<TResponse, TBuilder> Continue() 
        => new(_createBuilder, _steps, _defaultCatchHandler, _exceptionHandlers);
    
    #region Steps
    
    // public Pipe<TResponse, TBuilder> Then(Func<TBuilder, Task> nextAction, Pipe.Behavior behavior = Pipe.Behavior.StopOnError)
    // {
    //     _steps.Add(new PipeStepTaskAction(nextAction, behavior));
    //     return this;
    // }
    //
    // public Pipe<TResponse, TBuilder> Then(Action<TBuilder> nextAction, Pipe.Behavior behavior = Pipe.Behavior.StopOnError)
    // {
    //     _steps.Add(new PipeStepAction(nextAction, behavior));
    //     return this;
    // }
    
    public Pipe<TResponse, TBuilder> Then(NextAction<TResponse, TBuilder> nextAction, Pipe.Behavior behavior = Pipe.Behavior.StopOnError)
    {
        _steps.Add(new PipeStepNextAction(nextAction, behavior));
        return this;
        // if(nextAction is Action<TBuilder> action)
        //     return Then(action, behavior);
        //
        // if(nextAction is Func<TBuilder, Task> taskAction)
        //     return Then(taskAction, behavior);
        //
        // throw new ArgumentOutOfRangeException(nameof(nextAction), nextAction, null);

        // return nextAction switch
        // {
        //     Action<TBuilder> action => Then(action, behavior),
        //     Func<TBuilder, Task> action => Then(action, behavior),
        //     _ => throw new ArgumentOutOfRangeException(nameof(nextAction), nextAction, null)
        // };
    }

    public Pipe<TResponse, TBuilder> Yield()
    {
        _steps.Add(new PipeStepYield());
        return this;
    }
    
    #endregion

    #region ExceptionHandling

    public Pipe<TResponse, TBuilder> Catch<TException, TResponseMessage>(ExceptionHandler<TException, TResponseMessage> handler)
        where TException : Exception
        where TResponseMessage : IResponseMessage
    {
        _exceptionHandlers[typeof(TException)] = e => handler((e as TException)!);
        return this;
    }
    
    public Pipe<TResponse, TBuilder> Catch<TException>(string sourceId, string message)
        where TException : Exception 
        => Catch<TException, ExceptionResponseMessage>(e => new ExceptionResponseMessage(sourceId, message, e));

    public Pipe<TResponse, TBuilder> CatchAll(Func<Exception, IResponseMessage> handler)
    {
        _defaultCatchHandler = e => handler(e);
        return this;
    }
    
    public Pipe<TResponse, TBuilder> CatchAll(string sourceId, string message)
        => CatchAll(e => new ExceptionResponseMessage(sourceId, message, e));
    
    private ExceptionHandler GetExceptionHandler(Type exceptionType) => 
        _exceptionHandlers.GetValueOrDefault(exceptionType, _defaultCatchHandler);
    
    #endregion

    #region Invokation

    public async Task<TResponse> InvokeAsync(bool continueOnCapturedContext = false)
    {
        var builder = _createBuilder();
        foreach (var step in _steps)
        {
            if (!await InvokeStepAsync(builder, step).ConfigureAwait(continueOnCapturedContext))
                break;
        }

        return builder.Build();
    }
    
    public TResponse Invoke(bool throwIfAnyTask = true)
    {
        if (throwIfAnyTask && _steps.Any(s => s is PipeStepTaskAction))
            throw new Exception($"Use {nameof(InvokeAsync)} if you want to invoke a {nameof(Pipe<Response, ResponseBuilder>)} with any {nameof(PipeStepTaskAction)}.)");
        
        var builder = _createBuilder();
        foreach (var step in _steps)
        {
            if (!InvokeStep(builder, step))
                break;
        }

        return builder.Build();
    }

    private bool InvokeStep(TBuilder builder, PipeStepBase stepBase) =>
        stepBase switch
        {
            PipeStepYield      s => InvokeStep(builder, s),
            PipeStepAction     s => InvokeStep(builder, s),
            PipeStepTaskAction s => InvokeStepAsync(builder, s).GetAwaiter().GetResult(),
            // @todo
            PipeStepNextAction s => InvokeStepAsync(builder, s).GetAwaiter().GetResult(),
            _                    => throw new UnreachableException()
        };

    private Task<bool> InvokeStepAsync(TBuilder builder, PipeStepBase stepBase) =>
        stepBase switch
        {
            PipeStepYield      s => Task.FromResult(InvokeStep(builder, s)),
            PipeStepAction     s => Task.FromResult(InvokeStep(builder, s)),
            PipeStepTaskAction s => InvokeStepAsync(builder, s),
            PipeStepNextAction s => InvokeStepAsync(builder, s),
            _                    => throw new UnreachableException()
        };

    private static bool InvokeStep(TBuilder builder, PipeStepYield _)
        => builder.HasFailed;
    
    private bool InvokeStep(TBuilder builder, PipeStepAction action)
    {
        if(ShouldYield(builder, action.Behavior, out var shouldBreak))
            return shouldBreak.Value;

        try
        {
            action.Action.Invoke(builder);
        }
        catch (Exception ex)
        {
            builder.AddMessage(GetExceptionHandler(ex.GetType()).Invoke(ex));
        }
        
        return true;
    }
    
    private async Task<bool> InvokeStepAsync(TBuilder builder, PipeStepTaskAction action)
    {
        if(ShouldYield(builder, action.Behavior, out var shouldBreak))
            return shouldBreak.Value;

        try
        {
            await action.Action.Invoke(builder);
        }
        catch (Exception ex)
        {
            builder.AddMessage(GetExceptionHandler(ex.GetType()).Invoke(ex));
        }
        
        return true;
    }
    
    private async Task<bool> InvokeStepAsync(TBuilder builder, PipeStepNextAction action)
    {
        if(ShouldYield(builder, action.Behavior, out var shouldBreak))
            return shouldBreak.Value;

        try
        {
            var result = action.Action.DynamicInvoke(builder);
            if (result is Task task)
                await task;
        }
        catch (TargetInvocationException ex)
        {
            var inner = ex.InnerException!;
            builder.AddMessage(GetExceptionHandler(inner.GetType()).Invoke(inner));
        }
        catch (Exception ex)
        {
            builder.AddMessage(GetExceptionHandler(ex.GetType()).Invoke(ex));
        }
        
        return true;
    }
    
    
    private static bool ShouldYield(TBuilder builder, Pipe.Behavior behavior, [NotNullWhen(true)] out bool? result)
    {
        result = null;
        if(!builder.HasFailed)
            return false;
        
        switch (behavior)
        {
            case Pipe.Behavior.StopOnError:
                result = false;
                return true;
            case Pipe.Behavior.SkipOnError:
                result = true;
                return true;
            case Pipe.Behavior.IgnoreError:
                return false;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    #endregion

    #region Types
    
    private record PipeStepBase;
    private record PipeStepYield: PipeStepBase;
    private record PipeStepAction(Action<TBuilder> Action, Pipe.Behavior Behavior) : PipeStepBase;
    private record PipeStepTaskAction(Func<TBuilder, Task> Action, Pipe.Behavior Behavior) : PipeStepBase;
    private record PipeStepNextAction(NextAction<TResponse, TBuilder> Action, Pipe.Behavior Behavior) : PipeStepBase;
    
    #endregion
}
