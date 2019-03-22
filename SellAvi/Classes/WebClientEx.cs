using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using NLog;
using SellAvi.Browser;

namespace SellAvi.Classes
{
    public class WebClientEx : WebClient
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private string _responseHtml;

        public WebClientEx(CookieContainer container)
        {
            CookieContainer = container;
        }

        public WebClientEx()
        {
            DefaultHeaders = new WebHeaderCollection();
            CookieContainer = new CookieContainer();
        }

        public WebHeaderCollection DefaultHeaders { get; set; }
        public int RequestTimeOut { get; set; }
        public CefSharpWrapper CHW { get; set; }

        public Uri ResponseUri { get; set; } = new Uri("https://www.avito.ru/");

        public string ResponseHtml
        {
            get => _responseHtml;
            set
            {
                _responseHtml = value;
                OnHtmlChanged();
            }
        }

        public CookieContainer CookieContainer { get; set; }

        public List<KeyValuePair<string, string>> CookieContainerList
        {
            get
            {
                var container = new List<KeyValuePair<string, string>>();
                var cookieString = CookieContainer.GetCookieHeader(new Uri("https://www.avito.ru"));
                //var cookie = "u=21xfv06a.ctcggq.f88c91n7sl; _ym_uid=1474973500233043501; __gads=ID=04591024fc728a3c:T=1474973499:S=ALNI_MY_VrTcBj5tzLvk6Spe5nUKqBTjaA; vishnu1.new_chat=1; vishnu1.cuid=bafbda33-f7ed-42b9-a733-ae2cf65560d7; vishnu1.visible=0; _jsuid=4126337624; dfp_group=76; reklama-no-request=1; f=4.b53ee41b77d9840ae5ba07059b0d202f6e619f2005f5c6dc6e619f2005f5c6dc6e619f2005f5c6dc6e619f2005f5c6dc6e619f2005f5c6dc6e619f2005f5c6dc6e619f2005f5c6dc6e619f2005f5c6dc6e619f2005f5c6dc6e619f2005f5c6dc5b68b402642848be5b68b402642848bead33eaef55dd0da1f7d75d53b929b44c44620aa09dfab02de75a2b007093b89d05886bb864a616652f4891ba472e4f2618dc79c78ea31ba1ea48e2d99c5312aaffe65fd77b784b7bffe65fd77b784b7bb8a109ce707ef6137c6d6c44a42cb1c70176a16018517b5da399e993193588ae728b89f8cc435269728b89f8cc435269728b89f8cc435269728b89f8cc435269ffe65fd77b784b7b862357a052e106f23f601feec47f73646b10d486f2e98b94ca112ee97349d08fcd1583c110bfb60eb5625d268f549a9fb1bb2a353daea3d2983634ac97fc2097e59bc492ed15ed1b9421779967acfec4d9fcb3420731623d7c6d6c44a42cb1c77c6d6c44a42cb1c7ea48e2d99c5312aa9f8aee6ba1bc3a09; _ym_isad=1; nfh=12d0aa5dfd78cb805725fedb5f7846a3; _ga=GA1.2.150182955.1474973500; sessid=9389f1e1a56fed49d933ff6b6f791491.1490605768; crtg_rta=cravadb240%3D1%3B; _ym_visorc_34241905=b; __utmt=1; auth=1; anid=899063286%3Bb0be8baa939e3b5d531107058fdf7046%3B1; weborama-viewed=1; __utma=99926606.150182955.1474973500.1490698664.1490705486.154; __utmb=99926606.3.10.1490705488; __utmc=99926606; __utmz=99926606.1490698664.153.27.utmcsr=google|utmccn=(organic)|utmcmd=organic|utmctr=(not%20provided); v=1490705485";

                //container.Add(new KeyValuePair<string, string>("User-Agent", "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/55.0.2883.75 Safari/537.36"));
                //container.Add(new KeyValuePair<string, string>("Host", "socket2.avito.ru"));

                foreach (var cookieElement in cookieString.Split(';'))
                {
                    var key = cookieElement.Split('=')[0].Trim();
                    var value = cookieElement.Split('=')[1].Trim();
                    container.Add(new KeyValuePair<string, string>(key, value));
                }

                return container;
            }
        }

        public event EventHandler HtmlChanged;

