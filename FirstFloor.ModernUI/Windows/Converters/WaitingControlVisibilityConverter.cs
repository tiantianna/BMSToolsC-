using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace FirstFloor.ModernUI.Windows.Converters
{
    public enum EnumStatus
    {
        None,
        Initializing,
        Initialized,
        Loading,
        Loaded,
        Saving,
        Saved
    }
      

    public class WaitingControlVisibilityConverter : IValueConverter
    {
      

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                string status = value.ToString();
                switch (status)
                {
                    case "Initializing":
                    case "Loading":
                    case "Saving":
                        return Visibility.Visible;
                    case "Initialized":
                    case "Loaded":
                    case "Saved":
                    default:
                        return Visibility.Collapsed;
                }
            }
            catch (Exception)
            {
                return Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
