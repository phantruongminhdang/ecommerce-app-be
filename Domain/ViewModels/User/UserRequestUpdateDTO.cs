﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.ViewModels.User
{
    public class UserRequestUpdateDTO
    {
        public required string Username { get; set; }
        public required string Fullname { get; set; }
        public required string PhoneNumber { get; set; }
    }
}
