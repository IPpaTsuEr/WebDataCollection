using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CefSharp;
using ProcessAssistant;
using ProcessAssistant.Logger;
using WebDataCollection.Interface;
using WebDataCollection.Services;

namespace WebDataCollection.Handles
{
    public class ResourceRequestHandle : IResourceRequestHandler
    {
        private readonly IProcessAssistant _processAssistant;

        public ResourceRequestHandle(IProcessAssistant processAssistant)
        {
            _processAssistant = processAssistant;
        }

        public void Dispose()
        {
        }

        public ICookieAccessFilter GetCookieAccessFilter(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request)
        {
            return null;
        }

        public IResourceHandler GetResourceHandler(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request)
        {
            return null;
        }

        public IResponseFilter GetResourceResponseFilter(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, IResponse response)
        {
            if (_processAssistant.CanProcessAjaxData(request.Url, out var symbol))
            {
                var id = request.Identifier;
                var url = request.Url;
                var rp = ResponseDataService.Get(id);
                if (rp == null)
                {
                    rp = new ResponseFilter();
                    rp.Symbol = symbol;
                    ResponseDataService.Add(request.Identifier, rp);
                }
                return rp;
            }
            return null;
        }

        public CefReturnValue OnBeforeResourceLoad(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, IRequestCallback callback)
        {
            return CefReturnValue.Continue;
        }

        public bool OnProtocolExecution(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request)
        {
            return true;
        }

        public void OnResourceLoadComplete(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, IResponse response, UrlRequestStatus status, long receivedContentLength)
        {
            if (status == UrlRequestStatus.Success)
            {
                try
                {
                    var id = request.Identifier;
                    var url = request.Url;
                    var rp = ResponseDataService.Get(id);
                    if (rp != null)
                    {
                        _processAssistant.OnAjaxDataProcess(rp.Symbol, url, rp.ResponseMemoryStream).ContinueWith((r) =>
                         {
                             ResponseDataService.Remove(id);
                             rp.ResponseMemoryStream.Close();
                             rp.ResponseMemoryStream.Dispose();
                         });
                    }
                }
                catch (Exception error)
                {
                    Logger.DefaultLogger.LogError(error,"Ajax数据处理异常 {0}", request.Url);
                }
            }
        }

        public void OnResourceRedirect(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, IResponse response, ref string newUrl)
        {

        }

        public bool OnResourceResponse(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, IResponse response)
        {
            try
            {
                var id = request.Identifier;
                var url = request.Url;
                if (_processAssistant.CanProcessAjaxData(request.Url, out var symbol))
                {
                    var rp = ResponseDataService.Get(id);
                    if (rp == null)
                    {
                        rp = new ResponseFilter();
                        rp.Symbol = symbol;
                        ResponseDataService.Add(request.Identifier, rp);
                    }
                    var content_length = int.Parse(response.Headers["Content-Length"]);
                    rp.SetContentLength(content_length);
                }
            }
            catch { }

            return false;
        }
    }
}
