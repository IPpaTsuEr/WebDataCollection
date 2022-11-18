using CefSharp;
using ProcessAssistant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace WebDataCollection.Handles
{
    public class ResourceRequestHandleFactory : IResourceRequestHandlerFactory
    {
        private readonly IProcessAssistant _processAssistant;

        public ResourceRequestHandleFactory(IProcessAssistant processAssistant)
        {
            _processAssistant = processAssistant;
        }

        public bool HasHandlers => true;

        public IResourceRequestHandler GetResourceRequestHandler(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, bool isNavigation, bool isDownload, string requestInitiator, ref bool disableDefaultHandling)
        {
            return new ResourceRequestHandle(_processAssistant);
        }
       
    }
}
