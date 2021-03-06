﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using License.Core.Model;
using License.Logic.Common;
using License.DataModel;
using Microsoft.AspNet.Identity;
using DataModelTeamMember = License.DataModel.TeamMember;

namespace License.Logic.DataLogic
{
    public class TeamMemberLogic : BaseLogic
    {
        public DataModelTeamMember CreateInvite(DataModelTeamMember invit)
        {
            License.Core.Model.TeamMember userinvit = AutoMapper.Mapper.Map<DataModel.TeamMember, License.Core.Model.TeamMember>(invit);
            var obj = Work.TeamMemberRepository.GetData(f => f.TeamId == invit.TeamId && f.InviteeEmail == invit.InviteeEmail).FirstOrDefault();
            if (obj == null)
            {
                obj = Work.TeamMemberRepository.Create(userinvit);
                Work.TeamMemberRepository.Save();
            }
            return AutoMapper.Mapper.Map<License.Core.Model.TeamMember, DataModelTeamMember>(obj);
        }

        public List<DataModelTeamMember> GetTeamMembers(int TeamId)
        {
            List<DataModelTeamMember> teamMembers = new List<DataModelTeamMember>();
            var listData = Work.TeamMemberRepository.GetData(filter: t => t.TeamId == TeamId);
            foreach (var data in listData)
                teamMembers.Add(AutoMapper.Mapper.Map<Core.Model.TeamMember, DataModel.TeamMember>(data));
            return teamMembers;
        }

        public DataModelTeamMember VerifyUserInvited(string email, string adminid)
        {
            var team = Work.TeamRepository.GetData(r => r.AdminId == adminid).FirstOrDefault();
            var obj = Work.TeamMemberRepository.GetData(filter: t => t.TeamId == team.Id && t.InviteeEmail == email).FirstOrDefault();
            return AutoMapper.Mapper.Map<License.Core.Model.TeamMember, DataModelTeamMember>(obj);
        }

        public void UpdateInviteStatus(object inviteId, string status)
        {
            Core.Model.TeamMember invite = Work.TeamMemberRepository.GetById(inviteId);
            invite.InviteeStatus = status;
            Core.Model.TeamMember ember = Work.TeamMemberRepository.Update(invite);
            Work.TeamMemberRepository.Save();
        }

        public List<DataModel.TeamMember> GetTeamMemberDetailsByUserId(string userId)
        {
            var obj = Work.TeamMemberRepository.GetData(t => t.InviteeUserId == userId).ToList();
            return AutoMapper.Mapper.Map<List<DataModel.TeamMember>>(obj);
        }

        public void SetAsAdmin(int id, string userId, bool adminStatus)
        {

            Core.Model.TeamMember teamMember = Work.TeamMemberRepository.GetById(id);
            var team = Work.TeamRepository.GetById(teamMember.TeamId);
            var teamMemberList = Work.TeamMemberRepository.GetData(t => t.InviteeUserId == userId && t.Team.AdminId == team.AdminId).ToList();
            if (adminStatus)
            {
                if (!RoleManager.RoleExists("Admin"))
                    RoleManager.Create(new Core.Model.Role() { Name = "Admin" });
                UserManager.AddToRole(userId, "Admin");
            }
            else
                UserManager.RemoveFromRole(userId, "Admin");
            int count = 0;
            foreach (var teamMembers in teamMemberList)
            {
                teamMembers.IsAdmin = adminStatus;
                Work.TeamMemberRepository.Update(teamMembers);
                count++;
            }
            if (count > 0)
                Work.TeamMemberRepository.Save();
        }

        public bool DeleteTeamMember(DataModelTeamMember teamMember)
        {
            var obj = Work.TeamMemberRepository.GetData(t => t.InviteeUserId == teamMember.InviteeUserId && t.TeamId == teamMember.TeamId).FirstOrDefault();
            if (obj != null)
                return DeleteTeamMember(obj.Id);
            return false;
        }

        public bool DeleteTeamMember(int id)
        {
            try
            {
                var teamObj = Work.TeamMemberRepository.GetById(id);
                var licenseData = Work.UserLicenseRepository.GetData(u => u.UserId == teamObj.InviteeUserId && u.TeamId == teamObj.TeamId);
                if (licenseData.Count() > 0)
                {
                    int i = 0;
                    foreach (var dt in licenseData)
                    {
                        i++;
                        if (dt.IsTeamLicense)
                        {
                            var teamLicense = Work.TeamLicenseRepository.GetById(dt.TeamLicenseId);
                            teamLicense.IsMapped = false;
                            Work.TeamLicenseRepository.Update(teamLicense);
                            Work.TeamLicenseRepository.Save();
                        }
                        Work.UserLicenseRepository.Delete(dt);
                    }
                    if (i > 0)
                        Work.UserLicenseRepository.Save();

                }
                var team = Work.TeamRepository.GetById(teamObj.TeamId);


                var membList = Work.TeamMemberRepository.GetData(t => t.InviteeUserId == teamObj.InviteeUserId && team.AdminId == teamObj.Team.AdminId).ToList();
                int count = membList.Where(t => t.Id != teamObj.Id).Count();
                if (teamObj.IsAdmin && count == 0)
                    UserManager.RemoveFromRole(teamObj.InviteeUserId, "Admin");
                var status = Work.TeamMemberRepository.Delete(teamObj);
                Work.TeamMemberRepository.Save();
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
