﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace License.Model
{
    public class Registration
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string OrganizationName { get; set; }
        public string Password { get; set; }
        public string ServerUserId { get; set; }
        public string ManagerId { get; set; }
    }
}
