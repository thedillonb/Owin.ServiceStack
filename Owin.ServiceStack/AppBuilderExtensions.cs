using System.Reflection;
using System.Threading.Tasks;

namespace Owin.ServiceStack
{
    public static class AppBuilderExtensions
    {
        /// <summary>
        /// Adds ServiceStack to the <see cref="IAppBuilder"/> pipeline using a specific <see cref="IServiceStackHandler"/> as a host
        /// </summary>
        /// <param name="builder">The app builder pipeline</param>
        /// <param name="host">The host to use when servicing requests</param>
        /// <returns>The <see cref="IAppBuilder"/></returns>
        public static IAppBuilder UseServiceStack(this IAppBuilder builder, IServiceStackHandler host)
        {
            return builder.Use(async (ctx, next) =>
            {
                var req = new OwinHttpRequest(ctx.Request);
                var res = new OwinHttpResponse(ctx.Response);
                var t = new Task<bool>(() => host.Handle(req, res), TaskCreationOptions.LongRunning);
                t.Start();

                if (!(await t))
                    await next();
            });
        }

        /// <summary>
        /// Adds ServiceStack to the <see cref="IAppBuilder"/> pipeline using a built-in host
        /// </summary>
        /// <param name="builder">The app builder pipeline</param>
        /// <param name="serviceName">The service's name</param>
        /// <param name="assemblies">The assemblies to pass to ServiceStack for service discovery</param>
        /// <returns>The <see cref="IAppBuilder"/></returns>
        public static IAppBuilder UseServiceStack(this IAppBuilder builder, string serviceName, params Assembly[] assemblies)
        {
            var host = new ServiceStackHost(serviceName, assemblies);
            host.Init();
            return builder.UseServiceStack(host);
        }
    }
}
