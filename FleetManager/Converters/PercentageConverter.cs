using System;
using System.Globalization;
using System.Windows.Data;

namespace FleetManager.Converters
{
    /// <summary>
    /// Convertisseur simple pour les pourcentages
    /// Retourne la valeur directement (le pourcentage est déjà calculé dans le ViewModel)
    /// </summary>
    public class PercentageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double d)
            {
                return d;
            }
            return 0.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
