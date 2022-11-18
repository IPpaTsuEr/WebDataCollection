using CefSharp.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using WebDataCollection.Interface;
using WebDataCollection.Notify;
using WebDataCollection.Services;

namespace WebDataCollection.Model
{
    public class BrowserPage : MvvmNotify
    {
        private string _Url;
        public string Url { get { return _Url; } set { _Url = value; Notify(); } }

        private string _Title;
        public string Title { get { return _Title; } set { _Title = value; Notify(); } }

        private ImageSource _Icon;
        public ImageSource Icon { get { return _Icon; } set { _Icon = value; Notify(); } }

        public Control Browser { get => Service.GetBrowser(); set { Notify(); } }

        public ChromiumWebBrowser  WebBrowser { get => Service.GetBrowser(); }
        public ICEFService Service { get; set; }


        public void SetIcon(string iconImageUrl)
        {
            BitmapImage iconImage = null;

            App.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    iconImage = new BitmapImage(new Uri(iconImageUrl));
                }
                catch { }
            });
            Icon = iconImage;
        }

        public BrowserPage()
        {
            Service = new CEFService(this);
            Browser = Service.GetBrowser();
        }
    }
}
