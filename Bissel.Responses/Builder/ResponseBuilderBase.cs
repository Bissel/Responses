using Bissel.Response.Messages;

namespace Bissel.Response.Builder;

public abstract class ResponseBuilderBase
{
    internal bool? IsSuccess { get; set; }
    
    public bool HasFailed => IsSuccess == false;
    
    internal List<IResponseMessage> ResponseMessages { get; init; } = [];

    internal abstract void MarkAsFailed();
}