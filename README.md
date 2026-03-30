# Fleet Manager

Fleet Manager est une application de bureau (WPF) développée en C# avec le framework .NET 8. Elle permet de gérer une flotte de véhicules, d'effectuer un suivi rigoureux de leur état et utilisation, et de gérer les différents utilisateurs du système avec des rôles spécifiques.

## Fonctionnalités Principales

- **Authentification & Autorisation :** Connexion sécurisée avec gestion des rôles (Administrateur, Utilisateur standard).
- **Gestion des Véhicules :** Ajout, modification, et gestion des informations pour chaque véhicule de la flotte (`AddVehicleWindow`).
- **Suivi des Véhicules :** Enregistrement et consultation des interventions ou suivis d'utilisation liés aux véhicules (`AddSuiviWindow`).
- **Tableau de Bord & Statistiques :** Visualisation des données et statistiques de la flotte grâce à des graphiques via la bibliothèque `OxyPlot`.
- **Panneau d'Administration :** Espace dédié à la gestion des comptes pour créer ou éditer les utilisateurs de l'application (`AdminWindow`, `CreateUserWindow`).
- **Sécurité et Fiabilité :** Validation et nettoyage strict des données saisies (grâce à `HtmlSanitizer`) pour éviter les vulnérabilités classiques telles que la faille XSS.

## Stack Technique

- **Langage principal :** C#
- **Framework :** .NET 8.0 (Windows)
- **Interface Utilisateur :** WPF (Windows Presentation Foundation)
- **Base de Données :** MySQL (communicant à l'aide de `MySql.Data`)
- **Visualisation de de données :** OxyPlot : (`OxyPlot.Core`, `OxyPlot.Wpf`)
- **Sécurité :** HtmlSanitizer

## Prérequis et Installation

1. **Outils nécessaires :** 
   - [Visual Studio 2022](https://visualstudio.microsoft.com/) avec la charge de travail ".NET desktop development".
   - [SDK .NET 8.0](https://dotnet.microsoft.com/download/dotnet/8.0).

2. **Configuration de la Base de données :**
   - Un serveur MySQL (par défaut, configuré sur le port `3309`).
   - Assurez-vous d'avoir une base de données nommée `fleet_managers` intégrant les différentes tables requises (comme `utilisateurs`, `vehicules`, `suivis`, etc.).
   - Vous pouvez modifier la chaîne de connexion (Connection String) directement dans les classes d'accès aux données comme `AuthService.cs` :
     ```csharp
     "server=localhost;Port=3309;database=fleet_managers;uid=root;pwd=;";
     ```

3. **Lancement de l'application :**
   - Ouvrez la solution `FleetManager.sln` sous Visual Studio.
   - Les packages de dépendances NuGet (MySql.Data, HtmlSanitizer, OxyPlot) seront automatiquement restaurés par la solution à la compilation.
   - Initialisez la cible de démarrage sur le projet `FleetManager`.
   - Lancez l'application.

## Structure globale des fenêtres

- `MainWindow.xaml` : Fenêtre principale servant de tableau de bord et point d'entrée après authentification.
- `LoginWindow.xaml` : Écran d'authentification pour accéder à l'application.
- `AdminWindow.xaml` : Espace d'administration central réservé aux administrateurs.
- `CreateUserWindow.xaml` : Création de nouveaux profils utilisateurs.
- `AddVehicleWindow.xaml` : Déclaration de nouveaux véhicules dans la base de données de la flotte.
- `AddSuiviWindow.xaml` : Ajout d'enregistrements de suivi d'utilisation ou de réparation pour un véhicule sélectionné.
