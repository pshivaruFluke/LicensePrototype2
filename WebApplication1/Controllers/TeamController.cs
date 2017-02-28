﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using License.Core.Manager;
using License.Core.Model;
using License.Logic.Common;
using License.Logic.ServiceLogic;
using License.MetCalWeb;
using License.MetCalWeb.Common;
using License.MetCalWeb.Models;
using License.Model.Model;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using TeamMembers = License.Model.Model.TeamMembers;

namespace WebApplication1.Controllers
{
    [Authorize]
    public class TeamController : Controller
    {
        private TeamMemberLogic logic = null;
        private UserLogic userLogic = null;


        public TeamController()
        {
            logic = new TeamMemberLogic();
            userLogic = new UserLogic();
        }
        // GET: Team
        public ActionResult TeamContainer()
        {
            TeamModel model = LoadTeamMember();
            return View(model);
        }

        public ActionResult TeamMembers()
        {
            return View();
        }

        private TeamModel LoadTeamMember()
        {
            License.Model.Model.UserInviteList inviteList = new UserInviteList();
            string adminId = string.Empty;
            TeamModel model = null;

            if (logic.UserManager == null)
                logic.UserManager = Request.GetOwinContext().GetUserManager<AppUserManager>();

            if (logic.RoleManager == null)
                logic.RoleManager = Request.GetOwinContext().GetUserManager<AppRoleManager>();

            if (LicenseSessionState.Instance.User.Roles.Contains("Admin"))
            {
                adminId = LicenseSessionState.Instance.User.UserId;
                LicenseSessionState.Instance.IsTeamAdmin = true;
            }
            else
                adminId = logic.GetUserAdminDetails(LicenseSessionState.Instance.User.UserId);
            if (!String.IsNullOrEmpty(adminId))
            {
                inviteList = logic.GetUserInviteDetails(adminId);
                model = new TeamModel();
                model.AdminUser = inviteList.AdminUser;
                model.AcceptedUsers = inviteList.AcceptedInvites;
                model.PendinigUsers = inviteList.PendingInvites;
            }
            if (model.AcceptedUsers.Count <= 0 || LicenseSessionState.Instance.IsTeamAdmin)
                return model;
            var obj =
                model.AcceptedUsers
                    .FirstOrDefault(t => t.InviteeUserId == LicenseSessionState.Instance.User.UserId);
            LicenseSessionState.Instance.IsTeamAdmin = obj?.IsAdmin ?? false;
            return model;
        }


        public ActionResult Subscriptions()
        {
            return View();
        }

        public ActionResult Invite()
        {

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Invite(UserInviteModel model)
        {
            bool status = false;
            IdentityResult result;
            if (ModelState.IsValid)
            {
                //if(logic.VerifyUserInvited(model.Email, LicenseSessionState.Instance.User.UserId)!= null)
                //    return Json(new { success = false, Message = "User has already been invited" });

                if (userLogic.UserManager == null)
                    userLogic.UserManager = Request.GetOwinContext().GetUserManager<AppUserManager>();
                if (userLogic.RoleManager == null)
                    userLogic.RoleManager = Request.GetOwinContext().GetUserManager<AppRoleManager>();
                if (!userLogic.GetUserByEmail(model.Email))
                {
                    model.Password = (string)System.Configuration.ConfigurationManager.AppSettings.Get("InvitePassword");
                    result = userLogic.CreateUser(model.RegistratoinModel, "TeamMember");
                    status = result.Succeeded;
                }
                else
                    status = true;

                if (status)
                {
                    AppUser user = userLogic.UserManager.FindByEmail(model.Email);
                    TeamMembers invite = new TeamMembers();
                    invite.AdminId = LicenseSessionState.Instance.User.UserId;
                    invite.InviteeUserId = user.Id;
                    invite.InvitationDate = DateTime.Now.Date;
                    invite.InviteeEmail = model.Email;
                    invite.InviteeStatus = InviteStatus.Pending.ToString();
                    var data = logic.CreateInvite(invite);
                    if (data.Id > 0)
                    {
                        string body = System.IO.File.ReadAllText(Server.MapPath("~/EmailTemplate/Invitation.htm"));
                        body = body.Replace("{{AdminEmail}}", LicenseSessionState.Instance.User.Email);
                        string encryptString =  invite.AdminId + "," + data.Id;
                        string passPhrase = System.Configuration.ConfigurationManager.AppSettings.Get("passPhrase");
                        var dataencrypted = EncryptDecrypt.EncryptString(encryptString, passPhrase);
                        string token = userLogic.UserManager.GenerateEmailConfirmationToken(user.Id);

                        string joinUrl = Url.Action("Confirm", "Account",
                            new { invite = dataencrypted, status = InviteStatus.Accepted.ToString() }, protocol: Request.Url.Scheme);
                        string declineUrl = Url.Action("Confirm", "Account",
                            new { invite = dataencrypted, status = InviteStatus.Declined.ToString() }, protocol: Request.Url.Scheme);

                        body = body.Replace("{{JoinUrl}}", joinUrl);
                        body = body.Replace("{{DeclineUrl}}", declineUrl);
                        body = body.Replace("{{UserName}}", model.Email);
                        body = body.Replace("{{Password}}", model.Password);

                        userLogic.UserManager.SendEmail(user.Id, "Invite to fluke Calibration", body);
                    }
                }
            }
            return Json(new { success = true, message = "" });
        }
    }
}