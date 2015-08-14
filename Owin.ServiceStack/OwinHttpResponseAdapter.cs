using Microsoft.Owin;
using ServiceStack.ServiceHost;
using System.IO;
using System.Text;

namespace Owin.ServiceStack
{
    class OwinHttpResponseAdapter : IHttpResponse
    {
        private readonly IOwinResponse _response;

        public OwinHttpResponseAdapter(IOwinResponse response)
        {
            _response = response;
        }

        public string ContentType
        {
            get { return _response.ContentType; }
            set { _response.ContentType = value; }
        }

        private ICookies _cookies;
        public ICookies Cookies
        {
            get
            {
                if (_cookies == null)
                    _cookies = new OwinCookiesAdapter(_response);
                return _cookies;
            }
            set { _cookies = value; }
        }

        public int StatusCode
        {
            get { return _response.StatusCode; }
            set { _response.StatusCode = value; }
        }

        public string StatusDescription
        {
            get { return _response.Get<string>("owin.ResponseReasonPhrase"); }
            set { _response.Set("owin.ResponseReasonPhrase", value); }
        }

        public void Close() { }

        public void End() { }

        public void Flush() { }

        public bool IsClosed => false;

        public Stream OutputStream => _response.Body;

        public void AddHeader(string name, string value) => _response.Headers[name] = value;

        public object OriginalResponse => _response;

        public void Redirect(string url) => _response.Redirect(url);

        public void SetContentLength(long contentLength) => _response.ContentLength = contentLength;

        public void Write(string text)
        {
            var data = Encoding.UTF8.GetBytes(text);
            _response.Write(data);
        }
    }
}
