
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.ViewModels.User
{
    public class UserRequestCreateDTO
    {
        public required string Email { get; set; }
        public required string Fullname { get; set; }
        public required string Role { get; set; }
    }
}
