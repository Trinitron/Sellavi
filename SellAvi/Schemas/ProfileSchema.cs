using System;

namespace SellAvi.Schemas
{
    public class ProfileSchema : AbstractSchema
    {
        public ProfileSchema()
        {
            base.XPathTitleNode = "//div[@class='profile-item-main']";
            /*страница активных объявлений профиля*/
            base.IndexUrl = new Uri("https://www.avito.ru/profile/items/active");
        }
    }
}