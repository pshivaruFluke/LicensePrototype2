﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using License;

namespace License.Logic.DataLogic
{
    /// <summary>
    /// History 
    /// 
    ///     Created By: 
    ///     Created Date:
    ///     Purpose : 1.Functions to perform the CRUD functionality on Team table
    ///               2. get the Team Based on the User  and Admin
    /// </summary>
    public class TeamLogic : BaseLogic
    {
        public TeamLogic()
        {

        }

        // Gets All Teams
        public List<DataModel.Team> GetTeam()
        {
            var objList = Work.TeamRepository.GetData();
            var teamList = objList.Select(t => AutoMapper.Mapper.Map<DataModel.Team>(t)).ToList();
            return teamList;
        }

        // Get Team List based on the Admin Id
        public List<DataModel.Team> GetTeamsByAdmin(string adminId)
        {
            var objList = Work.TeamRepository.GetData(t => t.AdminId == adminId).ToList();
            var teamList = objList.Select(t => AutoMapper.Mapper.Map<DataModel.Team>(t)).ToList();
            return teamList;
        }

        // GGet Team List Based on the User Id
        public List<DataModel.Team> GetTeamsByUser(string userId)
        {
            var teamIdList = Work.TeamMemberRepository.GetData(tm => tm.InviteeUserId == userId).ToList().Select(t => t.TeamId).ToList();
            var objList = Work.TeamRepository.GetData(t => teamIdList.Contains(t.Id)).ToList();
            var teamList = objList.Select(t => AutoMapper.Mapper.Map<DataModel.Team>(t)).ToList();
            return teamList;
        }

        // Get  Team by Id
        public DataModel.Team GetTeamById(int id)
        {
            TeamMemberLogic memLogic = new TeamMemberLogic();
            var obj = Work.TeamRepository.GetById(id);
            DataModel.Team teamObj = AutoMapper.Mapper.Map<DataModel.Team>(obj);
            teamObj.TeamMembers = memLogic.GetTeamMembers(id);
            return teamObj;
        }

        // Create New Team in DB
        public DataModel.Team CreateTeam(DataModel.Team model)
        {
            var obj = AutoMapper.Mapper.Map<Core.Model.Team>(model);
            var objTemp = Work.TeamRepository.GetData(t => t.Name.Trim() == obj.Name.Trim() && t.AdminId == model.AdminId).FirstOrDefault();
            if (objTemp != null)
            {
                ErrorMessage = "Team Name already Exist";
                return null;
            }
            obj = Work.TeamRepository.Create(obj);
            Work.TeamRepository.Save();
            if (obj.Id > 0)
            {
                model = AutoMapper.Mapper.Map<DataModel.Team>(obj);
                UserLogic userLogic = new UserLogic();
                userLogic.UserManager = UserManager;
                model.AdminUser = userLogic.GetUserById(model.AdminId);
            }
            return model;
        }

        // Update Team details  based on the team Id
        public DataModel.Team UpdateTeam(int id, DataModel.Team model)
        {
            var obj = Work.TeamRepository.GetById(id);
            // The temp object to make sure the change in the team Name   is already exist in the Db excluduing the current team ID. If the record is already exist with the 
            // new Team then null will be return .
            var objTemp = Work.TeamRepository.GetData(t => t.Name.Trim() == model.Name.Trim() && t.AdminId == obj.AdminId && t.Id != id).FirstOrDefault();
            if (objTemp != null && objTemp.Id != obj.Id)
            {
                ErrorMessage = "Team Name already Exist";
                return null;
            }
            obj.Name = model.Name;
            obj = Work.TeamRepository.Update(obj);
            Work.TeamRepository.Save();
            return AutoMapper.Mapper.Map<DataModel.Team>(obj);
        }

        // Delete Team based on the Team ID.  If any users exist in the team which is being deleted then the User from the existing team will be moved under default team 
        // and Team License will be deleted.
        public bool DeleteTeam(int id)
        {
            var tempObj = Work.TeamRepository.GetById(id);
            var data = Work.TeamMemberRepository.GetData(tm => tm.TeamId == id).ToList();
            var team = Work.TeamRepository.GetData(t => t.IsDefaultTeam == true && t.AdminId == tempObj.AdminId).FirstOrDefault();
            // If users exist in the team which is being deleted then moving from the current Team to default team.
            if (data.Count > 0)
            {
                int i = 0;
                foreach (var mem in data)
                {
                    int existingcount = Work.TeamMemberRepository.GetData(t => t.InviteeUserId == mem.InviteeUserId && t.TeamId == team.Id).Count();
                    if (existingcount == 0)
                    {
                        mem.TeamId = team.Id;
                        Work.TeamMemberRepository.Update(mem);
                    }
                    i++;
                }
                if (i > 0)
                    Work.TeamMemberRepository.Save();
            }

            // Deleting the team license
            TeamLicenseLogic teamLicLogic = new TeamLicenseLogic();
            teamLicLogic.RemoveLicenseByTeam(id);

            // Deleting the team based on the ID
            var deletestatus = Work.TeamRepository.Delete(id);
            Work.TeamRepository.Save();
            return deletestatus;
        }

        // Used to Authenticate the Concurrent user log in  once the user authentication is success
        public bool AllowTeamMemberLogin(int teamId, string userId)
        {
            var team = Work.TeamRepository.GetById(teamId);
            if (team.ConcurrentUserCount == 0)
                return true;
            var loggedInUserCount = team.TeamMembers.Where(tm => tm.InviteeUserId != userId && tm.InviteeUser.IsActive == true).Count();
            if (team.AdminId != userId)
            {
                var user = UserManager.FindByIdAsync(team.AdminId).Result;
                if (user.IsActive)
                    loggedInUserCount += 1;
            }
            if (loggedInUserCount < team.ConcurrentUserCount)
                return true;
            return false;

        }

        // Used to update the concurrent user count for the team.
        public DataModel.Team UpdateConcurrentUser(DataModel.Team teamObj)
        {
            var team = Work.TeamRepository.GetById(teamObj.Id);
            team.ConcurrentUserCount = teamObj.ConcurrentUserCount;
            Work.TeamRepository.Update(team);
            Work.TeamRepository.Save();
            return AutoMapper.Mapper.Map<DataModel.Team>(team);
        }
    }
}
