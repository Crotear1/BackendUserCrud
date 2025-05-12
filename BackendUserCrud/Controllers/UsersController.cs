// Controllers/UsersController.cs
using Microsoft.AspNetCore.Mvc;
using UserManagementAPI.Models;
using UserManagementAPI.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UserManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // Pfad: /api/users
    public class UsersController : ControllerBase
    {
        private readonly UserService _userService;

        public UsersController(UserService userService)
        {
            _userService = userService;
        }

        // GET: api/users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            try
            {
                var users = await _userService.GetUsersAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                // Logge den Fehler (nicht im Produktionscode direkt an den Client senden)
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // POST: api/user
        [HttpPost] // 1. HTTP-Methode auf POST geändert
        public async Task<ActionResult<User>> PostUser([FromBody] User userToCreate) // 2. Parameter für den zu erstellenden User hinzugefügt
        {
            // Optionale, aber empfohlene Eingabevalidierung
            if (userToCreate == null)
            {
                return BadRequest("User data cannot be null.");
            }

            // Wenn du DataAnnotations (z.B. [Required], [EmailAddress]) in deinem User-Modell verwendest,
            // wird ModelState automatisch validiert.
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Gibt detaillierte Validierungsfehler zurück
            }

            try
            {
                // 4. Korrekter Aufruf der Service-Methode mit dem User-Objekt
                User? createdUser = await _userService.CreateUserAsync(userToCreate);

                if (createdUser == null)
                {
                    // 6. Spezifische Fehlerbehandlung, wenn der Service null zurückgibt
                    // (z.B. E-Mail existiert bereits, Validierungsfehler im Service)
                    return BadRequest("Could not create user. The email might already exist or the input is invalid.");
                    // Alternativ könntest du hier einen spezifischeren Statuscode wie 409 Conflict zurückgeben,
                    // wenn du sicher weißt, dass es ein Duplikat war.
                }

                // 5. Erfolgreiche Antwort mit Status 201 Created
                // 'GetUserById' ist der Name der Aktion, die einen einzelnen Benutzer abruft.
                // Diese Aktion musst du ebenfalls in deinem Controller haben.
                return CreatedAtAction(nameof(GetUserById), new { id = createdUser.Id }, createdUser);

                // Einfachere Alternative, falls du noch keine GetUserById-Aktion hast (gibt 200 OK zurück):
                // return Ok(createdUser);

            }
            catch (Exception ex)
            {
                // Allgemeiner Serverfehler - Hier solltest du den Fehler loggen!
                // z.B. _logger.LogError(ex, "An error occurred while creating a user.");
                return StatusCode(500, "An internal server error occurred. Please try again later.");
            }
        }

        // Damit CreatedAtAction korrekt funktioniert, benötigst du eine entsprechende GET-Aktion,
        // die einen Benutzer anhand seiner ID abrufen kann. Hier ein Beispiel:
        // (Diese Methode muss in derselben Controller-Klasse UsersController definiert sein)

         [HttpGet("{id:int}", Name = "GetUserById")] // Name = "GetUserById" kann helfen, wenn nameof() Probleme macht
         public async Task<ActionResult<User>> GetUserById(int id)
         {
             // Annahme: Du hast eine Methode GetUserByIdAsync in deinem UserService
             var user = await _userService.GetUserByIdAsync(id);
        
             if (user == null)
             {
                 return NotFound(); // Status 404, wenn der Benutzer nicht gefunden wurde
             }
        
             return Ok(user); // Status 200 mit dem Benutzerobjekt
        }

        // Hier kommen später POST (Create) und DELETE Endpunkte hinzu
    }
}
