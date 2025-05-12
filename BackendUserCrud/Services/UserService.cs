// Services/UserService.cs
using MySql.Data.MySqlClient;
using UserManagementAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Data; // Für IConfiguration

namespace UserManagementAPI.Services
{
    public class UserService
    {
        private readonly string _connectionString;

        public UserService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        }

        public async Task<List<User>> GetUsersAsync()
        {
            var users = new List<User>();
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var query = "SELECT Id, Username, Email, Created_At FROM users ORDER BY Username";
                using (var command = new MySqlCommand(query, connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            users.Add(new User
                            {
                                Id = reader.GetInt32("Id"),
                                Username = reader.GetString("Username"),
                                Email = reader.GetString("Email"),
                                CreatedAt = reader.GetDateTime("Created_At")
                            });
                        }
                    }
                }
            }
            return users;
        }

        /// <summary>
        /// Erstellt einen neuen Benutzer in der Datenbank.
        /// </summary>
        /// <param name="userToCreate">Das User-Objekt mit den Daten für den neuen Benutzer (Username und Email müssen gesetzt sein).</param>
        /// <returns>Den erstellten User mit von der DB generierter Id und CreatedAt, oder null bei einem Fehler.</returns>
        public async Task<User?> CreateUserAsync(User userToCreate)
        {
            // Überprüfe, ob die notwendigen Daten vorhanden sind
            if (userToCreate == null || string.IsNullOrWhiteSpace(userToCreate.Username) || string.IsNullOrWhiteSpace(userToCreate.Email))
            {
                // Hier könntest du eine ArgumentNullException oder ArgumentException werfen
                // oder einfach null zurückgeben, je nachdem, wie du Fehler behandeln möchtest.
                // Console.WriteLine("User-Objekt oder dessen Username/Email sind ungültig für die Erstellung.");
                return null;
            }

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                // SQL-Befehl zum Einfügen des Benutzers und zum Abrufen des neu erstellten Datensatzes.
                // 'id' ist AUTO_INCREMENT und 'created_at' hat einen DEFAULT CURRENT_TIMESTAMP.
                var query = @"
            INSERT INTO users (username, email)
            VALUES (@Username, @Email);
            SELECT id, username, email, created_at FROM users WHERE id = LAST_INSERT_ID();";
                // Wichtig: Spaltennamen in SELECT müssen exakt mit denen in der DB übereinstimmen (id, username, email, created_at)

                using (var command = new MySqlCommand(query, connection))
                {
                    // Parameter hinzufügen, um SQL-Injection zu verhindern
                    command.Parameters.AddWithValue("@Username", userToCreate.Username);
                    command.Parameters.AddWithValue("@Email", userToCreate.Email);

                    try
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync()) // Lese den durch SELECT zurückgegebenen Datensatz
                            {
                                return new User
                                {
                                    Id = reader.GetInt32("id"), // Achte auf korrekte Spaltennamen
                                    Username = reader.GetString("username"),
                                    Email = reader.GetString("email"),
                                    CreatedAt = reader.GetDateTime("created_at")
                                };
                            }
                        }
                    }
                    catch (MySqlException ex)
                    {
                        // Fehlerbehandlung, z.B. wenn die E-Mail bereits existiert (Unique Constraint Violation)
                        // Fehlercode 1062 für Duplicate entry
                        // Console.WriteLine($"MySQL Fehler beim Erstellen des Benutzers: {ex.Message} (Nummer: {ex.Number})");
                        // Hier könntest du spezifische Fehler loggen oder basierend auf ex.Number anders reagieren.
                        return null; // Gib null zurück oder wirf eine spezifischere Ausnahme
                    }
                    catch (Exception ex)
                    {
                        // Allgemeine Fehlerbehandlung
                        // Console.WriteLine($"Allgemeiner Fehler beim Erstellen des Benutzers: {ex.Message}");
                        return null;
                    }
                }
            }
            return null; // Sollte nicht erreicht werden, wenn alles gut geht, oder wenn ein Fehler oben nicht abgefangen wurde
        }


        /// <summary>
        /// Ruft einen einzelnen Benutzer anhand seiner ID aus der Datenbank ab.
        /// </summary>
        /// <param name="id">Die ID des gesuchten Benutzers.</param>
        /// <returns>Den gefundenen User oder null, wenn kein Benutzer mit dieser ID existiert oder ein Fehler auftritt.</returns>
        public async Task<User?> GetUserByIdAsync(int id)
        {
            User? user = null; // Initialisiere mit null, falls kein Benutzer gefunden wird

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                // SQL-Befehl zum Auswählen eines einzelnen Benutzers anhand seiner ID.
                // Achte darauf, dass die Spaltennamen (id, username, email, created_at)
                // exakt mit denen in deiner Datenbanktabelle übereinstimmen.
                var query = "SELECT id, username, email, created_at FROM users WHERE id = @Id;";

                using (var command = new MySqlCommand(query, connection))
                {
                    // Parameter hinzufügen, um SQL-Injection zu verhindern.
                    // Der Parametername @Id muss mit dem im Query übereinstimmen.
                    command.Parameters.AddWithValue("@Id", id);

                    try
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync()) // Wenn ein Datensatz gefunden wurde
                            {
                                user = new User
                                {
                                    Id = reader.GetInt32("id"),
                                    Username = reader.GetString("username"),
                                    Email = reader.GetString("email"),
                                    CreatedAt = reader.GetDateTime("created_at")
                                };
                            }
                            // Wenn reader.ReadAsync() false zurückgibt, wurde kein Benutzer gefunden,
                            // und 'user' bleibt null.
                        }
                    }
                    catch (MySqlException ex)
                    {
                        // Hier solltest du den Fehler loggen, z.B. mit einem ILogger.
                        // _logger?.LogError(ex, "Ein MySQL-Fehler ist beim Abrufen des Benutzers mit ID {UserId} aufgetreten.", id);
                        Console.WriteLine($"MySQL Fehler in GetUserByIdAsync für ID {id}: {ex.Message}");
                        // In diesem Fall geben wir null zurück, was der Controller als "Nicht gefunden" oder Fehler interpretieren kann.
                        return null;
                    }
                    catch (Exception ex)
                    {
                        // Allgemeiner Fehler
                        // _logger?.LogError(ex, "Ein allgemeiner Fehler ist beim Abrufen des Benutzers mit ID {UserId} aufgetreten.", id);
                        Console.WriteLine($"Allgemeiner Fehler in GetUserByIdAsync für ID {id}: {ex.Message}");
                        return null;
                    }
                }
            }

            return user; // Gibt den gefundenen Benutzer zurück oder null, wenn nicht gefunden oder ein Fehler auftrat.
        }
    }
}
