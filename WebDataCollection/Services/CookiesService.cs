using CefSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebDataCollection.Services
{
    public class CookiesService
    {

        public Task<bool> SetCookie(string url, Cookie cookie)
        {
            var cm = CefSharp.Cef.GetGlobalCookieManager();
            return cm.SetCookieAsync(url, cookie);
        }
        public Task<int> UnsetCookie(string url, string cookieName)
        {
            var cm = CefSharp.Cef.GetGlobalCookieManager();
            return cm.DeleteCookiesAsync(url, cookieName);
        }

        public List<Cookie> GetCookies(string url, bool httpOnly)
        {
            var cm = CefSharp.Cef.GetGlobalCookieManager();
            var cv = new CookieVisitor();
            var result = new List<Cookie>();
            cv.OnVisitor += (s, c) =>
            {
                result.Add(c);
            };
            if (cm.VisitUrlCookies(url, httpOnly, cv))
            {
                return result;
            }
            return null;
        }

    }

    public class CookieVisitor : ICookieVisitor
    {
        public event EventHandler<Cookie> OnVisitor;
        public void Dispose()
        {

        }

        public bool Visit(Cookie cookie, int count, int total, ref bool deleteCookie)
        {
            OnVisitor?.Invoke(this, cookie);
            return true;
        }
    }
}
