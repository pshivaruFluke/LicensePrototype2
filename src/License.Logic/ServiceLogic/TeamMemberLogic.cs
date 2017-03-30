﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using License.Core.Model;
using License.Logic.Common;
using License.Model;
using Microsoft.AspNet.Identity;
using TeamMembers = License.Model.TeamMembers;

namespace License.Logic.ServiceLogic
{
	public class TeamMemberLogic : BaseLogic
	{
		public TeamMembers CreateInvite(TeamMembers invit)
		{
			License.Core.Model.TeamMembers userinvit = AutoMapper.Mapper.Map<Model.TeamMembers, License.Core.Model.TeamMembers>(invit);
			var obj = Work.UserInviteRepository.GetData(f => f.AdminId == invit.AdminId && f.InviteeEmail == invit.InviteeEmail && f.TeamId == invit.TeamId).FirstOrDefault();
			if (obj == null)
			{
				obj = Work.UserInviteRepository.Create(userinvit);
				Work.UserInviteRepository.Save();
			}
			return AutoMapper.Mapper.Map<License.Core.Model.TeamMembers, TeamMembers>(obj);
		}

		public UserInviteList GetUserInviteDetails(string adminId)
		{
			AppUser user = UserManager.FindById(adminId);
			UserInviteList inviteList = new UserInviteList();
			inviteList.AdminUser = AutoMapper.Mapper.Map<AppUser, User>(user);
			List<TeamMembers> teamMembers = new List<TeamMembers>();
			var listData = Work.UserInviteRepository.GetData(filter: t => t.AdminId == adminId);
			foreach (var data in listData)
				teamMembers.Add(AutoMapper.Mapper.Map<Core.Model.TeamMembers, Model.TeamMembers>(data));
			if (teamMembers.Count > 0)
			{
				inviteList.PendingInvites =
					teamMembers.Where(s => s.InviteeStatus == InviteStatus.Pending.ToString()).ToList();
				inviteList.AcceptedInvites =
					teamMembers.Where(s => s.InviteeStatus == InviteStatus.Accepted.ToString()).ToList();
				inviteList.AcceptedInvites.Add(new TeamMembers()
				{
					AdminId = adminId,
					InviteeEmail = user.Email,
					InviteeStatus = InviteStatus.Accepted.ToString(),
					InviteeUserId = adminId,
                    InviteeUser = inviteList.AdminUser,
                    IsAdmin = true
				});
			}
			else
			{

				inviteList.AcceptedInvites.Add(new TeamMembers()
				{
					AdminId = adminId,
					InviteeEmail = user.Email,
					InviteeStatus = InviteStatus.Accepted.ToString(),
					InviteeUserId = adminId,
                    InviteeUser = inviteList.AdminUser,
                    IsAdmin = true
				});
			}
			return inviteList;
		}

		public TeamMembers VerifyUserInvited(string email, string adminid)
		{
			var obj = Work.UserInviteRepository.GetData(filter: t => t.AdminId == adminid && t.InviteeEmail == email).FirstOrDefault();
			return AutoMapper.Mapper.Map<License.Core.Model.TeamMembers, TeamMembers>(obj);
		}

		public void UpdateInviteStatus(object inviteId, string status)
		{
			Core.Model.TeamMembers invite = Work.UserInviteRepository.GetById(inviteId);
			invite.InviteeStatus = status;
			Core.Model.TeamMembers ember = Work.UserInviteRepository.Update(invite);
			Work.UserInviteRepository.Save();
		}

		public string GetUserAdminDetails(string userId)
		{
			var obj = Work.UserInviteRepository.GetData(t => t.InviteeUserId == userId).FirstOrDefault();
			if (obj != null)
				return obj.AdminId;
			return string.Empty;
		}

		public void SetAsAdmin(int id, string userId, bool status)
		{
			Core.Model.TeamMembers teamMembers = Work.UserInviteRepository.GetById(id);
			if (!RoleManager.RoleExists("Admin"))
				RoleManager.Create(new Core.Model.Role() { Name = "Admin" });
			UserManager.AddToRole(userId, "Admin");
			teamMembers.IsAdmin = status;
			Work.UserInviteRepository.Update(teamMembers);
			Work.UserInviteRepository.Save();
		}

		public bool DeleteTeamMember(int id)
		{
			try
			{
				var teamObj = Work.UserInviteRepository.GetById(id);
				var licenseData = Work.UserLicenseRepository.GetData(u => u.UserId == teamObj.InviteeUserId);
				if (licenseData.Count() > 0)
					foreach (var dt in licenseData)
						Work.UserLicenseRepository.Delete(dt);
				var status = Work.UserInviteRepository.Delete(teamObj);
				Work.UserInviteRepository.Save();
				return true;
			}
			catch (Exception ex)
			{
				ErrorMessage = ex.Message;
			}
			return false;

		}

	}
}
