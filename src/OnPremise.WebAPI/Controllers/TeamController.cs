﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using License.Logic.BusinessLogic;
using License.Logic.DataLogic;
using License.DataModel;

namespace OnPremise.WebAPI.Controllers
{
    [Authorize]
    [RoutePrefix("api/Team")]
    public class TeamController : BaseController
    {
        TeamBO teamBoLogic = null;
        TeamLogic logic = null;
        public TeamController()
        {
            teamBoLogic = new TeamBO();
            logic = new TeamLogic();
        }

        //[HttpGet]
        //[Route("All")]
        //public IHttpActionResult GetAll()
        //{
        //    var lstTeams = logic.GetTeam();
        //    return Ok(lstTeams);
        //}
        
        /// <summary>
        /// Get Method. To get the Team along with Team Member details based on the TeamId.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetById/{id}")]
        public HttpResponseMessage GetTeamById(int id)
        {
            TeamDetails dtls = teamBoLogic.GetteamDetails(id);
            if (dtls != null)
                return Request.CreateResponse(HttpStatusCode.OK, dtls);
            else
                return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, teamBoLogic.ErrorMessage);
        }

        /// <summary>
        /// Get Method.To get Team List  based on the admin Id
        /// </summary>
        /// <param name="adminId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetTeamsByAdminId/{adminId}")]
        public IHttpActionResult GetTeamByAdminId(string adminId)
        {
            var dtls = logic.GetTeamsByAdmin(adminId);
            return Ok(dtls);
        }

        /// <summary>
        /// GET Method. To get the Team List based on UserId to which user belongs
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetTeamsByUserId/{userId}")]
        public IHttpActionResult GetTeamByUserId(string userId)
        {
           
            var dtls = logic.GetTeamsByUser(userId);
            return Ok(dtls);
        }

        /// <summary>
        /// POST Method. To Create Team
        /// </summary>
        /// <param name="team"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Create")]
        public HttpResponseMessage CreateTeam(Team team)
        {
            logic.UserManager = UserManager;
            team = logic.CreateTeam(team);
            if (team != null)
                return Request.CreateResponse(HttpStatusCode.OK, team);
            else
                return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, logic.ErrorMessage);
        }

        /// <summary>
        /// PUT Method. To Update the Team Details
        /// </summary>
        /// <param name="id"></param>
        /// <param name="team"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("Update/{id}")]
        public HttpResponseMessage UpdateTeam(int id, Team team)
        {
            team = logic.UpdateTeam(id, team);
            if (team != null)
                return Request.CreateResponse(HttpStatusCode.OK, team);
            else
                return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, logic.ErrorMessage);
        }

        /// <summary>
        /// DELETE Method. To delete Team based on the TeamId. The Tea will be deleted but the Team Member will be
        /// updated to the default team .
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("Delete/{id}")]
        public HttpResponseMessage DeleteTeam(int id)
        {
            var status = logic.DeleteTeam(id);
            if (status)
                return Request.CreateResponse(HttpStatusCode.OK, "Success");
            else if (String.IsNullOrEmpty(logic.ErrorMessage))
                return Request.CreateResponse(HttpStatusCode.NotFound, "Data NOt found");
            else
                return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, logic.ErrorMessage);
        }
    }
}