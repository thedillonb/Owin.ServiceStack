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
    class OwinHttpRequest : IHttpRequest
    {
        private static readonly string physicalFilePath;
        private IOwinRequest _request;
        private MemoryStream bufferedStream;

        static OwinHttpRequest()
        {
            physicalFilePath = "~".MapHostAbsolutePath();
        }

        public OwinHttpRequest(IOwinRequest request)
        {
            _request = request;
        }

        public IFile[] Files { get; set; }

        public string AbsoluteUri => _request.Uri.AbsoluteUri;

        public string[] AcceptTypes => _request.Accept.Split(';');

        public string ApplicationFilePath => physicalFilePath;

        public string HttpMethod => _request.Method;

        public bool IsSecureConnection => _request.IsSecure;

        public string ContentType => _request.ContentType;

        public IDictionary<string, Cookie> Cookies => _request.Cookies.ToDictionary(x => x.Key, x => new Cookie(x.Key, x.Value));

        public bool IsLocal => true;

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
                    var form = _request.ReadFormAsync().Result;
                    var formData = new NameValueCollection();
                    foreach (var f in form)
                        formData.Add(f.Key, string.Join("", f.Value));
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

        public string OperationName { get; set; }

        public object OriginalRequest => _request;

        public string PathInfo => _request.Path.ToString();

        public string RawUrl => _request.Uri.AbsoluteUri;

        public string RemoteIp => _request.RemoteIpAddress;

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
            get
            {
                throw new NotImplementedException();
            }
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

        public Stream InputStream => bufferedStream ?? _request.Body;

        public string UserAgent => _request.Headers.ContainsKey("user-agent") ? _request.Headers["user-agent"] : null;

        public string UserHostAddress => _request.RemoteIpAddress;

        public string XForwardedFor => _request.Headers.ContainsKey(HttpHeaders.XForwardedFor) ? _request.Headers[HttpHeaders.XForwardedFor] : null;

        public string XRealIp => _request.Headers.ContainsKey(HttpHeaders.XRealIp) ? _request.Headers[HttpHeaders.XRealIp] : null;

        public T TryResolve<T>() => EndpointHost.AppHost.TryResolve<T>();

        public string GetRawBody()
        {
            if (bufferedStream != null)
                return bufferedStream.ToArray().FromUtf8Bytes();

            using (var reader = new StreamReader(InputStream))
                return reader.ReadToEnd();
        }

    }
}
