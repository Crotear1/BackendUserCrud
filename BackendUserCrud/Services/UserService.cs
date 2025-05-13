// Services/UserService.cs
using MySql.Data.MySqlClient;
using UserManagementAPI.Models;
using System.Data;

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
            if (userToCreate == null || string.IsNullOrWhiteSpace(userToCreate.Username) || string.IsNullOrWhiteSpace(userToCreate.Email))
            {
                return null;
            }

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var query = @"
            INSERT INTO users (username, email)
            VALUES (@Username, @Email);
            SELECT id, username, email, created_at FROM users WHERE id = LAST_INSERT_ID();";

                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Username", userToCreate.Username);
                    command.Parameters.AddWithValue("@Email", userToCreate.Email);

                    try
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                return new User
                                {
                                    Id = reader.GetInt32("id"),
                                    Username = reader.GetString("username"),
                                    Email = reader.GetString("email"),
                                    CreatedAt = reader.GetDateTime("created_at")
                                };
                            }
                        }
                    }
                    catch (MySqlException ex)
                    {
                        return null;
                    }
                    catch (Exception ex)
                    {
                        return null;
                    }
                }
            }
            return null;
        }


        /// <summary>
        /// Ruft einen einzelnen Benutzer anhand seiner ID aus der Datenbank ab.
        /// </summary>
        /// <param name="id">Die ID des gesuchten Benutzers.</param>
        /// <returns>Den gefundenen User oder null, wenn kein Benutzer mit dieser ID existiert oder ein Fehler auftritt.</returns>
        public async Task<User?> GetUserByIdAsync(int id)
        {
            User? user = null;

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var query = "SELECT id, username, email, created_at FROM users WHERE id = @Id;";

                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);

                    try
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                user = new User
                                {
                                    Id = reader.GetInt32("id"),
                                    Username = reader.GetString("username"),
                                    Email = reader.GetString("email"),
                                    CreatedAt = reader.GetDateTime("created_at")
                                };
                            }
                        }
                    }
                    catch (MySqlException ex)
                    {
                        Console.WriteLine($"MySQL Fehler in GetUserByIdAsync für ID {id}: {ex.Message}");.
                        return null;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Allgemeiner Fehler in GetUserByIdAsync für ID {id}: {ex.Message}");
                        return null;
                    }
                }
            }

            return user;
        }
    }
}
