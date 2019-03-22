using System;
using System.Windows;
using System.Windows.Controls;

namespace SellAvi.Views
{
    /// <summary>
    ///     Interaction logic for TimeControl.xaml
    /// </summary>
    public partial class ControlTimePicker : UserControl
    {
        public static readonly DependencyProperty DateTimeValueProperty =
            DependencyProperty.Register("DateTimeValue", typeof(DateTime), typeof(ControlTimePicker),
                null /*new UIPropertyMetadata(DateTime.Now)*/);

        public ControlTimePicker()
        {
            InitializeComponent();
            //DataContext = this;
        }

        public DateTime DateTimeValue
        {
            get => (DateTime) GetValue(DateTimeValueProperty);
            set => SetValue(DateTimeValueProperty, value);
        }

        private void TryCreateDateTime()
        {
            {
                var value = string.Format("{2} {0}:{1}", DHours, DMinutes, DDate.ToShortDateString());
                DateTime date;
                var isValidDate = DateTime.TryParse(value, out date);
                if (isValidDate) DateTimeValue = date;
            }
        }

        #region H:M:D

        private string _dHours = "";
        private string _dMinutes = "";
        private DateTime _dDate = new DateTime(2019,07,17);

        public string DMinutes
        {
            get => _dMinutes;
            set
            {
                _dMinutes = value;
                TryCreateDateTime();
            }
        }

        public string DHours
        {
            get => _dHours;
            set
            {
                _dHours = value;
                TryCreateDateTime();
            }
        }

        public DateTime DDate
        {
            get => _dDate;
            set
            {
                _dDate = value;
                TryCreateDateTime();
            }
        }

        #endregion
    }
}