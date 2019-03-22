using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using HtmlAgilityPack;
using mshtml;
using SellAvi.Classes;
using SHDocVw;
using WebBrowser = System.Windows.Controls.WebBrowser;

namespace SellAvi.Browser
{
    /// http://www.codeproject.com/Articles/555302/A-better-WPF-Browser-Control-IE-Wrapper
    /// <summary>
    ///     Class wraps a Browser (which itself is a bad designed WPF control) and presents itself as
    ///     a better designed WPF control. For example provides a bindable source property or commands.
    /// </summary>
    public class WpfWebBrowserWrapper : ContentControl
    {
        /*
        public static void DisplayHTML(string html)
        {
            // Open up a new Internet Explorer. 

            //var ie = CreateObject("InternetExplorer.Application");

            //SHDocVw.InternetExplorer ie =
            //var ie = SHDocVw.InternetExplorerClass();

            SHDocVw.InternetExplorer ie = new SHDocVw.InternetExplorerClass(){};

            SHDocVw.IWebBrowserApp wb = (SHDocVw.IWebBrowserApp)ie;
            wb.Visible = true;

            object noValue = System.Reflection.Missing.Value;
            wb.Navigate("about:blank", ref noValue, ref noValue, ref noValue, ref noValue);

            // Get access to its document.

            mshtml.IHTMLDocument2 htmlDoc2 = ie.Document as mshtml.IHTMLDocument2;

            // Update the document.

            htmlDoc2.writeln(html);
            htmlDoc2.close();
        }*/

        private const int URLMON_OPTION_USERAGENT = 0x10000001;

        private static readonly Guid SID_SWebBrowserApp = new Guid("0002DF05-0000-0000-C000-000000000046");


        public static readonly DependencyProperty BindableSourceProperty =
            DependencyProperty.Register("BindableSource", typeof(string), typeof(WpfWebBrowserWrapper),
                new UIPropertyMetadata("about:blank", BindableSourcePropertyChanged_));

        public static readonly DependencyProperty CurrentUrlProperty =
            DependencyProperty.Register("CurrentUrl", typeof(string), typeof(WpfWebBrowserWrapper),
                new UIPropertyMetadata(string.Empty));

        public static readonly DependencyProperty ZoomProperty =
            DependencyProperty.Register("Zoom", typeof(int), typeof(WpfWebBrowserWrapper),
                new UIPropertyMetadata(100, ZoomPropertyChanged_));

        public static readonly DependencyProperty IsNavigatingProperty =
            DependencyProperty.Register("IsNavigating", typeof(bool), typeof(WpfWebBrowserWrapper),
                new UIPropertyMetadata(false));

        public static readonly DependencyProperty LastErrorProperty =
            DependencyProperty.Register("LastError", typeof(string), typeof(WpfWebBrowserWrapper),
                new UIPropertyMetadata(string.Empty));

        public readonly WebBrowser m_innerBrowser;

        public WpfWebBrowserWrapper()
        {
            m_innerBrowser = new WebBrowser
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };

            Content = m_innerBrowser;
            m_innerBrowser.Navigated += m_innerBrowser_Navigated_;
            m_innerBrowser.Navigating += m_innerBrowser_Navigating_;
            m_innerBrowser.LoadCompleted += m_innerBrowser_LoadCompleted_;
            m_innerBrowser.Loaded += m_innerBrowser_Loaded_;
            m_innerBrowser.SizeChanged += m_innerBrowser_SizeChanged_;

            CommandBindings.Add(new CommandBinding(NavigationCommands.BrowseBack, BrowseBackExecuted_,
                CanBrowseBack_));
            CommandBindings.Add(new CommandBinding(NavigationCommands.BrowseForward, BrowseForwardExecuted_,
                CanBrowseForward_));
            CommandBindings.Add(new CommandBinding(NavigationCommands.Refresh, BrowseRefreshExecuted_));
            CommandBindings.Add(new CommandBinding(NavigationCommands.BrowseStop, BrowseStopExecuted_));
            CommandBindings.Add(new CommandBinding(NavigationCommands.GoToPage, BrowseGoToPageExecuted_));
        }

        // bindable source property to make the browser navigate to the given url. Assign this from your url bar.
        public string BindableSource
        {
            get => (string) GetValue(BindableSourceProperty);
            set => SetValue(BindableSourceProperty, value);
        }

        // bindable property depicting the current url. Use this to read out and present in your url bar.
        public string CurrentUrl
        {
            get => (string) GetValue(CurrentUrlProperty);
            set => SetValue(CurrentUrlProperty, value);
        }

        // percentage value : 20..800 change to let control zoom in out
        public int Zoom
        {
            get => (int) GetValue(ZoomProperty);
            set => SetValue(ZoomProperty, value);
        }

