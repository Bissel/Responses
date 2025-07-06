using Bissel.Response.Builder;
using Bissel.Response.Messages;

namespace Bissel.Response;

public static class R
{
    public static ResponseBuilder Create(params IResponseMessage[] messages)
    {
        return new ResponseBuilder { ResponseMessages = messages.ToList() };
    }

    public static ResponseBuilder<T> Create<T>(params IResponseMessage[] messages)
    {
        return new ResponseBuilder<T> { ResponseMessages = messages.ToList() };
    }

    public static ResponseBuilder<T> Create<T>(T result, params IResponseMessage[] messages)
    {
        return Create<T>(messages).SetResult(result);
    }

    public static ResponseManyBuilder<T> CreateMany<T>(params IResponseMessage[] messages)
    {
        return new ResponseManyBuilder<T> { ResponseMessages = messages.ToList() };
    }

    public static ResponseManyBuilder<T> CreateMany<T>(IEnumerable<T> results, params IResponseMessage[] messages)
    {
        return CreateMany<T>(messages).AddResults(results.ToArray());
    }

    public static ResponsePipe<ResponseBuilder> Pipe() => new(Create());
    public static ResponsePipe<ResponseBuilder<T>> Pipe<T>() => new(Create<T>());

    public static ResponsePipe<ResponseManyBuilder<T>> PipeMany<T>() => new(CreateMany<T>());

    public static T MarkAsFailed<T>(this T builder)
        where T : ResponseBuilderBase
    {
        builder.MarkAsFailed();
        return builder;
    }

    public static T AddMessage<T>(this T builder, IResponseMessage message, bool markAsFailed = false,
        bool markErrorsAsFailed = true)
        where T : ResponseBuilderBase
    {
        builder.ResponseMessages.Add(message);
        if (markAsFailed || (markErrorsAsFailed && message is IErrorResponseMessage))
            builder.MarkAsFailed();
        return builder;
    }
    
    public static T AddMessages<T>(this T builder, List<IResponseMessage> messages, bool markAsFailed = false,
        bool markErrorsAsFailed = true)
        where T : ResponseBuilderBase
    {
        builder.ResponseMessages.AddRange(messages);
        if (markAsFailed || (markErrorsAsFailed && messages.Any(m => m is IErrorResponseMessage)))
            builder.MarkAsFailed();
        return builder;
    }
}