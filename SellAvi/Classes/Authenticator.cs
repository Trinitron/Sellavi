using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows;
using HtmlAgilityPack;
using NLog;
using SellAvi.Classes;
using SellAvi.Classes.Exceptions;
using SellAvi.Model.DataBaseModels;

namespace SellAvi
{
    internal class Authenticator
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly Uri LoginUrl = new Uri("https://m.avito.ru/profile/login");

        private static readonly NameValueCollection _userCredentials = new NameValueCollection
        {
            {"login", ""},
            {"password", ""},
            {"next", "/profile"}
        };

        private readonly AvitoUser _user;

        public Authenticator(AvitoUser usr)
        {
            if (usr.UserName != null)
                usr.UserName = usr.UserName.Trim();
            if (usr.UserPassword != null)
                usr.UserPassword = usr.UserPassword.Trim();

            _user = usr;

            _userCredentials.Set("login", usr.UserName);
            _userCredentials.Set("password", usr.UserPassword);
        }

        public WebClientEx Client { get; set; }

        public string ResolvedCaptchaText { get; set; }

        public string CaptchaImagePath { get; set; }

        public bool Authenticate(bool cookieAuth = true)
        {
            try
            {
                Client.DefaultHeaders[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                Client.DefaultHeaders[HttpRequestHeader.Referer] = "https://avito.ru";

                if (cookieAuth && _user.UserCookie != null)
                {
                    Logger.Trace("Использование Cookie для входа");
                    Client.FillCookieContainer(_user.UserCookie, "https://www.avito.ru");
                    //OR
                    //Client.Headers[HttpRequestHeader.Cookie] = _user.UserCookie;
                }
                else
                {
                    Logger.Trace("Авторизация через POST запрос");
                    Client.UploadValues(LoginUrl, _userCredentials);

                    //Авторизация невозможна, необходимо восстановить пароль
                    if (Client.ResponseHtml.Contains("Авторизация невозможна"))
                        throw new Exception("Для входа в Avito необходимо восстановить пароль!");

                    //Если не произошел 302 редирект на страницу : /profile то: либо надо ввести каптчу, либо неверно введен пароль
                    if (Client.ResponseUri == LoginUrl &&
                        (CaptchaImagePath = TryParseCaptchaFromHtml(Client.ResponseHtml)) != null)
                        if (ResolvedCaptchaText != null)
                        {
                            _userCredentials.Set("captcha", ResolvedCaptchaText);
                            ResolvedCaptchaText = null;
                            Authenticate(false);
                        }
                }

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
                MessageBox.Show(ex.Message, "Ошибка авторизации на сайте", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public bool CheckAuthorization()
        {
            Logger.Trace("Проверка доступности профиля пользователя");
            var checkUrl = new Uri("https://www.avito.ru/profile");
            var authorizationSucceed = false;
            try
            {
                Client.DownloadStringWithNotification(checkUrl);

                authorizationSucceed = Client.ResponseUri == checkUrl;
                if (authorizationSucceed == false)
                    throw new Exception("Avito отказало вам в доступе, проверьте логин и пароль и попробуйте заново!");

                authorizationSucceed = true;
            }
            catch (LicenceException ex)
            {
                Logger.Error(ex);
                MessageBox.Show(ex.Message, "Ограничение лицензии", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                MessageBox.Show(ex.Message, "Ошибка входа", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return authorizationSucceed;
        }

        /// <summary>
        ///     TODO: Аналогичный метод в TaskProductUploader
        /// </summary>
        /// <param name="htmlContent">Содержимое веб-страницы, предполагаемо содержащие каптчу</param>
        /// <param name="cip">Captcha Full Path (local)</param>
        /// <returns></returns>
        private string TryParseCaptchaFromHtml(string htmlContent)
        {
            Directory.CreateDirectory("captcha");
            var captchaRegex = new Regex(@"\/captcha\?\d+");
            var captchaUrl = captchaRegex.Match(htmlContent).Value;
            if (captchaUrl == string.Empty)
            {
                Logger.Warn("Невозможно получить ссылку на изображение защитного кода с предоставленной страницы");
                return null;
            }

            var captachaFileName = "captcha/" + Path.GetRandomFileName() + ".jpg";
            Client.DownloadFile("https://www.avito.ru" + captchaUrl, captachaFileName);
            return Path.GetFullPath(captachaFileName);
        }

        public static string GetLicenseQuery(AvitoUser usr, int action = 0)
        {
            _userCredentials.Set("login", usr.UserName);
            _userCredentials.Set("password", usr.UserPassword);
            return "http://sellavi.ru/";
        }

        /// <summary>
        ///     Заполнение информации о пользователе из настроек профиля avito
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public AvitoUser UpdateUserInformation(AvitoUser user)
        {
            Logger.Trace("Получение контактных данных пользователя");
            var htmlPage = new HtmlDocument();
            var phones = new List<string>();
            var profileHtml = Client.DownloadString("https://www.avito.ru/profile/settings");

            if (Client.ResponseUri == new Uri("https://www.avito.ru/profile/settings"))
                try
                {
                    htmlPage.LoadHtml(profileHtml);

                    var id = htmlPage.DocumentNode.SelectSingleNode(
                        "//span[contains(@class, 'profile-settings-header-descr')]");
                    user.ProfileId = int.Parse(Regex.Match(id.InnerText, @"\d+").Value);

                    user.CompanyName =
                        HtmlEntity.DeEntitize(htmlPage.GetElementbyId("fld_name").GetAttributeValue("value", ""));
                    var cm = htmlPage.GetElementbyId("fld_manager");
                    user.CompanyManager =
                        cm != null ? HtmlEntity.DeEntitize(cm.GetAttributeValue("value", "")) : null;

                    //регион может отсутствовать в профиле
                    var regionCode =
                        htmlPage.DocumentNode.SelectSingleNode(
                                "//select[@name = 'locationId']/option[@selected='selected']")
                            .GetAttributeValue("value", null);
                    //user.TRegion_Id = String.IsNullOrEmpty(regionCode) ? null : (int?)int.Parse(regionCode);

                    user.CompanyEmail =
                        htmlPage.DocumentNode
                            .SelectSingleNode("//span[contains(@class, 'profile-settings-header__name')]")
                            .InnerText.Trim();
                    var phoneNodes =
                        htmlPage.DocumentNode.SelectNodes("//tr[@class='settings-phones-item']/@data-phone");

                    foreach (var link in phoneNodes) phones.Add(link.GetAttributeValue("data-phone", ""));

                    user.CompanyPhones = string.Join(";", phones);
                    return user;
                }
                catch (Exception ex)
                {
                    Logger.Error("Ошибка получения данных о пользователе со страницы профиля: ", ex);
                    MessageBox.Show(
                        "При попытке получения контактной информации с сайта произошла ошибка, попробуйте обновить программу.",
                        "Ошибка чтения профиля", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            else
                MessageBox.Show("Нельзя обновить информацию о пользователе, который не авторизирован в системе",
                    "Ошибка обновления", MessageBoxButton.OK, MessageBoxImage.Error);

            return null;
        }

        public bool HasCredentials()
        {
            if (string.IsNullOrEmpty(_user.UserName) || string.IsNullOrEmpty(_user.UserPassword)) return false;

            return true;
        }
    }
}