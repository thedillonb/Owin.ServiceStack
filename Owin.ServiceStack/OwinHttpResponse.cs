using Microsoft.Owin;
using ServiceStack.ServiceHost;
using System;
using System.IO;
using System.Text;

namespace Owin.ServiceStack
{
    public class OwinHttpResponse : IHttpResponse
    {
        private readonly IOwinResponse _response;

        public OwinHttpResponse(IOwinResponse response)
        {
            _response = response;
        }

        public string ContentType
        {
            get
            {
                return _response.ContentType;
            }

            set
            {
                _response.ContentType = value;
            }
        }

        public ICookies Cookies
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool IsClosed
        {
            get
            {
                return false;
            }
        }

        public object OriginalResponse
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public Stream OutputStream
        {
            get
            {
                return null;
            }
        }

        public int StatusCode
        {
            get { return _response.StatusCode; }
            set { _response.StatusCode = value; }
        }

        public string StatusDescription
        {
            get { return string.Empty; }

            set { }
        }

        public void AddHeader(string name, string value) => _response.Headers[name] = value;

        public void Close()
        {

        }

        public void End()
        {

        }

        public void Flush()
        {

        }

        public void Redirect(string url)
        {
            throw new NotImplementedException();
        }

        public void SetContentLength(long contentLength)
        {
            throw new NotImplementedException();
        }

        public void Write(string text) => _response.Write(Encoding.UTF8.GetBytes(text));
    }
}
