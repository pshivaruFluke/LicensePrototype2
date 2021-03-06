﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace License.Core.Model
{
    public class AppUser : IdentityUser
    {
        [NotMapped]
        public string UserId
        {
            get { return base.Id; }
        }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        [DataType(DataType.EmailAddress, ErrorMessage = "Please enter Valid Email Address")]
        [Required(ErrorMessage = "The Email Address is Requuired")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public override string Email
        {
            get { return base.Email; }
            set { base.Email = value; }
        }

        [RegularExpression(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$", ErrorMessage = "Entered phone format is not valid.")]
        public override string PhoneNumber
        {
            get
            {
                return base.PhoneNumber;
            }
            set
            {
                base.PhoneNumber = value;
            }
        }

        [Display(Name = "Email")]
        [Required(ErrorMessage = "Email Address is Required")]
        public override string UserName
        {
            get
            {
                return base.UserName;
            }
            set
            {
                base.UserName = value;
            }
        }

        public string ServerUserId { get; set; }

        public bool IsActive { get; set; }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<AppUser> manager, string authenticationType)
        {
            var userIdentity = await manager.CreateIdentityAsync(this, authenticationType);
            // Add custom user claims here
            return userIdentity;
        }
    }
}
