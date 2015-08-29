using ServiceStack.ServiceHost;

namespace Owin.ServiceStack
{
    /// <summary>
    /// Describes a handler interface for ServiceStack requests
    /// </summary>
    public interface IServiceStackHost
    {
        /// <summary>
        /// Initialize the IoC container
        /// </summary>
        void Init();

        /// <summary>
        /// Handle a request
        /// </summary>
        /// <param name="httpRequest">The request object</param>
        /// <param name="httpResponse">The response object</param>
        /// <returns>True if the request was handled, false if otherwise</returns>
        bool Handle(IHttpRequest httpRequest, IHttpResponse httpResponse);
    }
}
