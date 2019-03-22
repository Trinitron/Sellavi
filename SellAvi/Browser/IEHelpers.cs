using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using mshtml;
using FILETIME = System.Runtime.InteropServices.ComTypes.FILETIME;

namespace SellAvi.Browser
{
    /// <summary>
    ///     ThanksTo: http://stackoverflow.com/questions/25557474/get-wpf-webbrowser-html
    /// </summary>
    public class IEHelpers
    {
        /*
            HtmlDocumentHelper.FillField(webBrowser.Document, <id>, <value>);
            HtmlDocumentHelper.FillField(webBrowser.Document, <id>, <value>);
            HtmlDocumentHelper.ClickButton(webBrowser.Document, <id>);
            HtmlDocumentHelper.ExecuteScript(webBrowser.Document, "alert(1);");
         */
        public static void FillField(object doc, string id, string value)
        {
            var element = findElementByID(doc, id);
            element.setAttribute("value", value);
        }

        public static void ClickButton(object doc, string id)
        {
            var element = findElementByID(doc, id);
            element.click();
        }

        private static IHTMLElement findElementByID(object doc, string id)
        {
            IHTMLDocument2 thisDoc;
            if (!(doc is IHTMLDocument2))
                return null;
            thisDoc = (IHTMLDocument2) doc;

            var element = thisDoc.all.OfType<IHTMLElement>()
                .Where(n => n != null && n.id != null)
                .Where(e => e.id == id).First();
            return element;
        }

        private static void ExecuteScript(object doc, string js)
        {
            IHTMLDocument2 thisDoc;
            if (!(doc is IHTMLDocument2))
                return;
            thisDoc = (IHTMLDocument2) doc;
            thisDoc.parentWindow.execScript(js);
        }

        /// <summary>
        ///     Открыть браузер после публикации товара
        /// </summary>
        /// <param name="link"></param>
        public static void OpenWebBrowser(string link)
        {
            if (link != null)
                Process.Start(new ProcessStartInfo
                {
                    FileName = link,
                    UseShellExecute = true
                });
        }


        /// <summary>
        ///     <see cref="http://mdb-blog.blogspot.ru/2013/02/c-winforms-webbrowser-clear-all-cookies.html" />
        /// </summary>
        /// <returns></returns>
        public static bool SupressCookiePersist()
        {
            // 3 = INTERNET_SUPPRESS_COOKIE_PERSIST 
            // 81 = INTERNET_OPTION_SUPPRESS_BEHAVIOR
            return SetOption(81, 3);
        }

        public static bool EndBrowserSession()
        {
            // 42 = INTERNET_OPTION_END_BROWSER_SESSION 
            return SetOption(42, null);
        }

        private static bool SetOption(int settingCode, int? option)
        {
            var optionPtr = IntPtr.Zero;
            var size = 0;
            if (option.HasValue)
            {
                size = sizeof(int);
                optionPtr = Marshal.AllocCoTaskMem(size);
                Marshal.WriteInt32(optionPtr, option.Value);
            }

            var success = InternetSetOption(0, settingCode, optionPtr, size);

            if (optionPtr != IntPtr.Zero) Marshal.Release(optionPtr);
            return success;
        }

        [DllImport("wininet.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool InternetSetOption(
            int hInternet,
            int dwOption,
            IntPtr lpBuffer,
            int dwBufferLength
        );
    }


    #region Proxy_IE

    /// <summary>
    ///     http://blogs.msdn.com/b/jpsanders/archive/2011/04/26/how-to-set-the-proxy-for-the-webbrowser-control-in-net.aspx
    /// </summary>
    public static class WinInetInterop
    {
        /// <summary>
        ///     https://msdn.microsoft.com/en-us/library/windows/desktop/aa385328(v=vs.85).aspx
        /// </summary>
        public enum INTERNET_OPTION
        {
            // Sets or retrieves an INTERNET_PER_CONN_OPTION_LIST structure that specifies
            // a list of options for a particular connection.
            INTERNET_OPTION_PER_CONNECTION_OPTION = 75,

