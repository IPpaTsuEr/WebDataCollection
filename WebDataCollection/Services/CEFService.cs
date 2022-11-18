using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using CefSharp;
using CefSharp.Wpf;
using ProcessAssistant;
using WebDataCollection.Handles;
using WebDataCollection.Interface;
using WebDataCollection.Model;

namespace WebDataCollection.Services
{
    public class CEFService : ICEFService
    {
        private ChromiumWebBrowser _webBrowser { get; set; }
        private readonly BrowserPage _browserPage;

        private bool _isDevToolsShowing = false;

        public event EventHandler<string> OnNewWindowPopup;
        public event EventHandler OnPageLoaded;

        static CEFService()
        {
            if (!CefSharp.Cef.IsInitialized)
            {
                CefSettings cefSettings = new CefSettings();
                cefSettings.Locale = "zh-CN";
                cefSettings.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/50.0.2661.102 Safari/537.36";
                cefSettings.CachePath = AppDomain.CurrentDomain.SetupInformation.ApplicationBase + @"\Cache";
                cefSettings.PersistSessionCookies = true;

                CefSharp.CefSharpSettings.FocusedNodeChangedEnabled = true;

                CefSharp.Cef.Initialize(cefSettings, true);
            }
        }
        public CEFService(BrowserPage browserPage)
        {
            _browserPage = browserPage;

            _webBrowser = new ChromiumWebBrowser();
            //_webBrowser.JavascriptObjectRepository.Settings.JavascriptBindingApiGlobalObjectName = "CEF_JCaller";
            //_webBrowser.JavascriptObjectRepository.Settings.LegacyBindingEnabled = true;
            ////_webBrowser.JavascriptObjectRepository.Register("",);

            _webBrowser.WpfKeyboardHandler = new CefSharp.Wpf.Experimental.WpfImeKeyboardHandler(_webBrowser);

            _webBrowser.TitleChanged += _webBrowser_TitleChanged;
            _webBrowser.FrameLoadEnd += _webBrowser_FrameLoadEnd;

            _webBrowser.AllowDrop = true;

            var lfh = new LifeSpanHandle();
            lfh.OnNewWindowPopup += Lfh_OnNewWindowPopup;
            _webBrowser.LifeSpanHandler = lfh;

        }

        public void SetResourceHandlerFactory(IProcessAssistant processAssistant)
        {
            _webBrowser.ResourceRequestHandlerFactory = new ResourceRequestHandleFactory(processAssistant);
        }


        private void Lfh_OnNewWindowPopup(object sender, string e)
        {
            OnNewWindowPopup(this, e);
        }

        private void _webBrowser_FrameLoadEnd(object sender, CefSharp.FrameLoadEndEventArgs e)
        {

            if (Uri.TryCreate(_webBrowser.GetBrowser().MainFrame.Url, UriKind.RelativeOrAbsolute, out var mainUrl))
            {
                _browserPage.SetIcon($"{mainUrl.Scheme}://{mainUrl.Host}/favicon.ico");
            }
            _browserPage.Url = _webBrowser.GetBrowser().MainFrame.Url;

            OnPageLoaded?.Invoke(this, null);
        }



        private void _webBrowser_TitleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _browserPage.Title = _webBrowser.Title;
        }

        public ChromiumWebBrowser GetBrowser()
        {
            return _webBrowser;
        }

        public void LoadUrl(string url)
        {
            _browserPage.Url = url;
            _webBrowser.Load(url);
        }


        public void DevToolsSwitcher()
        {
            if (_isDevToolsShowing)
            {
                _webBrowser.CloseDevTools();
            }
            else
            {
                _webBrowser.ShowDevTools();
            }
        }



        public async Task<JavascriptResponse> ExcuteScript(string jsCode)
        {
            return await _webBrowser.GetMainFrame().EvaluateScriptAsync(jsCode);
        }

        public Task CallMethod(string jsMethodName, params object[] args)
        {
            _webBrowser.ExecuteScriptAsync(jsMethodName, args);
            return Task.CompletedTask;
        }

        public async Task WaitePageLoading(CancellationToken cancellationToken)
        {
            while (true)
            {
                await Task.Delay(500);
                cancellationToken.ThrowIfCancellationRequested();
                var isLodede = App.Current.Dispatcher.Invoke(
                    () =>
                    {
                        return _browserPage.Service.GetBrowser().GetBrowser().IsLoading;
                    });
                if (!isLodede)
                    break;
            }
        }

        public void Refresh()
        {
            _webBrowser.Reload();
        }
    }
}
