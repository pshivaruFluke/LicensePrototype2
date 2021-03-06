﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using License.Logic.DataLogic;
using License.DataModel;
using License.Logic.Common;
using License.Core.Manager;
namespace License.Logic.BusinessLogic
{
    public class TeamBO
    {
        UserLogic userLogic = null;
        TeamMemberLogic logic = null;
        TeamLogic teamLogic = null;
        TeamLicenseLogic teamLicenseLogic = null;
        LicenseLogic licLogic = null;
        UserLicenseLogic userLicLogic = null;

        public string ErrorMessage { get; set; }

        public AppUserManager UserManager { get; set; }

        public AppRoleManager RoleManager { get; set; }
        public TeamBO()
        {
            userLogic = new UserLogic();
            logic = new TeamMemberLogic();
            teamLogic = new TeamLogic();
            teamLicenseLogic = new TeamLicenseLogic();
            userLicLogic = new UserLicenseLogic();
            licLogic = new LicenseLogic();
        }

        public void Initialize()
        {
            userLogic.UserManager = UserManager;
            userLogic.RoleManager = RoleManager;
        }

        public TeamDetails GetteamDetails(int id)
        {
            TeamLogic teamLogic = new TeamLogic();
            TeamDetails dtls = new TeamDetails();
            var team = teamLogic.GetTeamById(id);
            if (team != null)
            {
                dtls.Team = new Team();
                dtls.Team.AdminId = team.AdminId;
                dtls.Team.AdminUser = team.AdminUser;
                dtls.Team.Id = team.Id;
                dtls.Team.Name = team.Name;
                if (team.TeamMembers.Count > 0)
                {
                    dtls.PendinigUsers =
                        team.TeamMembers.Where(s => s.InviteeStatus == InviteStatus.Pending.ToString()).ToList();
                    dtls.AcceptedUsers =
                        team.TeamMembers.Where(s => s.InviteeStatus == InviteStatus.Accepted.ToString()).ToList();
                    dtls.AcceptedUsers.Add(new TeamMember()
                    {
                        InviteeEmail = team.AdminUser.Email,
                        InviteeStatus = InviteStatus.Accepted.ToString(),
                        InviteeUserId = team.AdminUser.UserId,
                        InviteeUser = team.AdminUser,
                        IsAdmin = true
                    });
                }
                else
                {
                    dtls.AcceptedUsers.Add(new TeamMember()
                    {
                        InviteeEmail = team.AdminUser.Email,
                        InviteeStatus = InviteStatus.Accepted.ToString(),
                        InviteeUserId = team.AdminUser.UserId,
                        InviteeUser = team.AdminUser,
                        IsAdmin = true
                    });
                }
            }
            return dtls;
        }

        public TeamMemberResponse CreateTeamMembereInvite(TeamMember member)
        {
            Initialize();
            var user = userLogic.GetUserByEmail(member.InviteeEmail);
            TeamMemberResponse teamMemResObj = new TeamMemberResponse();
            string password = System.Configuration.ConfigurationManager.AppSettings.Get("TeamMemberDefaultPassword");
            if (user == null)
            {
                Registration reg = new Registration()
                {
                    Email = member.InviteeEmail,
                    Password = password
                };
                var status = userLogic.CreateUser(reg, "TeamMember");
                if (status)
                    user = userLogic.GetUserByEmail(member.InviteeEmail);
                else
                {
                    ErrorMessage = userLogic.ErrorMessage;
                    return null;
                }
            }

            member.InviteeUserId = user.UserId;
            var teamMemObj = logic.CreateInvite(member);
            if (teamMemObj != null)
            {
                teamMemResObj.UserId = user.UserId;
                teamMemResObj.UserName = user.UserName;
                teamMemResObj.Password = password;
                teamMemResObj.TeamMemberId = teamMemObj.Id;
                return teamMemResObj;
            }
            else
                ErrorMessage = logic.ErrorMessage;

            return null;

        }

        public bool AddTeamMembers(List<TeamMember> teamMemberList)
        {
            Initialize();
            User user = null;
            foreach (var mem in teamMemberList)
            {
                if (user == null || user.UserId != mem.InviteeUserId)
                    user = userLogic.GetUserById(mem.InviteeUserId);
                mem.InviteeEmail = user.Email;
                mem.InvitationDate = DateTime.Now.Date;
                logic.CreateInvite(mem);
            }
            return true;
        }

