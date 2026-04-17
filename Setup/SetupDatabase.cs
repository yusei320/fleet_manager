using Microsoft.Data.Sqlite;
using System;
using System.IO;

class Program
{
    static void Main()
    {
        string connectionString = "Data Source=..\\fleet_manager.db";
        string scriptPath = "create_database.sql";
        
        if (!File.Exists(scriptPath))
        {
            Console.WriteLine($"Erreur : Le fichier {scriptPath} n'existe pas.");
            return;
        }

        string sqlScript = File.ReadAllText(scriptPath);
        
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            
            // Séparer le script en commandes individuelles en ignorant les commentaires
            var lines = sqlScript.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            var currentCommand = "";
            int commandCount = 0;
            
            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                
                // Ignorer les lignes vides et les commentaires
                if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith("--"))
                    continue;
                
                currentCommand += " " + trimmedLine;
                
                // Si la ligne se termine par un point-virgule, exécuter la commande
                if (trimmedLine.EndsWith(";"))
                {
                    var commandToExecute = currentCommand.Trim();
                    if (!string.IsNullOrEmpty(commandToExecute))
                    {
                        using (var cmd = new SqliteCommand(commandToExecute, connection))
                        {
                            try
                            {
                                cmd.ExecuteNonQuery();
                                commandCount++;
                                Console.WriteLine($"Exécuté ({commandCount}) : {commandToExecute.Substring(0, Math.Min(50, commandToExecute.Length))}...");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Erreur lors de l'exécution : {ex.Message}");
                            }
                        }
                    }
                    currentCommand = "";
                }
            }
            
            Console.WriteLine($"Total de commandes exécutées : {commandCount}");
        }
        
        Console.WriteLine("Base de données créée avec succès !");
    }
}
