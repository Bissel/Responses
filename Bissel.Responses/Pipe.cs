using Bissel.Response.Builder;
using Bissel.Response.Messages;

namespace Bissel.Response;

public delegate IResponseMessage ExceptionHandler(Exception exception);

public delegate TResponseMessage ExceptionHandler<in TException, out TResponseMessage>(TException exception) 
    where TException : Exception
    where TResponseMessage : IResponseMessage;

public static class Pipe
{
    public enum Behavior
    {
        StopOnError,
        IgnoreError,
        SkipOnError
    }
    
    public static Pipe<Response, ResponseBuilder> Create() => new(() => new());
    
    public static Pipe<Response<T>, ResponseBuilder<T>> Create<T>() => new(() => new());
    
    public static Pipe<ResponseMany<T>, ResponseManyBuilder<T>> CreateMany<T>() => new(() => new());
    
    public static readonly ExceptionHandler DefaultExceptionHandler =
        e => new ExceptionResponseMessage("86F08156-AA22-416A-915B-6A1654513CB0","Something went wrong", e);
}
