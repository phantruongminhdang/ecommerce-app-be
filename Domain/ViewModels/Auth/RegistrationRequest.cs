using Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.ViewModels.Auth
{
    public class RegistrationRequest
    {
        [Required]
        public required string Email { get; set; }

        [Required]
        public required string Username { get; set; }

        [Required]
        public required string Fullname { get; set; }

        [Required]
        public required string Password { get; set; }

        [Required]
        public required Role Role { get; set; }
    }
}