        // percentage value : 20..800 change to let control zoom in out
        public string LastError => (string) GetValue(LastErrorProperty);

        // Indicates to the outside that the browser is currently working on a page aka loading. Read only
        public int IsNavigating
        {
            get => (int) GetValue(IsNavigatingProperty);
            set => SetValue(IsNavigatingProperty, value);
        }

        // gets the browser control's underlying activeXcontrol. Ready only from within Loaded-event but before loaded Document!
        // do not use prior loaded event.
        public InternetExplorer ActiveXControl
        {
            get
            {
                // this is a brilliant way to access the WebBrowserObject prior to displaying the actual document (eg. Document property)
                var fiComWebBrowser =
                    typeof(WebBrowser).GetField("_axIWebBrowser2", BindingFlags.Instance | BindingFlags.NonPublic);
                if (fiComWebBrowser == null) return null;
                var objComWebBrowser = fiComWebBrowser.GetValue(m_innerBrowser);
                if (objComWebBrowser == null) return null;
                return objComWebBrowser as InternetExplorer;
            }
        }

        public WebClientEx AdditionalWebClient { get; set; }


        private void m_innerBrowser_SizeChanged_(object sender, SizeChangedEventArgs e)
        {
            ApplyZoom(this);
        }

        private void m_innerBrowser_Loaded_(object sender, EventArgs e)
        {
            // make browser control not silent: allow HTTP-Auth-dialogs. Requery command availability
            var ie = ActiveXControl;
            ie.Silent = true;
            CommandManager.InvalidateRequerySuggested();
        }


        // called when the loading of a web page is done
        private void m_innerBrowser_LoadCompleted_(object sender, NavigationEventArgs e)
        {
            ApplyZoom(this); // apply later and not only at changed event, since only works if browser is rendered.
            SetCurrentValue(IsNavigatingProperty, false);
            CommandManager.InvalidateRequerySuggested();
        }

        // called when the browser started to load and retrieve data.
        private void m_innerBrowser_Navigating_(object sender, NavigatingCancelEventArgs e)
        {
            //Синхронизация Cookie между WebClient и ActiveX контролом
            SetCurrentValue(IsNavigatingProperty, true);
            SynchronizeCookies();
        }

        // re query the commands once done navigating.
        private void m_innerBrowser_Navigated_(object sender, NavigationEventArgs e)
        {
            RegisterWindowErrorHanlder_();

            CommandManager.InvalidateRequerySuggested();
            // publish the just navigated (maybe redirected) url
            if (e.Uri != null) SetCurrentValue(CurrentUrlProperty, e.Uri.ToString());
        }

        private void BrowseForwardExecuted_(object sender, ExecutedRoutedEventArgs e)
        {
            m_innerBrowser.GoForward();
        }

        private void CanBrowseForward_(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = m_innerBrowser.IsLoaded && m_innerBrowser.CanGoForward;
        }

        private void BrowseBackExecuted_(object sender, ExecutedRoutedEventArgs e)
        {
            m_innerBrowser.GoBack();
        }

        private void CanBrowseBack_(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = m_innerBrowser.IsLoaded && m_innerBrowser.CanGoBack;
        }

        private void BrowseRefreshExecuted_(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                //ActiveXControl.Refresh2(false);
                m_innerBrowser.Refresh(true);
                //RegisterWindowErrorHanlder_();
            }
            catch (COMException comException)
            {
            }

