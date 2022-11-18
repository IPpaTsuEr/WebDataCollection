using CefSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebDataCollection.Handles
{
    public class ResponseFilter : IResponseFilter
    {
        private int ContentLength;
        public MemoryStream ResponseMemoryStream;
        public string Symbol;
        public void Dispose()
        {
            //ResponseMemoryStream?.Dispose();
        }

        public FilterStatus Filter(Stream dataIn, out long dataInRead, Stream dataOut, out long dataOutWritten)
        {
            dataInRead = 0;
            dataOutWritten = 0;
            try
            {
                //没有ContentLength时通过In Stream 是否还有数据进行判断
                if (dataIn == null)
                {
                    return FilterStatus.Done;
                }

                dataInRead = dataIn.Length;
                dataIn.CopyTo(dataOut);
                dataOutWritten = dataIn.Length;

                dataIn.Position = 0;
                dataIn.CopyTo(ResponseMemoryStream);

                if (ResponseMemoryStream.Length == ContentLength)
                    return FilterStatus.Done;
                else
                    return FilterStatus.NeedMoreData;

            }
            catch
            {
                return FilterStatus.Error;
            }
        }

        public bool InitFilter()
        {
            ResponseMemoryStream = new MemoryStream();
            ResponseMemoryStream.Position = 0;
            return true;
        }

        public void SetContentLength(int contentLength)
        {
            ContentLength = contentLength;
        }
    }
}
