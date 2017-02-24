﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace License.Core.Model
{
    public class TeamMembers
    {
        [Key]
        public int Id { get; set; }
        public string AdminId { get; set; }
        public int TeamId { get; set; }
        public string InviteeEmail { get; set; }
        public string InviteeUserId { get; set; }
        public string InviteeStatus { get; set; }
        public DateTime InvitationDate { get; set; }
        [ForeignKey("AdminId")]
        public AppUser AdminUser { get; set; }
        [ForeignKey("InviteeUserId")]
        public AppUser InviteeUser { get; set; }
        public bool IsAdmin { get; set; }
    }
}