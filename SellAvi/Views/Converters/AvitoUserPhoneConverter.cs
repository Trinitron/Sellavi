using System;
using System.Globalization;
using System.Windows.Data;
using SellAvi.Model.DataBaseModels;

namespace SellAvi.Views.Converters
{
    public class AvitoUserPhoneConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var avitoUser = values[0] as AvitoUser;
            if (avitoUser == null || avitoUser.CompanyPhones == null)
                return null;

            var telIndex = (int) values[1];
            var phonesArray = avitoUser.CompanyPhones.Split(';');
            MoveWithinArray(phonesArray, telIndex, 0);
            avitoUser.CompanyPhones = string.Join(";", phonesArray);
            return avitoUser;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private void MoveWithinArray(Array array, int source, int dest)
        {
            var temp = array.GetValue(source);
            Array.Copy(array, dest, array, dest + 1, source - dest);
            array.SetValue(temp, dest);
        }
    }
}