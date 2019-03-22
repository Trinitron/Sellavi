using System.Text.RegularExpressions;

namespace SellAvi.Classes
{
    public static class AvitoFormats
    {
        /// <summary>
        ///     Приведение цены объявления к числовому виду, без пробелов, отступов, валюты
        /// </summary>
        /// <param name="priceUnformatted"></param>
        /// <returns></returns>
        public static string TrimPrice(this string priceUnformatted)
        {
            var formattedPrice = priceUnformatted
                .Replace("&nbsp;", "")
                .Replace(" руб.", "")
                .Replace(" ", "")
                .Replace("\n", "")
                .Replace("&thinsp;", "")
                .Replace("₽", "")
                .Replace("Цена не указана", "");
            formattedPrice = Regex.Match(formattedPrice, @"\d+").Value;
            return formattedPrice;
        }
    }
}