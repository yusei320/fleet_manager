using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FleetManager.Models
{
    /// <summary>
    /// Classe de base pour l'implémentation de INotifyPropertyChanged
    /// Utilisée par tous les ViewModels pour notifier les changements de propriétés
    /// </summary>
    public class ObservableBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Méthode pour notifier qu'une propriété a changé
        /// </summary>
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Méthode pour mettre à jour une propriété et notifier le changement
        /// </summary>
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
