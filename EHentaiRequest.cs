using EHentaiAPI.ExtendFunction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Wbooru.Network;

namespace WbooruPlugin.EHentai
{
    public class EHentaiRequest : IRequest
    {
        private string url;

        public EHentaiRequest(string url)
        {
            this.url = url;
        }

        public WebHeaderCollection Headers { get; set; } = new WebHeaderCollection();
        public CookieContainer Cookies { get; set; } = new CookieContainer();
        public string Method { get; set; } = "GET";
        public HttpContent Content { get; set; } = default;

        public async Task<IResponse> SendAsync()
        {
            var response = await RequestHelper.CreateDeafultAsync(url, async req =>
             {
                 req.Headers = Headers;
                 req.CookieContainer = Cookies;
                 req.Method = Method;

                 if (Content != null)
                 {
                     await Content.CopyToAsync(await req.GetRequestStreamAsync());
                     req.ContentType = Content.Headers.ContentType.ToString();
                     req.ContentLength = Content.Headers.ContentLength ?? 0;
                 }
             });

            return new DefaultResponseImpl(response as HttpWebResponse);
        }
    }
}
