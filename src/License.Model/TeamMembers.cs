﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace License.DataModel
{
    public class Team
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string AdminId { get; set; }
        public User AdminUser { get; set; }
        public bool IsDefaultTeam { get; set; }
        public int ConcurrentUserCount { get; set; }
        public ICollection<TeamMember> TeamMembers { get; set; }
    }

    public class TeamMember
    {
        public int Id { get; set; }
        public int TeamId { get; set; }
        public string InviteeEmail { get; set; }
        public string InviteeUserId { get; set; }
        public string InviteeStatus { get; set; }
        public DateTime InvitationDate { get; set; }
        public User InviteeUser { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsSelected { get; set; }
        public bool IsActive { get; set; }
        [JsonIgnore]
        public Team Team { get; set; }
    }

    public class TeamConcurrentUserResponse
    {
        public int TeamId { get; set; }
        public bool UserUpdateStatus { get; set; }
        public string ErrorMessage { get; set; }
        public int OldUserCount { get; set; }
    }

    public class TeamDetails
    {
        public Team Team { get; set; }
        public List<TeamMember> PendinigUsers { get; set; }
        public List<TeamMember> AcceptedUsers { get; set; }

        public TeamDetails()
        {
            PendinigUsers = new List<TeamMember>();
            AcceptedUsers = new List<TeamMember>();
        }
    }

    public class TeamMemberResponse
    {
        public string UserId { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public int TeamMemberId { get; set; }
    }
}
