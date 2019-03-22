using System;
using System.Windows;
using CefSharp;
using CefSharp.Wpf;
using Security;
using SellAvi.Properties;

namespace SellAvi.Browser
{
    public class CefSharpWrapper : ChromiumWebBrowser
    {
        public CefSharpWrapper()
        {
            IsBrowserInitializedChanged += CefSharpWrapper_IsBrowserInitializedChanged;
        }

        private void CefSharpWrapper_IsBrowserInitializedChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                Address = "https://www.avito.ru";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        //https://github.com/cefsharp/CefSharp.git

        public void SetUserAuthorizationCookies(string cookieString)
        {
            try
            {
                //TODO: куки лучше удалять только для домена avito.ru
                Cef.GetGlobalCookieManager().DeleteCookies("", "");
                foreach (var cookieElement in cookieString.Split(';'))
                {
                    var cName = cookieElement.Split('=')[0].Trim();
                    var cValue = cookieElement.Split('=')[1].Trim();
                    var k = new Cookie
                    {
                        Creation = DateTime.Today,
                        Expires = DateTime.Today.AddDays(30),
                        Name = cName,
                        Value = cValue
                    };
                    //var b = Cef.GetGlobalCookieManager().SetCookie("https://www.avito.ru", k);
                    var b1 = Cef.GetGlobalCookieManager().SetCookie("https://www.avito.ru/", k);
                    //var b2 = Cef.GetGlobalCookieManager().SetCookie("https://avito.ru", k);
                    //var b3 = Cef.GetGlobalCookieManager().SetCookie("http://www.avito.ru", k);
                    //var b4 = Cef.GetGlobalCookieManager().SetCookie("www.avito.ru", k);
                    //var b5 = Cef.GetGlobalCookieManager().SetCookie(".avito.ru", k);
                }
            }
            catch
            {
            }

            //m_innerBrowser.Navigate("https://www.avito.ru/profile/items/old");
        }
    }
}