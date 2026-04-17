using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Data;

namespace FleetManager.Converters
{
    /// <summary>
    /// Convertisseur pour obtenir la valeur maximale d'une collection
    /// Utilisé pour calculer le pourcentage des barres dans les graphiques
    /// </summary>
    public class MaxValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return 0.0;
            
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
