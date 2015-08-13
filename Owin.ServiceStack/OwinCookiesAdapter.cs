using ServiceStack.ServiceHost;
using System.Net;
using Microsoft.Owin;

namespace Owin.ServiceStack
{
    /// <summary>
    /// A concret implementation of <see cref="ICookies"/> which adapts the OWIN cookies store to the Service Stack
    /// </summary>
    class OwinCookiesAdapter : ICookies
    {
        private readonly IOwinResponse _response;

        public OwinCookiesAdapter(IOwinResponse response)
        {
            _response = response;
        }

        public void AddCookie(Cookie cookie)
        {
            _response.Cookies.Append(cookie.Name, cookie.Value, new CookieOptions()
            {
                Domain = cookie.Domain,
                Expires = cookie.Expires,
                HttpOnly = cookie.HttpOnly,
                Secure = cookie.Secure,
                Path = cookie.Path
            });
        }

        public void AddPermanentCookie(string cookieName, string cookieValue, bool? secureOnly = default(bool?))
        {
            _response.Cookies.Append(cookieName, cookieValue, new CookieOptions() { Secure = secureOnly ?? false });
        }

        public void AddSessionCookie(string cookieName, string cookieValue, bool? secureOnly = default(bool?))
        {
            _response.Cookies.Append(cookieName, cookieValue, new CookieOptions() { Secure = secureOnly ?? false });
        }

        public void DeleteCookie(string cookieName)
        {
            _response.Cookies.Delete(cookieName);
        }
    }
}
