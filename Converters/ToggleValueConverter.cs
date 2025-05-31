using Microsoft.UI.Xaml.Data;
using System;


namespace DebloaterPro.Converters
{
    public class ToggleValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is int i)
            {
                return i == 1 ? "Disable" : "Enable";
            }
            else
            {
                return "Set";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
