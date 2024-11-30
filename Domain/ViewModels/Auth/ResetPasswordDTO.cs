using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.ViewModels.Auth
{
    public class ResetPasswordDTO
    {
        public required string NewPassword { get; set; }
        public required string ConfirmPassword { get; set; }
    }
}