        /// <summary>
        ///     CookieContainer.SetCookies(new Uri("https://www.avito.ru"),response.Headers[HttpResponseHeader.SetCookie]);
        /// </summary>
        /// <param name="cookieString"></param>
        /// <param name="url"></param>
        public void FillCookieContainer(string cookieString, string url)
        {
            // 1.
            //foreach (var cookieElement in cookieString.Split(';'))
            //{
            //    CookieContainer.SetCookies(new Uri(url), cookieElement.Trim());
            //}

            // 2.
            //http://stackoverflow.com/questions/1047669/cookiecontainer-bug
            //CookieContainer.SetCookies(new Uri(url), cookieString.Replace(";", ","));

            // 3.
            foreach (var cookieElement in cookieString.Split(';'))
            {
                var cName = cookieElement.Split('=')[0].Trim();
                var cValue = cookieElement.Split('=')[1].Trim();
                CookieContainer.Add(new Cookie(cName, cValue, "/", ".avito.ru"));
            }
        }

        public new string UploadValues(Uri address, NameValueCollection nvc)
        {
            var r = base.UploadValues(address, nvc);
            return _responseHtml = Encoding.GetString(r);
        }

        public new string DownloadString(Uri address)
        {
            return _responseHtml = base.DownloadString(address);
        }

        public new string UploadString(Uri address, string data)
        {
            return _responseHtml = base.UploadString(address, data);
        }

        public string DownloadStringWithNotification(Uri address)
        {
            ResponseHtml = DownloadString(address);
            return ResponseHtml;
        }

        public void OnHtmlChanged()
        {
            if (HtmlChanged != null) HtmlChanged(this, EventArgs.Empty);
        }

        public void BrowserNavigationNeeded(Uri urlTo)
        {
            if (HtmlChanged != null) HtmlChanged(this, new NavigationEventArgs {NavigateTo = urlTo});
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            var r = base.GetWebRequest(address);
            var request = r as HttpWebRequest;
            if (request != null)
            {
                request.Timeout = RequestTimeOut == 0 ? request.Timeout : RequestTimeOut;
                // Важна последовательность заголовков!
                request.Accept = DefaultHeaders[HttpRequestHeader.Accept];
                request.Referer =
                    DefaultHeaders[HttpRequestHeader.Referer] ??
                    ResponseUri.AbsoluteUri; //OriginalString; //Uri.ToSting() содержит недопустимые знаки управления
                request.ContentType = DefaultHeaders[HttpRequestHeader.ContentType];
                request.UserAgent = DefaultHeaders[HttpRequestHeader.UserAgent];
                request.CookieContainer = CookieContainer;
            }

            return r;
        }

        protected override WebResponse GetWebResponse(WebRequest request, IAsyncResult result)
        {
            try
            {
                var response = base.GetWebResponse(request, result);
                //ReadHtmlContent(response); //BUG: не задействовать метод поскольку он закрывает соедиение
                ReadCookies(response);
                ReadResponseUri(response);
                return response;
            }
            catch (Exception ex)
            {
                Logger.Error("Запрос страницы {0} завершился с ошибкой >> {1}", request.RequestUri, ex.Message);
            }

            return null;
        }

        protected override WebResponse GetWebResponse(WebRequest request)
        {
            try
            {
                var response = base.GetWebResponse(request);
                //ReadHtmlContent(response);
                ReadResponseUri(response);
                ReadCookies(response);
                return response;
            }
            catch (Exception ex)
            {
                Logger.Error("Запрос страницы {0} завершился с ошибкой >> {1}", request.RequestUri, ex.Message);
            }

            return null;
        }

        private void ReadCookies(WebResponse r)
        {
            var response = r as HttpWebResponse;
            if (response != null)
            {
                var cookies = response.Cookies;
                CookieContainer.Add(cookies);
                //OR
                //CookieContainer.SetCookies(new Uri("https://www.avito.ru"), response.Headers[HttpResponseHeader.SetCookie]);
            }
        }

        private void ReadHtmlContent(WebResponse r)
        {
            var response = r as HttpWebResponse;
            if (response != null)
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    ResponseHtml = reader.ReadToEnd();
                }
        }

        private void ReadResponseUri(WebResponse r)
        {
            var response = r as HttpWebResponse;
            if (response != null) ResponseUri = response.ResponseUri;
        }

        public class NavigationEventArgs : EventArgs
        {
            public Uri NavigateTo { get; set; }
        }
    }
}