using System.Collections.ObjectModel;
using Bissel.Responses.Messages;

namespace Bissel.Responses;

public interface IResponse
{
    /// <summary>
    /// Determine if the Request was successful
    /// </summary>
    public bool IsSuccess { get; }
    
    /// <summary>
    /// A collection of <see cref="IResponseMessage"/>
    /// </summary>
    public ReadOnlyCollection<IResponseMessage> ResponseMessages { get; }
}