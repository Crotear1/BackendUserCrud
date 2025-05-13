// Controllers/UsersController.cs
using Microsoft.AspNetCore.Mvc;
using UserManagementAPI.Models;
using UserManagementAPI.Services;

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
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // POST: api/users
        [HttpPost]
        public async Task<ActionResult<User>> PostUser([FromBody] User userToCreate)
        {
            if (userToCreate == null)
            {
                return BadRequest("User data cannot be null.");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                User? createdUser = await _userService.CreateUserAsync(userToCreate);

                if (createdUser == null)
                {
                    return BadRequest("Could not create user. The email might already exist or the input is invalid.");
                }
                return CreatedAtAction(nameof(GetUserById), new { id = createdUser.Id }, createdUser);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An internal server error occurred. Please try again later.");
            }
        }

        // Get: api/users/{id}
        [HttpGet("{id:int}", Name = "GetUserById")]
         public async Task<ActionResult<User>> GetUserById(int id)
         {
             var user = await _userService.GetUserByIdAsync(id);
        
             if (user == null)
             {
                 return NotFound();
             }
        
             return Ok(user);
        }

        // Hier kommt später DELETE Endpunkt hinzu
    }
}
