#nullable disable

namespace Master.API.Models.DTOs
{
    public class RegisterRequest
    {

        public string FullName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }

        public string Role { get; set; }




    }
}
