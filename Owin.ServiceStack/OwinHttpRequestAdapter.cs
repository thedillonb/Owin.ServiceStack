using Microsoft.Owin;
using ServiceStack.Common.Utils;
using ServiceStack.Common.Web;
using ServiceStack.ServiceHost;
using ServiceStack.Text;
using ServiceStack.WebHost.Endpoints;
using ServiceStack.WebHost.Endpoints.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;

namespace Owin.ServiceStack
{
    /// <summary>
    /// An implementation of <see cref="IHttpRequest"/> based on the <see cref="IOwinRequest"/>
    /// </summary>
    class OwinHttpRequestAdapter : IHttpRequest
    {
        private static readonly string[] FormContentTypes = new[] { "application/x-www-form-urlencoded" };
        private static readonly string physicalFilePath = "~".MapHostAbsolutePath();
        private IOwinRequest _request;
        private MemoryStream bufferedStream;

        public OwinHttpRequestAdapter(IOwinRequest request)
        {
            _request = request;
        }

        public string AbsoluteUri => _request.Uri.AbsoluteUri.TrimEnd('/');

        public string[] AcceptTypes => _request.Accept.Split(',');

        public string ApplicationFilePath => physicalFilePath;

        public string HttpMethod => _request.Method;

        public bool IsSecureConnection => _request.IsSecure;

        public string ContentType => _request.ContentType;

        public IDictionary<string, Cookie> Cookies => _request.Cookies.ToDictionary(x => x.Key, x => new Cookie(x.Key, x.Value));

        public bool IsLocal => true;

        public string OperationName { get; set; }

        public object OriginalRequest => _request;

        public string RawUrl => _request.Uri.AbsolutePath;

        public string RemoteIp => _request.RemoteIpAddress;

        public Stream InputStream => bufferedStream ?? _request.Body;

        public string UserAgent => _request.Headers.ContainsKey("user-agent") ? _request.Headers["user-agent"] : null;

        public string UserHostAddress => _request.RemoteIpAddress;

        public string XForwardedFor => _request.Headers.ContainsKey(HttpHeaders.XForwardedFor) ? _request.Headers[HttpHeaders.XForwardedFor] : null;

        public string XRealIp => _request.Headers.ContainsKey(HttpHeaders.XRealIp) ? _request.Headers[HttpHeaders.XRealIp] : null;

        public T TryResolve<T>() => EndpointHost.AppHost.TryResolve<T>();

        private string pathInfo;
        public string PathInfo
        {
            get
            {
                if (this.pathInfo == null)
                {
                    var mode = EndpointHost.Config.ServiceStackHandlerFactoryPath;

                    var pos = RawUrl.IndexOf("?");
                    if (pos != -1)
                    {
                        var path = RawUrl.Substring(0, pos);
                        this.pathInfo = global::ServiceStack.WebHost.Endpoints.Extensions.HttpRequestExtensions.GetPathInfo(
                            path,
                            mode,
                            mode ?? "");
                    }
                    else
                    {
                        this.pathInfo = RawUrl;
                    }

                    this.pathInfo = this.pathInfo.UrlDecode();
                    this.pathInfo = NormalizePathInfo(pathInfo, mode);
                }
                return this.pathInfo;
            }
        }

        private IFile[] _files;
        public IFile[] Files
        {
            get
            {
                if (_files == null)
                    _files = new IFile[0];
                return _files;
            }

        }

        public long ContentLength
        {
            get
            {
                long ret = 0;
                if (_request.Headers.ContainsKey("content-length"))
                    long.TryParse(_request.Headers["content-length"], out ret);
                return ret;
            }
        }

        private NameValueCollection _formData;
        public NameValueCollection FormData
        {
            get
            {
                if (_formData == null)
                {
                    var formData = new NameValueCollection();

                    var contentType = ContentType?.Split(new[] { ";" }, 2, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? ContentType;
                    if (FormContentTypes.Any(x => string.Equals(contentType, x, StringComparison.OrdinalIgnoreCase)))
                    {
                        var form = _request.ReadFormAsync().Result;
                        foreach (var f in form)
                            formData.Add(f.Key, string.Join("", f.Value));
                    }

                    _formData = formData;
                }

                return _formData;
            }
        }

        private NameValueCollection _headers;
        public NameValueCollection Headers
        {
            get
            {
                if (_headers == null)
                {
                    var c = new NameValueCollection();
                    foreach (var r in _request.Headers)
                        c.Add(r.Key, string.Join("", r.Value));
                    _headers = c;
                }

                return _headers;
            }
        }

        private Dictionary<string, object> items;
        public Dictionary<string, object> Items
        {
            get
            {
                if (items == null)
                    items = new Dictionary<string, object>();
                return items;
            }
        }

        private NameValueCollection _queryString;
        public NameValueCollection QueryString
        {
            get
            {
                if (_queryString == null)
                {
                    var s = new NameValueCollection();
                    foreach (var pair in _request.Query.Where(x => x.Value != null))
                        s.Add(pair.Key, string.Join(",", pair.Value));
                    _queryString = s;
                }

                return _queryString;
            }
        }

        private string responseContentType;
        public string ResponseContentType
        {
            get { return responseContentType ?? (responseContentType = this.GetResponseContentType()); }
            set { this.responseContentType = value; }
        }

        public Uri UrlReferrer
        {
            get { return null; }
        }

        public bool UseBufferedStream
        {
            get { return bufferedStream != null; }
            set
            {
                bufferedStream = value
                    ? bufferedStream ?? new MemoryStream(_request.Body.ReadFully())
                    : null;
            }
        }

        public string GetRawBody()
        {
            if (bufferedStream != null)
                return bufferedStream.ToArray().FromUtf8Bytes();

            using (var reader = new StreamReader(InputStream))
                return reader.ReadToEnd();
        }

        private static string NormalizePathInfo(string pathInfo, string handlerPath)
        {
            if (handlerPath != null && pathInfo.TrimStart('/').StartsWith(
                handlerPath, StringComparison.InvariantCultureIgnoreCase))
            {
                return pathInfo.TrimStart('/').Substring(handlerPath.Length);
            }

            return pathInfo;
        }
    }
}
