using System;

namespace SellAvi.Schemas
{
    /// <summary>
    ///     Класс, описывает как парсить определенную часть сайта
    /// </summary>
    public abstract class AbstractSchema
    {
        protected AbstractSchema()
        {
            SiteUrl = new Uri("https://www.avito.ru/");
        }

        /// <summary>
        ///     Сайт (Avito)
        /// </summary>
        public Uri SiteUrl { get; set; }

        /// <summary>
        ///     Ссылка на объявления в табличном виде (индекс страница)
        /// </summary>
        public virtual Uri IndexUrl { get; set; }

        /// <summary>
        ///     XPath для доступа к списку заголовков объявлений (DIV), внутри которых есть Заголовок и URL (h3/a[title=''])
        ///     "//div[@class='profile-item-main']";        - активные объявления из профиля
        ///     "//div[@class='item_table-header']";        - объявления из поиска на сайте
        ///     "//div[@class='profile-item-description']"; - завершенные объявления на активацию
        /// </summary>
        public virtual string XPathTitleNode { get; set; }
    }
}