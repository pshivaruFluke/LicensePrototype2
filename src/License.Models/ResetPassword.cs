﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace License.Models
{
    public class ResetPassword
    {
        public string Password { get; set; }

        public string ConfirmPassword { get; set; }

        public string UserId { get; set; }

        public string Token { get; set; }

        public string ServerUserId { get; set; }
    }
}