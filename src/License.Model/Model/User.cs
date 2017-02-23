﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace License.Model.Model
{
    public class User
    {
        public string UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string UserName { get; set; }
        public int OrganizationId { get; set; }
        public Organization Organization { get; set; }
        public string Name { get; set; }
        public IList<string> Roles { get; set; }
    }
}
