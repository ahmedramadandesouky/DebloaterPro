using Microsoft.UI.Xaml.Data;
using Microsoft.Windows.ApplicationModel.DynamicDependency;
using System;


namespace DebloaterPro.Converters
{
    public class VersionToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value is PackageVersion version ? $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}" : string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
