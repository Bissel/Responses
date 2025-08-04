using Bissel.Response.Builder;
using Bissel.Response.Messages;

namespace Bissel.Response;

public static class R
{
    public static ResponseBuilder Create(params IResponseMessage[] messages)
    {
        var rb = new ResponseBuilder();
        rb.AddMessages(messages);
        return rb;
    } 
    
    public static ResponseBuilder<T> Create<T>(params IResponseMessage[] messages)
    {
        var rb = new ResponseBuilder<T>();
        rb.AddMessages(messages);
        return rb;
    } 
    
    public static ResponseManyBuilder<T> CreateMany<T>(params IResponseMessage[] messages)
    {
        var rb = new ResponseManyBuilder<T>();
        rb.AddMessages(messages);
        return rb;
    }
}