            // Notify the system that the registry settings have been changed so that
            // it verifies the settings on the next call to InternetConnect.
            INTERNET_OPTION_SETTINGS_CHANGED = 39,

            // Causes the proxy data to be reread from the registry for a handle.
            INTERNET_OPTION_REFRESH = 37,


            // Sets or retrieves a string value that contains the user name used to access the proxy.
            INTERNET_OPTION_PROXY_USERNAME = 43,

            // Sets or retrieves a string value that contains the password used to access the proxy.
            INTERNET_OPTION_PROXY_PASSWORD = 44
        }

        private const int INTERNET_OPEN_TYPE_DIRECT = 1; // direct to net
        private const int INTERNET_OPEN_TYPE_PRECONFIG = 0; // read registry
        public static string applicationName;

        /// <summary>
        ///     Sets an Internet option.
        /// </summary>
        [DllImport("wininet.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        public static extern bool /*BOOL*/ InternetSetOption(
            IntPtr hInternet, //_In_ HINTERNET hInternet,
            INTERNET_OPTION dwOption, //_In_ DWORD     dwOption,
            IntPtr lpBuffer, //_In_ LPVOID    lpBuffer
            int lpdwBufferLength); //_In_ DWORD     dwBufferLength

        [DllImport("wininet.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr /*HINTERNET*/ InternetOpen(
            string lpszAgent, //_In_ LPCTSTR lpszAgent,
            int dwAccessType, //_In_ DWORD   dwAccessType,
            string lpszProxyName, //_In_ LPCTSTR lpszProxyName,
            string lpszProxyBypass, //_In_ LPCTSTR lpszProxyBypass,
            int dwFlags); //_In_ DWORD   dwFlags

        [DllImport("wininet.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        /*в ручную*/
        public static extern bool HttpSendRequest(
            IntPtr hInternet, //_In_ HINTERNET hRequest,
            string lpszHeaders, //_In_ LPCTSTR   lpszHeaders,
            int dwHeadersLength, //_In_ DWORD     dwHeadersLength,
            IntPtr lpOptional, //_In_ LPVOID    lpOptional,
            int dwOptionalLength); //_In_ DWORD     dwOptionalLength

        [DllImport("wininet.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        /*в ручную*/
        public static extern IntPtr InternetConnect(
            IntPtr hInternet, //_In_ HINTERNET     hInternet,
            string lpszServerName, //_In_ LPCTSTR       lpszServerName,
            int nServerPort, //_In_ INTERNET_PORT nServerPort,???
            string lpszUsername, //_In_ LPCTSTR       lpszUsername,
            string lpszPassword, //_In_ LPCTSTR       lpszPassword,
            int dwService, //_In_ DWORD         dwService,         (INTERNET_SERVICE_HTTP = 3)
            int dwFlags, //_In_ DWORD         dwFlags,
            IntPtr dwContext); //_In_ DWORD_PTR     dwContext ???

        [DllImport("wininet.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        /*в ручную*/
        public static extern IntPtr HttpOpenRequest(
            IntPtr hConnect, //_In_ HINTERNET hConnect,
            string lpszVerb, //_In_ LPCTSTR   lpszVerb,
            string lpszObjectName, //_In_ LPCTSTR   lpszObjectName,
            string lpszVersion, //_In_ LPCTSTR   lpszVersion,
            string lpszReferer, //_In_ LPCTSTR   lpszReferer,
            string lplpszAcceptTypes, //_In_ LPCTSTR   *lplpszAcceptTypes,
            int dwFlags, //_In_ DWORD     dwFlags,
            IntPtr dwContext); //_In_ DWORD_PTR dwContext


        [DllImport("wininet.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool InternetCloseHandle(IntPtr hInternet);


        /// <summary>
        ///     Queries an Internet option on the specified handle. The Handle will be always 0.
        /// </summary>
        [DllImport("wininet.dll", CharSet = CharSet.Ansi, SetLastError = true,
            EntryPoint = "InternetQueryOption")]
        private static extern bool InternetQueryOptionList(
            IntPtr Handle,
            INTERNET_OPTION OptionFlag,
            ref INTERNET_PER_CONN_OPTION_LIST OptionList,
            ref int size);


        /// <summary>
        ///     Set the proxy server for LAN connection.
        /// </summary>
        public static bool SetConnectionProxy(string proxyServer, string proxyLogin = null, string proxyPass = null)
        {
            var hInternet = InternetOpen(applicationName, INTERNET_OPEN_TYPE_DIRECT, null, null, 0);

            //// Create 3 options.
            //INTERNET_PER_CONN_OPTION[] Options = new INTERNET_PER_CONN_OPTION[3];

            // Create 2 options.
            var Options = new INTERNET_PER_CONN_OPTION[2];

            // Set PROXY flags.
            Options[0] = new INTERNET_PER_CONN_OPTION();
            Options[0].dwOption = (int) INTERNET_PER_CONN_OptionEnum.INTERNET_PER_CONN_FLAGS;
            Options[0].Value.dwValue = (int) INTERNET_OPTION_PER_CONN_FLAGS.PROXY_TYPE_PROXY;

            // Set proxy name.
            Options[1] = new INTERNET_PER_CONN_OPTION();
            Options[1].dwOption =
                (int) INTERNET_PER_CONN_OptionEnum.INTERNET_PER_CONN_PROXY_SERVER;
            Options[1].Value.pszValue = Marshal.StringToHGlobalAnsi(proxyServer);
            /*
            if (proxyLogin!=null && proxyPass!=null)
            {
                // Set proxy name.
                Options[2] = new INTERNET_PER_CONN_OPTION();
                Options[2].dwOption =
                    (int)INTERNET_PER_CONN_OptionEnum.INTERNET_PER_CONN_PROXY_SERVER;
                Options[2].Value.pszValue = Marshal.StringToHGlobalAnsi(proxyServer);

                // Set proxy name.
                Options[3] = new INTERNET_PER_CONN_OPTION();
                Options[3].dwOption =
                    (int)INTERNET_PER_CONN_OptionEnum.INTERNET_PER_CONN_PROXY_SERVER;
                Options[3].Value.pszValue = Marshal.StringToHGlobalAnsi(proxyServer);
            }*/


            //// Set proxy bypass.
            //Options[2] = new INTERNET_PER_CONN_OPTION();
            //Options[2].dwOption =
            //    (int)INTERNET_PER_CONN_OptionEnum.INTERNET_PER_CONN_PROXY_BYPASS;
            //Options[2].Value.pszValue = Marshal.StringToHGlobalAnsi(“local”);

            //// Allocate a block of memory of the options.
            //System.IntPtr buffer = Marshal.AllocCoTaskMem(Marshal.SizeOf(Options[0])
            //    + Marshal.SizeOf(Options[1]) + Marshal.SizeOf(Options[2]));

            // Allocate a block of memory of the options.
            var buffer = Marshal.AllocCoTaskMem(Marshal.SizeOf(Options[0])
                                                + Marshal.SizeOf(Options[1]));

            var current = buffer;

            // Marshal data from a managed object to an unmanaged block of memory.
            for (var i = 0; i < Options.Length; i++)
            {
                Marshal.StructureToPtr(Options[i], current, false);
                current = (IntPtr) ((int) current + Marshal.SizeOf(Options[i]));
            }

            // Initialize a INTERNET_PER_CONN_OPTION_LIST instance.
            var option_list = new INTERNET_PER_CONN_OPTION_LIST();

            // Point to the allocated memory.
            option_list.pOptions = buffer;

            // Return the unmanaged size of an object in bytes.
            option_list.Size = Marshal.SizeOf(option_list);

            // IntPtr.Zero means LAN connection.
            option_list.Connection = IntPtr.Zero;

            option_list.OptionCount = Options.Length;
            option_list.OptionError = 0;
            var size = Marshal.SizeOf(option_list);

            // Allocate memory for the INTERNET_PER_CONN_OPTION_LIST instance.
            var intptrStruct = Marshal.AllocCoTaskMem(size);

            // Marshal data from a managed object to an unmanaged block of memory.
            Marshal.StructureToPtr(option_list, intptrStruct, true);

            // Set internet settings.
            var bReturn = InternetSetOption(hInternet,
                INTERNET_OPTION.INTERNET_OPTION_PER_CONNECTION_OPTION, intptrStruct, size);

            //установка login / pass для proxy
            /*
            string un = "login";
            string pw = "pass";
            IntPtr username = Marshal.StringToHGlobalAnsi(un);
            IntPtr password = Marshal.StringToHGlobalAnsi(pw);

            IntPtr hConnectHandle = WinInetInterop.InternetConnect(hInternet, "185.125.219.169", 0, un, pw, 3, 0, IntPtr.Zero);
            var errorG1 = Marshal.GetLastWin32Error();

            WinInetInterop.InternetSetOption(hConnectHandle, WinInetInterop.INTERNET_OPTION.INTERNET_OPTION_PROXY_USERNAME, username, Marshal.SizeOf(username));
            WinInetInterop.InternetSetOption(hConnectHandle, WinInetInterop.INTERNET_OPTION.INTERNET_OPTION_PROXY_PASSWORD, password, Marshal.SizeOf(password));
            var errorG = Marshal.GetLastWin32Error();

            IntPtr hResourceHandle = WinInetInterop.HttpOpenRequest(hConnectHandle, "GET", "/", null, null, null, 0x00400000, IntPtr.Zero);
            var errorG2 = Marshal.GetLastWin32Error();
            var r = WinInetInterop.HttpSendRequest(hResourceHandle, "", 0, IntPtr.Zero, 0);
            var errorG3 = Marshal.GetLastWin32Error();
            */

            // Free the allocated memory.
            Marshal.FreeCoTaskMem(buffer);
            Marshal.FreeCoTaskMem(intptrStruct);
            InternetCloseHandle(hInternet);

            // Throw an exception if this operation failed.
            if (!bReturn) throw new ApplicationException("Set Internet Option Failed!");

            return bReturn;
        }


        /// <summary>
        ///     Backup the current options for LAN connection.
        ///     Make sure free the memory after restoration.
        /// </summary>
        private static INTERNET_PER_CONN_OPTION_LIST GetSystemProxy()
        {
            // Query following options. 
            var Options = new INTERNET_PER_CONN_OPTION[3];

            Options[0] = new INTERNET_PER_CONN_OPTION();
            Options[0].dwOption = (int) INTERNET_PER_CONN_OptionEnum.INTERNET_PER_CONN_FLAGS;
            Options[1] = new INTERNET_PER_CONN_OPTION();
            Options[1].dwOption = (int) INTERNET_PER_CONN_OptionEnum.INTERNET_PER_CONN_PROXY_SERVER;
            Options[2] = new INTERNET_PER_CONN_OPTION();
            Options[2].dwOption = (int) INTERNET_PER_CONN_OptionEnum.INTERNET_PER_CONN_PROXY_BYPASS;

            // Allocate a block of memory of the options.
            var buffer = Marshal.AllocCoTaskMem(Marshal.SizeOf(Options[0])
                                                + Marshal.SizeOf(Options[1]) + Marshal.SizeOf(Options[2]));

            var current = buffer;

            // Marshal data from a managed object to an unmanaged block of memory.
            for (var i = 0; i < Options.Length; i++)
            {
                Marshal.StructureToPtr(Options[i], current, false);
                current = (IntPtr) ((int) current + Marshal.SizeOf(Options[i]));
            }

            // Initialize a INTERNET_PER_CONN_OPTION_LIST instance.
            var Request = new INTERNET_PER_CONN_OPTION_LIST();

            // Point to the allocated memory.
            Request.pOptions = buffer;

            Request.Size = Marshal.SizeOf(Request);

            // IntPtr.Zero means LAN connection.
            Request.Connection = IntPtr.Zero;

            Request.OptionCount = Options.Length;
            Request.OptionError = 0;
            var size = Marshal.SizeOf(Request);

            //

            // Query internet options. 
            var result = InternetQueryOptionList(IntPtr.Zero,
                INTERNET_OPTION.INTERNET_OPTION_PER_CONNECTION_OPTION,
                ref Request, ref size);
            if (!result) throw new ApplicationException("Set Internet Option Failed!");

            return Request;
        }

        /// <summary>
        ///     Restore the options for LAN connection.
        /// </summary>
        public static bool RestoreSystemProxy()
        {
            var hInternet = InternetOpen(applicationName, INTERNET_OPEN_TYPE_DIRECT, null, null, 0);

            var request = GetSystemProxy();
            var size = Marshal.SizeOf(request);

            // Allocate memory. 
            var intptrStruct = Marshal.AllocCoTaskMem(size);

            // Convert structure to IntPtr 
            Marshal.StructureToPtr(request, intptrStruct, true);

            // Set internet options.
            var bReturn = InternetSetOption(hInternet,
                INTERNET_OPTION.INTERNET_OPTION_PER_CONNECTION_OPTION,
                intptrStruct, size);

            // Free the allocated memory.
            Marshal.FreeCoTaskMem(request.pOptions);
            Marshal.FreeCoTaskMem(intptrStruct);

            if (!bReturn) throw new ApplicationException("Set Internet Option Failed!");


            // Notify the system that the registry settings have been changed and cause
            // the proxy data to be reread from the registry for a handle.
            InternetSetOption(hInternet, INTERNET_OPTION.INTERNET_OPTION_SETTINGS_CHANGED,
                IntPtr.Zero, 0);
            InternetSetOption(hInternet, INTERNET_OPTION.INTERNET_OPTION_REFRESH,
                IntPtr.Zero, 0);

            InternetCloseHandle(hInternet);

            return bReturn;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        private struct INTERNET_PER_CONN_OPTION_LIST
        {
            public int Size;

            // The connection to be set. NULL means LAN.
            public IntPtr Connection;

            public int OptionCount;
            public int OptionError;

            // List of INTERNET_PER_CONN_OPTIONs.
            public IntPtr pOptions;
        }

        private enum INTERNET_PER_CONN_OptionEnum
        {
            INTERNET_PER_CONN_FLAGS = 1,
            INTERNET_PER_CONN_PROXY_SERVER = 2,
            INTERNET_PER_CONN_PROXY_BYPASS = 3,
            INTERNET_PER_CONN_AUTOCONFIG_URL = 4,
            INTERNET_PER_CONN_AUTODISCOVERY_FLAGS = 5,
            INTERNET_PER_CONN_AUTOCONFIG_SECONDARY_URL = 6,
            INTERNET_PER_CONN_AUTOCONFIG_RELOAD_DELAY_MINS = 7,
            INTERNET_PER_CONN_AUTOCONFIG_LAST_DETECT_TIME = 8,
            INTERNET_PER_CONN_AUTOCONFIG_LAST_DETECT_URL = 9,
            INTERNET_PER_CONN_FLAGS_UI = 10
        }

        /// <summary>
        ///     Constants used in INTERNET_PER_CONN_OPTON struct.
        /// </summary>
        private enum INTERNET_OPTION_PER_CONN_FLAGS
        {
            PROXY_TYPE_DIRECT = 0x00000001, // direct to net
            PROXY_TYPE_PROXY = 0x00000002, // via named proxy
            PROXY_TYPE_AUTO_PROXY_URL = 0x00000004, // autoproxy URL
            PROXY_TYPE_AUTO_DETECT = 0x00000008 // use autoproxy detection
        }

        /// <summary>
        ///     Used in INTERNET_PER_CONN_OPTION.
        ///     When create a instance of OptionUnion, only one filed will be used.
        ///     The StructLayout and FieldOffset attributes could help to decrease the struct size.
        /// </summary>
        [StructLayout(LayoutKind.Explicit)]
        private struct INTERNET_PER_CONN_OPTION_OptionUnion
        {
            // A value in INTERNET_OPTION_PER_CONN_FLAGS.
            [FieldOffset(0)] public int dwValue;
            [FieldOffset(0)] public IntPtr pszValue;
            [FieldOffset(0)] public readonly FILETIME ftValue;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct INTERNET_PER_CONN_OPTION
        {
            // A value in INTERNET_PER_CONN_OptionEnum.
            public int dwOption;
            public INTERNET_PER_CONN_OPTION_OptionUnion Value;
        }
    }

    #endregion
}