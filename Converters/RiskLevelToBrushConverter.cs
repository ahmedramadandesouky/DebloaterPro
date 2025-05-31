using DebloaterPro.Pages;
using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using System;


namespace DebloaterPro.Converters
{
    public partial class RiskLevelToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value switch
            {
                AppRiskLevel.Safe => new SolidColorBrush(Colors.Green),
                AppRiskLevel.Moderate => new SolidColorBrush(Colors.Orange),
                AppRiskLevel.Critical => new SolidColorBrush(Colors.DarkGoldenrod),
                AppRiskLevel.Dangerous => new SolidColorBrush(Colors.Red),
                _ => new SolidColorBrush(Colors.Gray)
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotImplementedException();
    }
}