        public bool RemoveTeamMembers(List<TeamMember> teamMemberList)
        {
            bool status = true;
            foreach (var mem in teamMemberList)
            {
                status &= logic.DeleteTeamMember(mem);
                var adminId = teamLogic.GetTeamById(mem.TeamId).AdminId;
                var team = teamLogic.GetTeamsByAdmin(adminId).FirstOrDefault(t => t.IsDefaultTeam);
                var memberlist = logic.GetTeamMemberDetailsByUserId(mem.InviteeUserId);
                if (memberlist.Count == 0)
                    logic.CreateInvite(new TeamMember() { InvitationDate = DateTime.Now.Date, TeamId = team.Id, InviteeEmail = mem.InviteeEmail, InviteeUserId = mem.InviteeUserId, InviteeStatus = License.Logic.Common.InviteStatus.Accepted.ToString() });
            }
            return status;
        }

        /// <summary>
        /// It will returns the list of products based on selected team.
        /// </summary>
        /// <param name="teamId">selected team id.</param>
        /// <returns>List of products</returns>
        public List<Product> GetTeamLicenseProductByTeamId(int teamId)
        {
            SubscriptionBO subscriptionBO = new SubscriptionBO();
            List<Product> productList = new List<Product>();
            var productIdList = GetProductByTeamId(teamId);
            for (int index = 0; index < productIdList.Count; index++)
            {
                var product = subscriptionBO.GetProductFromJsonFile(productIdList[index]);
                if (product != null)
                {
                    productList.Add(product);
                }
            }
            return productList;
        }

        /// <summary>
        /// Deleting listed products from selected team.
        /// </summary>
        /// <param name="productIdToDelete">product id list for delete from team selected team</param>
        /// <param name="teamId">selected team team id</param>
        /// <returns></returns>
        public bool DeleteTeamLicense(DeleteTeamDetails teamObj)
        {
            List<int> collectionOfprodIdToDelete = new List<int>();//It contains same product id multiple times.

            List<Core.Model.LicenseData> licenseDataList = new List<Core.Model.LicenseData>();
            try
            {

                var teammembersList = logic.GetTeamMembers(teamObj.TeamId);
                var userList = teammembersList.Where(tm => tm.InviteeUser.IsActive == true).ToList();
                var proceedDelete = true;
                if (userList != null && userList.Count > 0)
                {
                    ErrorMessage = "Won't be able to revoke Licence until team Members log out";
                    proceedDelete = false;
                }
                if (proceedDelete)
                {
                    userLicLogic.RevokeTeamLicenseFromUser(teamObj.LogInUserId,teamObj.TeamId);
                    var teamLic = teamLicenseLogic.GetTeamLicense(teamObj.TeamId);
                    foreach (var pro in teamObj.productIdList)
                    {
                        var teamLicByPro = teamLic.Where(tl => tl.ProductId == pro).ToList();
                        if (teamLicByPro != null && teamLicByPro.Count > 0)
                        {
                            foreach (var teamLicense in teamLicByPro)
                            {
                                teamLicenseLogic.RemoveLicenseByLicenseId(teamLicense.LicenseId);
                                licLogic.UpdateLicenseStatus(teamLicense.LicenseId, false);
                            }
                        }
                    }
                }
                return proceedDelete;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                return false;
            }

        }

        public List<int> GetProductByTeamId(int teamId)
        {
            List<int> productIdList = new List<int>();
            var teamLicenseIdList = teamLicenseLogic.GetTeamLicense(teamId).Select(tl => tl.LicenseId).ToList();
            if (teamLicenseIdList.Count > 0)
            {
                var licenseData = teamLicenseLogic.GetLicenseData();
                //Get distinct Product Id List, by license id 
                foreach (var liceId in teamLicenseIdList)
                {
                    int licenseProductId = licenseData.Where(x => x.Id == liceId).Select(p => p.ProductId).FirstOrDefault();
                    //check whether the product id already exist in list
                    if (productIdList.Count == 0 || !productIdList.Contains(licenseProductId))
                    {
                        productIdList.Add(licenseProductId);
                    }
                }
            }
            return productIdList;
        }

