﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using License.MetCalWeb;
using License.MetCalWeb.Common;
using License.MetCalWeb.Models;
using System.Collections;
using System.Net.Http;
using Newtonsoft.Json;

namespace License.MetCalWeb.Controllers
{
    [Authorize]
    [SessionExpire]
    public class TeamController : BaseController
    {
        public TeamController()
        {

        }


        public ActionResult Index()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            items.Add(new SelectListItem() { Text = "1", Value = "1" });
            items.Add(new SelectListItem() { Text = "2", Value = "2" });
            items.Add(new SelectListItem() { Text = "3", Value = "3" });
            items.Add(new SelectListItem() { Text = "4", Value = "4" });
            items.Add(new SelectListItem() { Text = "5", Value = "5" });
            items.Add(new SelectListItem() { Text = "6", Value = "6" });
            items.Add(new SelectListItem() { Text = "7", Value = "7" });
            items.Add(new SelectListItem() { Text = "8", Value = "8" });
            items.Add(new SelectListItem() { Text = "9", Value = "9" });
            items.Add(new SelectListItem() { Text = "10", Value = "10" });
            ViewData["UserCountList"] = items;
            string userId = string.Empty;
            if (!LicenseSessionState.Instance.IsSuperAdmin)
                userId = LicenseSessionState.Instance.User.UserId;
            var teamList = OnPremiseSubscriptionLogic.GetTeamList(userId);
            LicenseSessionState.Instance.TeamList = teamList;
            return View(LicenseSessionState.Instance.TeamList);
        }

        public ActionResult CreateTeam()
        {
            return View("Create");
        }

        [HttpPost]
        public ActionResult CreateTeam(Team model)
        {
            if (ModelState.IsValid)
            {
                model.AdminId = LicenseSessionState.Instance.User.UserId;
                HttpClient client = WebApiServiceLogic.CreateClient(ServiceType.OnPremiseWebApi);
                var response = client.PostAsJsonAsync("api/Team/Create", model).Result;
                if (response.IsSuccessStatusCode)
                {
                    var jsonData = response.Content.ReadAsStringAsync().Result;
                    var data = JsonConvert.DeserializeObject<Team>(jsonData);
                    LicenseSessionState.Instance.TeamList.Add(data);
                    return RedirectToAction("Index");
                }
                else
                {
                    var jsonData = response.Content.ReadAsStringAsync().Result;
                    var obj = JsonConvert.DeserializeObject<ResponseFailure>(jsonData);
                    ModelState.AddModelError("", response.ReasonPhrase + " - " + obj.Message);

                }
            }
            var _message = string.Join(Environment.NewLine, ModelState.Values
                                        .SelectMany(x => x.Errors)
                                        .Select(x => x.ErrorMessage));
            return Json(new { success = false, message = _message });
        }

