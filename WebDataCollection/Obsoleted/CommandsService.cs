using CefSharp;
using CefSharp.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;

namespace WebDataCollection.Obsoleted
{
    public enum ProcessType
    {
        IdName,
        ClassName,
        TagName,
        Index,
        Property,
        Find,
        Click,
        SetText,
        Text,
        SetProperty,
        SetTextProperty,
        ScrollX,
        ScrollY,
        PageHeight
    }
    public class TargetProcess
    {
        public ProcessType Target { get; set; }
        public string TargetData { get; set; }
    }

    public class TargetDescriptor
    {
        public string TaregtUrl { get; set; }
        public List<TargetProcess> Sequence { get; set; }

    }

    public class CommandsService
    {
        private ChromiumWebBrowser _webBrowser;

        public CommandsService(ChromiumWebBrowser webBrowser)
        {
            _webBrowser = webBrowser;
        }


        private string ConvertToJavaScript(TargetDescriptor target)
        {
            var data = new StringBuilder();
            data.Append("document.getElementsBy");
            foreach (var item in target.Sequence)
            {
                switch (item.Target)
                {
                    case ProcessType.IdName:
                        data.Replace("getElementsBy", "getElementBy").Append($"Id(\"{item.TargetData}\")");
                        break;
                    case ProcessType.ClassName:
                        data.Append($"ClassName(\"{item.TargetData}\")");
                        break;
                    case ProcessType.TagName:

                        data.Append($"TagName(\"{item.TargetData}\")");
                        break;
                    case ProcessType.Index:
                        data.Append($"[{item.TargetData}]");
                        break;
                    case ProcessType.Find:

                        break;
                    case ProcessType.Click:
                        data.Append(".click()");
                        break;
                    case ProcessType.SetText:
                        data.Append($".text = \"{item.TargetData}\"");
                        break;
                    case ProcessType.Text:
                        data.Append(".text");
                        break;
                    case ProcessType.SetProperty:
                        data.Append($" = {item.TargetData}");
                        break;
                    case ProcessType.SetTextProperty:
                        data.Append($" = \"{item.TargetData}\"");
                        break;
                    case ProcessType.Property:
                        //data.Insert(0, "(function () { return ").Append($".{item.TargetData};").Append("})();");
                        data.Append($".{item.TargetData}");
                        break;
                    case ProcessType.ScrollX:
                        data.Clear().Append($"window.scroll({{ top: 0, left: {item.TargetData}, behavior: 'smooth' }});");
                        break;
                    case ProcessType.ScrollY:
                        data.Clear().Append($"window.scroll({{ top:  {item.TargetData}, left: 0, behavior: 'smooth' }});");
                        break;
                    case ProcessType.PageHeight:
                        data.Append("document.body.clientHeight");
                        break;
                    default:
                        break;
                }
            }
            return data.ToString();
        }

        public JavascriptResponse Excute(TargetDescriptor target)
        {
            var jsCode = ConvertToJavaScript(target);
            return new JavascriptResponse();
        }
    }
}
