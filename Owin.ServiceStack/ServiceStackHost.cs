using Funq;
using ServiceStack.Common;
using ServiceStack.Configuration;
using ServiceStack.Html;
using ServiceStack.IO;
using ServiceStack.Logging;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface.ServiceModel;
using ServiceStack.ServiceModel.Serialization;
using ServiceStack.WebHost.Endpoints;
using ServiceStack.WebHost.Endpoints.Extensions;
using ServiceStack.WebHost.Endpoints.Support;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Owin.ServiceStack
{
    /// <summary>
    /// A <see cref="IAppHost"/> specifically for the Owin Pipeline
    /// </summary>
    public class ServiceStackHost : IAppHost, IHasContainer, IServiceStackHandler
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ServiceStackHost));
        private readonly DateTime startTime;

        protected ServiceStackHost()
        {
            this.startTime = DateTime.UtcNow;
            Log.Info("Begin Initializing Application...");

            EndpointHostConfig.SkipPathValidation = true;
        }

        public ServiceStackHost(string serviceName, params Assembly[] assembliesWithServices)
            : this()
        {
            EndpointHost.ConfigureHost(this, serviceName, CreateServiceManager(assembliesWithServices));
        }

        protected virtual ServiceManager CreateServiceManager(params Assembly[] assembliesWithServices)
        {
            return new ServiceManager(assembliesWithServices);
        }

        public void Init()
        {
            var serviceManager = EndpointHost.Config.ServiceManager;
            if (serviceManager != null)
            {
                serviceManager.Init();
                Configure(EndpointHost.Config.ServiceManager.Container);
            }
            else
            {
                Configure(null);
            }

            EndpointHost.AfterInit();

            var elapsed = DateTime.UtcNow - this.startTime;
            Log.InfoFormat("Initializing Application took {0}ms", elapsed.TotalMilliseconds);
        }

        public void Configure(Container container)
        {
        }

        // Handle the processing of a request in here.
        public bool Handle(IHttpRequest httpReq, IHttpResponse httpRes)
        {
            try
            {
                var handler = ServiceStackHttpHandlerFactory.GetHandler(httpReq);
                var serviceStackHandler = handler as IServiceStackHttpHandler;

                if (serviceStackHandler == null)
                    throw new NotImplementedException("Cannot execute handler: " + handler + " at PathInfo: " + httpReq.PathInfo);

                if (handler is NotFoundHttpHandler)
                    return false;

                var restHandler = serviceStackHandler as RestHandler;
                if (restHandler != null)
                    httpReq.OperationName = restHandler.RestPath.RequestType.Name;

                serviceStackHandler.ProcessRequest(httpReq, httpRes, httpReq.OperationName ?? string.Empty);
                httpRes.Close();
                return true;
            }
            catch (Exception ex)
            {
                var error = string.Format("Error this.ProcessRequest(context): [{0}]: {1}", ex.GetType().Name, ex.Message);
                Log.ErrorFormat(error);

                try
                {
                    var errorResponse = new ErrorResponse
                    {
                        ResponseStatus = new ResponseStatus
                        {
                            ErrorCode = ex.GetType().Name,
                            Message = ex.Message,
                            StackTrace = ex.StackTrace,
                        }
                    };

                    var requestCtx = new HttpRequestContext(httpReq, httpRes, errorResponse);
                    var contentType = requestCtx.ResponseContentType;

                    var serializer = EndpointHost.ContentTypeFilter.GetResponseSerializer(contentType);
                    if (serializer == null)
                    {
                        contentType = EndpointHost.Config.DefaultContentType;
                        serializer = EndpointHost.ContentTypeFilter.GetResponseSerializer(contentType);
                    }

                    httpRes.StatusCode = 500;
                    httpRes.ContentType = contentType;

                    serializer(requestCtx, errorResponse, httpRes);

                    httpRes.Close();
                    return true;
                }
                catch (Exception errorEx)
                {
                    error = string.Format("Error this.ProcessRequest(context)(Exception while writing error to the response): [{0}]: {1}", errorEx.GetType().Name, errorEx.Message);
                    Log.ErrorFormat(error);
                    return false;
                }
            }
        }

        protected void SetConfig(EndpointHostConfig config)
        {
            if (config.ServiceName == null)
                config.ServiceName = EndpointHost.Config.ServiceName;

            //if (config.ServiceManager == null)
            //config.ServiceManager = EndpointHost.Config.ServiceManager;

            config.ServiceManager.ServiceController.EnableAccessRestrictions = config.EnableAccessRestrictions;

            EndpointHost.Config = config;

            JsonDataContractSerializer.Instance.UseBcl = config.UseBclJsonSerializers;
            JsonDataContractDeserializer.Instance.UseBcl = config.UseBclJsonSerializers;
        }


        public virtual void Release(object instance)
        {
            try
            {
                var iocAdapterReleases = Container.Adapter as IRelease;
                if (iocAdapterReleases != null)
                {
                    iocAdapterReleases.Release(instance);
                }
                else
                {
                    var disposable = instance as IDisposable;
                    if (disposable != null)
                        disposable.Dispose();
                }
            }
            catch {/*ignore*/}
        }

        public virtual void OnEndRequest()
        {
            foreach (var item in HostContext.Instance.Items.Values)
            {
                Release(item);
            }

            HostContext.Instance.EndRequest();
        }

        public Container Container 
            => EndpointHost.Config.ServiceManager.Container;

        public void RegisterAs<T, TAs>() where T : TAs 
            => Container.RegisterAutoWiredAs<T, TAs>();

        public void Register<T>(T instance) 
            => Container.Register(instance);

        public T TryResolve<T>() 
            => Container.TryResolve<T>();

        protected IServiceController ServiceController 
            => EndpointHost.Config.ServiceController;

        public IServiceRoutes Routes => EndpointHost.Config.ServiceController.Routes;

        public Dictionary<Type, Func<IHttpRequest, object>> RequestBinders 
            => EndpointHost.ServiceManager.ServiceController.RequestTypeFactoryMap;

        public IContentTypeFilter ContentTypeFilters 
            => EndpointHost.ContentTypeFilter;

        public List<Action<IHttpRequest, IHttpResponse>> PreRequestFilters 
            => EndpointHost.RawRequestFilters;

        public List<Action<IHttpRequest, IHttpResponse, object>> RequestFilters 
            => EndpointHost.RequestFilters;

        public List<Action<IHttpRequest, IHttpResponse, object>> ResponseFilters 
            => EndpointHost.ResponseFilters;

        public List<IViewEngine> ViewEngines 
            => EndpointHost.ViewEngines;

        public List<HttpHandlerResolverDelegate> CatchAllHandlers 
            => EndpointHost.CatchAllHandlers;

        public EndpointHostConfig Config 
            => EndpointHost.Config;

        public List<IPlugin> Plugins 
            => EndpointHost.Plugins;

        public virtual IServiceRunner<TRequest> CreateServiceRunner<TRequest>(ActionContext actionContext) 
            => new ServiceRunner<TRequest>(this, actionContext);

        public virtual string ResolveAbsoluteUrl(string virtualPath, IHttpRequest httpReq) 
            => httpReq.GetAbsoluteUrl(virtualPath);

        public HandleUncaughtExceptionDelegate ExceptionHandler
        {
            get { return EndpointHost.ExceptionHandler; }
            set { EndpointHost.ExceptionHandler = value; }
        }

        public HandleServiceExceptionDelegate ServiceExceptionHandler
        {
            get { return EndpointHost.ServiceExceptionHandler; }
            set { EndpointHost.ServiceExceptionHandler = value; }
        }

        public IVirtualPathProvider VirtualPathProvider
        {
            get { return EndpointHost.VirtualPathProvider; }
            set { EndpointHost.VirtualPathProvider = value; }
        }

        public virtual void LoadPlugin(params IPlugin[] plugins)
        {
            foreach (var plugin in plugins)
            {
                try
                {
                    plugin.Register(this);
                }
                catch (Exception ex)
                {
                    Log.Warn("Error loading plugin " + plugin.GetType().Name, ex);
                }
            }
        }

        public void RegisterService(Type serviceType, params string[] atRestPaths)
        {
            var genericService = EndpointHost.Config.ServiceManager.RegisterService(serviceType);
            if (genericService != null)
            {
                var requestType = genericService.GetGenericArguments()[0];
                foreach (var atRestPath in atRestPaths)
                {
                    this.Routes.Add(requestType, atRestPath, null);
                }
            }
            else
            {
                var reqAttr = serviceType.GetCustomAttributes(true).OfType<DefaultRequestAttribute>().FirstOrDefault();
                if (reqAttr != null)
                {
                    foreach (var atRestPath in atRestPaths)
                    {
                        this.Routes.Add(reqAttr.RequestType, atRestPath, null);
                    }
                }
            }
        }
    }
}
