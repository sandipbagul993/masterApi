using Master.API.Models;
using Master.API.Models.DTOs;
using Master.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Master.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly MasterDBContext _context;

        public AccountController(IAuthService authService, MasterDBContext context)
        {
            _authService = authService;
            _context = context;
        }


        [HttpGet("GetAllUsers")]
        public async Task<ActionResult<IEnumerable<User>>> GetAllUsers()
        {
            var data = await _context.Users.ToListAsync();
            return Ok(data);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {

            try
            {
                var token = await _authService.RegisterAsync(request);
                return Ok(new
                {
                    Result = true,
                    Message = "Registration Succesfull",
                    Data = new
                    {

                        Email = request.Email,
                        Token = token
                    }

                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _context.Users.Where(u => u.Email == request.Username).FirstOrDefaultAsync();
            if (user == null) return Unauthorized();
            try
            {
                var token = await _authService.LoginAsync(request);
                return Ok(new
                {
                    Result = true,
                    Message = "Login Succesfull",
                    Data = new
                    {
                        Role = user.Role,
                        Id = user.UserId,
                        Username = request.Username,
                        Token = token,
                        FullName=user.FullName,
                        Email=user.Email
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }


        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {

            var data = await _context.Users.Where(u => u.UserId == id).FirstOrDefaultAsync();
            if (data == null)
            {
                return NotFound("Invalid User");
            }

            _context.Users.Remove(data);
            await _context.SaveChangesAsync();
            return Ok($"Id - {id} - User Deleted Successfully");
        }

    }
}
