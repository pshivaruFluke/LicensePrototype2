﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LicenseServer.DataModel;
using Microsoft.AspNet.Identity;
using LicenseServer.Core.DbContext;
using LicenseServer.Core.Manager;
using Microsoft.AspNet.Identity.EntityFramework;
using LicenseServer.Core.Model;

namespace LicenseServer.Logic
{
    public class UserLogic : BaseLogic
    {
        public UserLogic()
        {
        }

        public ICollection<User> GetUsers()
        {
            OrganizationLogic orgLogic = new OrganizationLogic();
            List<User> usersList = new List<User>();
            UserSubscriptionLogic subscriptionLogic = new UserSubscriptionLogic();
            var users = UserManager.Users.ToList();
            foreach (var u in users)
            {
                User temp = AutoMapper.Mapper.Map<LicenseServer.Core.Model.Appuser, User>(u);
                temp.Organization = orgLogic.GetOrganizationById(temp.OrganizationId);
                temp.SubscriptionList = subscriptionLogic.GetUserSubscription(temp.UserId).Select(s => s.Subtype).ToList();
                usersList.Add(temp);
            }
            return usersList;
        }

        public User CreateUser(Registration u, string roleName = "BackendAdmin")
        {
            UserTokenLogic tokenLogic = new UserTokenLogic();
            var status = tokenLogic.VerifyUserToken(new LicenseServer.DataModel.UserToken() { Email = u.Email, Token = u.Token });
            if(!status)
            {
                ErrorMessage = "Invalid User Token";
                return null;
            }
            User ur = new User();
            ur.FirstName = u.FirstName;
            ur.LastName = u.LastName;
            ur.Email = u.Email;
            ur.PhoneNumber = u.PhoneNumber;
            ur.UserName = u.Email;
            var teamName = u.OrganizationName;
            OrganizationLogic logic = new OrganizationLogic();
            DataModel.Organization t = logic.GetOrganizationByName(teamName);
            if (t == null)
                t = logic.CreateOrganization(new DataModel.Organization() { Name = teamName });
            ur.OrganizationId = t.Id;
            LicenseServer.Core.Model.Appuser user = AutoMapper.Mapper.Map<User, LicenseServer.Core.Model.Appuser>(ur);
            IdentityResult result;
            try
            {
                RoleLogic rolelogic = new RoleLogic();
                rolelogic.RoleManager = RoleManager;
                result = UserManager.Create(user, u.Password);
                var roleObj = rolelogic.GetRoleByName(roleName);
                if (roleObj == null)
                    result = rolelogic.CreateRole(new Role() { Name = roleName });
                UserManager.AddToRole(user.Id, roleName);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return AutoMapper.Mapper.Map<User>(user);
        }

        public User GetUserById(string id)
        {
            var u = UserManager.FindById(id);
            var user = AutoMapper.Mapper.Map<LicenseServer.Core.Model.Appuser, User>(u);
            IList<string> roles = UserManager.GetRoles(user.UserId);
            user.Roles = roles;
            return user;
        }

        public bool UpdateUser(string id, User user)
        {
            var appuser = UserManager.FindById(id);
            appuser.FirstName = user.FirstName;
            appuser.LastName = user.LastName;
            appuser.PhoneNumber = user.PhoneNumber;
            var result = UserManager.Update(appuser);
            return result.Succeeded;
        }

        public bool DeleteUser(string id)
        {
            var user = UserManager.FindById(id);
            var result = UserManager.Delete(user);
            return result.Succeeded;
        }

        public bool ResetPassword(string userId, string token, string password)
        {
            var result = UserManager.ResetPassword(userId, token, password);
            return result.Succeeded;
        }

        public bool ChangePassword(string userId, string oldPassword, string newPassword)
        {
            var result = UserManager.ChangePassword(userId, oldPassword, newPassword);
            return result.Succeeded;
        }

        public User GetUserByEmail(string email)
        {
            var usr = UserManager.FindByEmail<Core.Model.Appuser, string>(email);
            User user = AutoMapper.Mapper.Map<User>(usr);
            IList<string> roles = UserManager.GetRoles(user.UserId);
            user.Roles = roles;
            return user;
        }

        public User ForgotPassword(string email)
        {
            Core.Model.Appuser user = UserManager.FindByEmail(email);
            return AutoMapper.Mapper.Map<Core.Model.Appuser, User>(user);
        }

        public User AuthenticateUser(string userName, string password)
        {
            Core.Model.Appuser user = UserManager.Find(userName, password);
            if (user != null)
            {
                user.IsActive = true;
                UserManager.Update(user);
                User userObj = AutoMapper.Mapper.Map<Core.Model.Appuser, User>(user);
                IList<string> roles = UserManager.GetRoles(user.Id);
                userObj.Roles = roles;
                return userObj;
            }
            return null;

        }


        public async Task<bool> ResetPassword(string userId, string newPassword)
        {
            var token = UserManager.GeneratePasswordResetToken<Appuser, string>(userId);
            var result = UserManager.ResetPassword(userId, token, newPassword);
            return result.Succeeded;
        }

        public bool ValidateUser(string userName, string password)
        {
            var user = UserManager.Find(userName, password);
            user.IsActive = true;
            UserManager.Update(user);
            return user != null;
        }

        public System.Security.Claims.ClaimsIdentity CreateClaimsIdentity(string userId, string authType)
        {
            var obj = UserManager.FindById(userId);
            return UserManager.CreateIdentity(obj, authType);

        }

        public void UpdateLogInStatus(string userid, bool status)
        {
            var user = UserManager.FindById(userid);
            user.IsActive = status;
            UserManager.Update(user);
        }


    }
}