            // No setting of navigating=true, since the reset event never triggers on Refresh!
            //SetCurrentValue(IsNavigatingProperty, true);
        }

        private void BrowseStopExecuted_(object sender, ExecutedRoutedEventArgs executedRoutedEventArgs)
        {
            var control = ActiveXControl;
            if (control != null)
                control.Stop();
        }

        /// <summary>
        ///     Navigates to the page specified in the parameter.
        /// </summary>
        private void BrowseGoToPageExecuted_(object sender, ExecutedRoutedEventArgs executedRoutedEventArgs)
        {
            Uri uriResult;
            var isValidUrl =
                Uri.TryCreate((string) executedRoutedEventArgs.Parameter, UriKind.Absolute, out uriResult) &&
                uriResult.Scheme == Uri.UriSchemeHttp;

            if (isValidUrl)
                m_innerBrowser.Navigate(uriResult.ToString());
        }


        private static void BindableSourcePropertyChanged_(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var wrapper = (WpfWebBrowserWrapper) o;
            var browser = wrapper.m_innerBrowser;
            if (browser != null)
            {
                var uri = e.NewValue as string;
                if (!string.IsNullOrWhiteSpace(uri) && Uri.IsWellFormedUriString(uri, UriKind.Absolute))
                {
                    Uri uriObj;
                    try
                    {
                        uriObj = new Uri(uri);
                        browser.Source = uriObj;
                    }
                    catch (UriFormatException)
                    {
                        // just don't crash because of a malformed url
                    }
                }
                else
                {
                    browser.Source = null;
                }
            }
        }

        // register script errors handler on DOM - document.window
        private void RegisterWindowErrorHanlder_()
        {
            object parwin = ((dynamic) m_innerBrowser.Document).parentWindow;
            //TODO: comment
            //var cookie = new System.Windows.Forms.AxHost.ConnectionPointCookie(parwin, new HtmlWindowEvents2Impl(this), typeof(IIntHTMLWindowEvents2));

            // MemoryLEAK? No: cookie has a Finalize() to Disconnect istelf. We'll rely on that. If disconnected too early, 
            // though (eg. in LoadCompleted-event) scripts continue to run and can cause error messages to appear again.
            // --> forget cookie and be happy.
        }

        public static void ZoomPropertyChanged_(DependencyObject src, DependencyPropertyChangedEventArgs e)
        {
            ApplyZoom(src);
        }

        private static void ApplyZoom(DependencyObject src)
        {
            const int k_minZoom = 10;
            const int k_maxZoom = 1000;
            const float k_zoomInReference = 800.0f;


            var browser = src as WpfWebBrowserWrapper;
            if (browser != null && browser.IsLoaded)
            {
                var webbr = browser.m_innerBrowser;
                var zoomPercent = browser.Zoom;

                // Determine auto-zoom
                if (browser.Zoom == -1)
                {
                    if (browser.ActualWidth < k_zoomInReference)
                        zoomPercent = (int) (browser.ActualWidth / k_zoomInReference * 100);
                    else
                        zoomPercent = 100;
                }

                // rescue too high or too low values
                zoomPercent = Math.Min(zoomPercent, k_maxZoom);
                zoomPercent = Math.Max(zoomPercent, k_minZoom);

                // grab a handle to the underlying ActiveX object
                IServiceProvider serviceProvider = null;
                if (webbr.Document != null) serviceProvider = (IServiceProvider) webbr.Document;

                if (serviceProvider == null)
                    return;

                var serviceGuid = SID_SWebBrowserApp;
                var iid = typeof(IWebBrowser2).GUID;
                var browserInst = (IWebBrowser2) serviceProvider.QueryService(ref serviceGuid, ref iid);

                try
                {
                    object zoomPercObj = zoomPercent;
                    // send the zoom command to the ActiveX object
                    browserInst.ExecWB(OLECMDID.OLECMDID_OPTICAL_ZOOM,
                        OLECMDEXECOPT.OLECMDEXECOPT_DONTPROMPTUSER,
                        ref zoomPercObj,
                        IntPtr.Zero);
                }
                catch (Exception)
                {
                    // ignore this dynamic call if it fails.
                }
            }
        }

        private void SynchronizeCookies()
        {
            /*if (ActiveXControl != null)
            {
                foreach (var cookieElement in AdditionalWebClient.CookieContainer.GetCookieHeader(new Uri("https://wwww.avito.ru")).Split(';'))
                    {
                        //Client.CookieContainer.SetCookies(new Uri("https://www.avito.ru"), cookieElement.Trim());
                        InternetSetCookie("https://www.avito.ru/profile", null, cookieElement);
                    }
            }*/
        }

        public string GetBrowserCookies(string url)
        {
            var size = 0;
            InternetGetCookie(url, null, null, ref size);

            // create buffer of correct size
            var lpszCookieData = new StringBuilder(size);
            InternetGetCookie(url, null, lpszCookieData, ref size);

            return lpszCookieData.ToString();
        }

        public void SetUserAuthorizationCookies(string cookieString)
        {
            try
            {
                //BUG: Cookie не удаляются! (возможно проблема с авторизацией) 
                //Сброс устаревших cookie (wininet.dll)
                IEHelpers.SupressCookiePersist();
                IEHelpers.EndBrowserSession();

                dynamic document = m_innerBrowser.Document;
                document.ExecCommand("ClearAuthenticationCache", false, null);

                //Установка новых
                foreach (var cookieElement in cookieString.Split(';'))
                {
                    var isSuccess = InternetSetCookie("https://www.avito.ru", null, cookieElement.Trim());
                }
            }
            catch
            {
            }

            //m_innerBrowser.Navigate("https://www.avito.ru/profile/items/old");
        }


        private static void OnHtmlChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var webBrowser = dependencyObject as WpfWebBrowserWrapper;
            var webClient = e.NewValue as WebClientEx;

            //SetHtml(webBrowser,e.NewValue as string);
            if (webBrowser != null)
                if (webBrowser.ActiveXControl != null)
                    if ( /*GetCookie(webBrowser) != null &&*/ 1 == 0)
                        webBrowser.ActiveXControl.Navigate("https://www.avito.ru/profile", null, null, null, null);
        }

        public void SetBrowserDefaultPage(string license)
        {
            try
            {
                using (var sr = new StreamReader("index.html"))
                {
                    DateTime licenseDate;
                    var indexContent = sr.ReadToEnd();
                    var htmlPage = new HtmlDocument();
                    var daysLeft = "0";

                    htmlPage.LoadHtml(indexContent);
                    var node = htmlPage.DocumentNode.SelectSingleNode("//span[@class='label label-danger']");

                    if (license != string.Empty &&
                        DateTime.TryParse(license.Substring(license.Length - 10, 10), out licenseDate))
                        daysLeft = (licenseDate - DateTime.Now).TotalDays.ToString("0");

                    node.InnerHtml = daysLeft;
                    m_innerBrowser.NavigateToString(htmlPage.DocumentNode.OuterHtml);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }


        //http://www.codeproject.com/Articles/14161/Automation-of-Internet-Explorer-Using-shdocvw-dll
        //https://patricjsson.wordpress.com/2004/12/29/injecting-html-code-directly-into-an-internet-explorer/
        //http://www.codeproject.com/Articles/28094/Working-With-Microsoft-mshtml-dll-and-SHDocVw-dll
        private static void SetBaseUrl(HTMLDocument htmlDoc)
        {
            //htmlDoc.baseUrl = "https://m2.avito.ru";
            //htmlDoc.url = "https://m2.avito.ru";
            //((mshtml.HTMLDocumentClass)(htmlDoc)).url

            var link = htmlDoc.createElement("base");
            link.setAttribute("href", "https://www.avito.ru");
            var ni = htmlDoc.getElementsByTagName("head").item(0);
            if (ni != null)
            {
                var s = ni.insertbefore(link, ni.firstChild);
            }

            //s = ni.insertBefore(link, ni.firstChild)
            //htmlDoc.getElementsByTagName("head").item(0);
            //var s = 
            //mshtml.HTMLDocument htmlDoc = ActiveXControl.Document;
            //htmlDoc.
        }

        public void ChangeUserAgent(string Agent)
        {
            UrlMkSetSessionOption(URLMON_OPTION_USERAGENT, Agent, Agent.Length, 0);
        }

        public string GetHtmlContent()
        {
            dynamic doc = m_innerBrowser.Document;
            return doc.documentElement.InnerHtml;
        }

        public string GetUserAgent()
        {
            var userAgent = new StringBuilder(255);
            var returnLength = 0;
            UrlMkGetSessionOption(URLMON_OPTION_USERAGENT, userAgent, userAgent.Capacity, ref returnLength, 0);
            return userAgent.ToString();
        }

        [DllImport("wininet.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool InternetSetCookie(string lpszUrlName, string lpszCookieName, string lpszCookieData);

        [DllImport("wininet.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool InternetGetCookie(string lpszUrlName, string lpszCookieName,
            StringBuilder lpszCookieData,
            ref int lpdwSize);


        [DllImport("urlmon.dll", CharSet = CharSet.Ansi)]
        private static extern bool UrlMkSetSessionOption(int dwOption, string pBuffer, int dwBufferLength,
            int dwReserved);

        [DllImport("urlmon.dll", CharSet = CharSet.Ansi)]
        private static extern int UrlMkGetSessionOption(int dwOption, StringBuilder pBuffer, int dwBufferLength,
            ref int pdwBufferLength, int dwReserved);

        // needed to implement the Event for script errors
        [Guid("3050f625-98b5-11cf-bb82-00aa00bdce0b")]
        [InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
        [TypeLibType(TypeLibTypeFlags.FHidden)]
        [ComImport]
        private interface IIntHTMLWindowEvents2
        {
            [DispId(1002)]
            bool onerror(string description, string url, int line);
        }

        // needed to implement the Event for script errors
        private class HtmlWindowEvents2Impl : IIntHTMLWindowEvents2
        {
            private readonly WpfWebBrowserWrapper m_control;

            public HtmlWindowEvents2Impl(WpfWebBrowserWrapper control)
            {
                m_control = control;
            }

            // implementation of the onerror Javascript error. Return true to indicate a "Handled" state.
            public bool onerror(string description, string urlString, int line)
            {
                m_control.SetCurrentValue(LastErrorProperty, description + "@" + urlString + ": " + line);
                // Handled:
                return true;
            }
        }

        // Needed to expose the WebBrowser's underlying ActiveX control for zoom functionality
        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("6d5140c1-7436-11ce-8034-00aa006009fa")]
        internal interface IServiceProvider
        {
            [return: MarshalAs(UnmanagedType.IUnknown)]
            object QueryService(ref Guid guidService, ref Guid riid);
        }
    }
}