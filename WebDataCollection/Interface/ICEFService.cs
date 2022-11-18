using CefSharp;
using CefSharp.Wpf;
using ProcessAssistant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebDataCollection.Services;

namespace WebDataCollection.Interface
{
    public interface ICEFService
    {
        event EventHandler<string> OnNewWindowPopup;
        event EventHandler OnPageLoaded;

        ChromiumWebBrowser GetBrowser();
        void LoadUrl(string url);
        void Refresh();
        void DevToolsSwitcher();
        Task<JavascriptResponse> ExcuteScript(string jsCode);
        Task CallMethod(string jsMethodName, params object[] args);

        void SetResourceHandlerFactory(IProcessAssistant processAssistant);

        Task WaitePageLoading(CancellationToken cancellationToken);
    }
}