        public List<int> GetTeamProductByUserId(string userId, bool isAdmin)
        {
            List<Team> teamList = null;
            if (isAdmin)
                teamList = teamLogic.GetTeamsByAdmin(userId);
            else
                teamList = teamLogic.GetTeamsByUser(userId);
            List<int> productIds = new List<int>();
            foreach (var team in teamList)
            {
                var proIds = GetProductByTeamId(team.Id);
                productIds.AddRange(proIds);
            }

            return productIds.Distinct().ToList();
        }

        public TeamConcurrentUserResponse UpdateConcurrentUsers(Team team)
        {
            TeamConcurrentUserResponse concurentUserResponse = new TeamConcurrentUserResponse();
            concurentUserResponse.TeamId = team.Id;

            var dbTeamObj = teamLogic.GetTeamById(team.Id);
            var teamLicList = teamLicenseLogic.GetTeamLicense(team.Id);
            if (dbTeamObj.ConcurrentUserCount == 0) // specifying concurrent user  for first time 
                teamLogic.UpdateConcurrentUser(team);
            //If dbConcurrent user is greater than the changed valuee
            else if (dbTeamObj.ConcurrentUserCount > team.ConcurrentUserCount)
            {
                var proList = teamLicList.Select(t => t.ProductId).Distinct();
                foreach (var pro in proList)
                {
                    var licList = teamLicList.Where(t => t.ProductId == pro).ToList();
                    for (int k = team.ConcurrentUserCount; k < licList.Count; k++)
                    {
                        var deleteLic = licList[k];
                        if (deleteLic.IsMapped)
                            userLicLogic.RevokeTeamLicenseFromUser(deleteLic.Id);
                        licLogic.UpdateLicenseStatus(deleteLic.LicenseId, false);
                        teamLicenseLogic.RemoveLicenseByLicenseId(deleteLic.LicenseId);
                    }
                }
                teamLogic.UpdateConcurrentUser(team);
            }
            //If dbConcurrent user is lesser than the changed value
            else if (dbTeamObj.ConcurrentUserCount < team.ConcurrentUserCount)
            {
                var requiredLicense = team.ConcurrentUserCount - dbTeamObj.ConcurrentUserCount;
                var proIds = teamLicList.Select(l => l.ProductId).Distinct();
                bool isLicenseAvailable = true;
                foreach (var pro in proIds)
                {
                    var licAvailableCount = licLogic.GetAvailableLicenseCountByProduct(pro);
                    isLicenseAvailable &= licAvailableCount >= requiredLicense;
                }
                if (!isLicenseAvailable)
                {
                    concurentUserResponse.ErrorMessage = "Not much license Exist";
                    concurentUserResponse.UserUpdateStatus = false;
                    concurentUserResponse.OldUserCount = dbTeamObj.ConcurrentUserCount;
                    return concurentUserResponse;
                }
                else
                {
                    TeamLicenseDataMapping licMappingObj = new TeamLicenseDataMapping();
                    licMappingObj.ConcurrentUserCount = requiredLicense;
                    licMappingObj.ProductIdList = proIds.ToList();
                    licMappingObj.TeamList = new List<string>
                    {
                        team.Id.ToString()
                    };
                    teamLicenseLogic.CreateMultipleTeamLicense(licMappingObj);
                }

                teamLogic.UpdateConcurrentUser(team);
            }

            concurentUserResponse.UserUpdateStatus = true;
            return concurentUserResponse;
        }

        public bool DeleteTeam(int teamId)
        {
            try
            {
                var licList = teamLicenseLogic.GetTeamLicense(teamId);
                if (licList != null && licList.Count > 0)
                {
                    foreach (var lic in licList)
                    {
                        if (lic.IsMapped)
                            userLicLogic.RevokeTeamLicenseFromUser(lic.Id);
                        teamLicenseLogic.RemoveLicenseByLicenseId(lic.LicenseId);
                        licLogic.UpdateLicenseStatus(lic.LicenseId, false);
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            bool status = false;
            if (String.IsNullOrEmpty(ErrorMessage))
                status = teamLogic.DeleteTeam(teamId);
            if (!status)
                ErrorMessage = ErrorMessage + " " + teamLogic.ErrorMessage;
            return status;
        }
    }
}