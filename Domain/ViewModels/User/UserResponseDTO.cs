using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.ViewModels.User
{
    public class UserResponseDTO
    {
        public string Id { get; set; }
        public string? Fullname { get; set; }
        public string Role { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public bool IsLockout { get; set; } = false;
    }
}
