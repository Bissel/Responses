using System.Collections.ObjectModel;
using Bissel.Response.Messages;

namespace Bissel.Response;

public interface IResponse
{
    public bool IsSuccess { get; }
    public ReadOnlyCollection<IResponseMessage> ResponseMessages { get; }
}