        public ActionResult TeamMapLicense(int teamId)
        {
            TeamMappingDetails teamMappingDetails = new TeamMappingDetails();
            var team = LicenseSessionState.Instance.TeamList.ToList().Where(t => t.Id == teamId).FirstOrDefault();
            TempData["Teamid"] = teamId;
            teamMappingDetails.ConcurrentUserCount = team.ConcurrentUserCount;
            teamMappingDetails.SelectedTeamName = team.Name;
            teamMappingDetails.ProductList = OnPremiseSubscriptionLogic.GetProductsFromSubscription().ToList();
            return View(teamMappingDetails);

        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TeamMapLicense(TeamMappingDetails teamMappingDetails, params string[] selectedSubscription)
        {
            var responseData = UpdateLicense(teamMappingDetails.ConcurrentUserCount, selectedSubscription);
            if (!String.IsNullOrEmpty(responseData))
            {
                ModelState.AddModelError("", responseData);
                return View("TeamContainer", "TeamManagement");
            }
            return RedirectToAction("TeamContainer", "TeamManagement");
        }

        public string UpdateLicense(int concurrentUserCount, string[] selectedSubscription)
        {

            List<string> teamIdList = new List<string>();
            teamIdList.Add(TempData["Teamid"].ToString());
            List<int> listOfProId = ExtractLicenseData(selectedSubscription);
            TeamLicenseDataMapping mapping = new TeamLicenseDataMapping() { ConcurrentUserCount = concurrentUserCount, ProductIdList = listOfProId, TeamList = teamIdList };
            HttpClient client = WebApiServiceLogic.CreateClient(ServiceType.OnPremiseWebApi);
            var response = client.PostAsJsonAsync("api/License/CreateTeamLicence", mapping).Result;
            if (!response.IsSuccessStatusCode)
            {
                var jsonData = response.Content.ReadAsStringAsync().Result;
                var obj = JsonConvert.DeserializeObject<ResponseFailure>(jsonData);
                return response.ReasonPhrase + " - " + obj.Message;
            }
            else
            {
                if (teamIdList.Contains(LicenseSessionState.Instance.SelectedTeam.Id.ToString()))//doubt
                {
                    var subscriptionDetails = OnPremiseSubscriptionLogic.GetUserLicenseForUser();
                    LicenseSessionState.Instance.UserSubscriptionList = subscriptionDetails;
                }
            }
            return String.Empty;
        }

        private static List<int> ExtractLicenseData(string[] SelectedSubscription)
        {
            List<int> proIdList = new List<int>();
            foreach (var data in SelectedSubscription)
            {
                var prodId = data.Split(new char[] { ':' })[1];
                proIdList.Add(Convert.ToInt32(prodId));
            }
            return proIdList;
        }

        public IList<SubscriptionDetails> GetLicenseListBySubscription(string userId)
        {
            TempData["UserId"] = userId;
            //Logic to get the Subscription details Who are Team Member and Role is assigned as admin by the Super admin
            string adminUserId = string.Empty;
            IList<SubscriptionDetails> licenseMapModelList = OnPremiseSubscriptionLogic.GetSubscriptionForLicenseMap(userId, adminUserId);
            return licenseMapModelList;
        }
        public ActionResult EditTeam(int id)
        {
            var team = LicenseSessionState.Instance.TeamList.FirstOrDefault(t => t.Id == id);
            return View("Edit", team);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditTeam(int id, Team model)
        {
            HttpClient client = WebApiServiceLogic.CreateClient(ServiceType.OnPremiseWebApi);
            var response = client.PutAsJsonAsync("api/team/Update/" + id, model).Result;
            if (response.IsSuccessStatusCode)
            {
                var data = response.Content.ReadAsStringAsync().Result;
                var teamObj = JsonConvert.DeserializeObject<Team>(data);
                LicenseSessionState.Instance.TeamList.RemoveAll(f => f.Id == id);
                LicenseSessionState.Instance.TeamList.Add(teamObj);
                return RedirectToAction("Index");
            }
            else
            {
                var jsonData = response.Content.ReadAsStringAsync().Result;
                var obj = JsonConvert.DeserializeObject<ResponseFailure>(jsonData);
                ModelState.AddModelError("", response.ReasonPhrase + " - " + obj.Message);
            }
            var _message = string.Join(Environment.NewLine, ModelState.Values
                                      .SelectMany(x => x.Errors)
                                      .Select(x => x.ErrorMessage));
            return Json(new { success = false, message = _message });
        }

        [HttpGet]
        public ActionResult DeleteTeam(int id)
        {
            HttpClient client = WebApiServiceLogic.CreateClient(ServiceType.OnPremiseWebApi);
            var response = client.DeleteAsync("api/team/Delete/" + id).Result;
            if (response.IsSuccessStatusCode)
            {
                LicenseSessionState.Instance.TeamList.RemoveAll(t => t.Id == id);
                if (LicenseSessionState.Instance.SelectedTeam != null && LicenseSessionState.Instance.SelectedTeam.Id == id)
                    LicenseSessionState.Instance.SelectedTeam = LicenseSessionState.Instance.TeamList.FirstOrDefault(s => s.IsDefaultTeam = true);
                return RedirectToAction("Index");
            }
            else
            {
                var jsonData = response.Content.ReadAsStringAsync().Result;
                var obj = JsonConvert.DeserializeObject<ResponseFailure>(jsonData);
                ModelState.AddModelError("", response.ReasonPhrase + " - " + obj.Message);
            }
            return View();
        }

        public ActionResult RevokeTeamLicense(int teamId)
        {
            TeamDetails teamDetails = new TeamDetails();
            teamDetails.Team = LicenseSessionState.Instance.TeamList.ToList().Where(t => t.Id == teamId).FirstOrDefault();
            teamDetails.ProductList = OnPremiseSubscriptionLogic.GetTeamLicenseDetails(teamId);
            return View(teamDetails);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RevokeTeamLicense(int teamId, params string[] SelectedSubscription)
        {
            DeleteTeamDetails details = new DeleteTeamDetails();
            details.LogInUserId = LicenseSessionState.Instance.User.UserId;
            details.TeamId = teamId;
            details.productIdList = ExtractLicenseData(SelectedSubscription);
            HttpClient client = WebApiServiceLogic.CreateClient(ServiceType.OnPremiseWebApi);
            var response = client.PostAsJsonAsync("api/License/Delete", details).Result;
            if (!response.IsSuccessStatusCode)
            {
                var jsondata = response.Content.ReadAsStringAsync().Result;
                var responsefailure = JsonConvert.DeserializeObject<ResponseFailure>(jsondata);
                return Json(new { success = false, message = responsefailure.Message });
            }
            return Json(new { success = true, message = "" });

        }

        public ActionResult UpdateConcurentUser(int teamId, int noOfUser)
        {
            HttpClient client = WebApiServiceLogic.CreateClient(ServiceType.OnPremiseWebApi);
            Team team = new Team() { Id = teamId, ConcurrentUserCount = noOfUser };
            var response = client.PostAsJsonAsync("api/team/UpdateConcurentUser", team).Result;
            if (response.IsSuccessStatusCode)
            {
                var jsonData = response.Content.ReadAsStringAsync().Result;
                var responsedata = JsonConvert.DeserializeObject<TeamConcurrentUserResponse>(jsonData);
                if (responsedata.UserUpdateStatus)
                {
                    team = LicenseSessionState.Instance.TeamList.FirstOrDefault(t => t.Id == teamId);
                    if (team != null)
                        team.ConcurrentUserCount = noOfUser;
                    return Json(new { success = true, message = "", OldUserCount = team.ConcurrentUserCount }, JsonRequestBehavior.AllowGet);
                }
                else
                    return Json(new { success = false, message = responsedata.ErrorMessage, OldUserCount = responsedata.OldUserCount }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var jsonData = response.Content.ReadAsStringAsync().Result;
                var failureData = JsonConvert.DeserializeObject<ResponseFailure>(jsonData);
                team = LicenseSessionState.Instance.TeamList.FirstOrDefault(t => t.Id == teamId);

                return Json(new { success = false, message = failureData.Message, OldUserCount = team != null ? team.ConcurrentUserCount : 0 }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}