﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using LicenseServer.Logic;
using LicenseServer.DataModel;

namespace Centralized.WebAPI.Controllers
{
    [Authorize]
    [RoutePrefix("api/subscription")]
    public class SubscriptionController : BaseController
    {

        private SubscriptionTypeLogic logic = null;
        public SubscriptionController()
        {
            logic = new SubscriptionTypeLogic();
        }

        /// <summary>
        /// GET Method. To get all the Subscriptions
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("All")]
        public IHttpActionResult GetAllSubscription()
        {
            var subList = logic.GetSubscriptionType();
            return Ok(subList);
        }

        /// <summary>
        /// Get List of Subscriptions Both Default and Custom Subscriptions
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("All/{userId}")]
        public IHttpActionResult GetAllSubscription(string userId)
        {
            var subList = logic.GetSubscriptionType(userId);
            return Ok(subList);
        }

        /// <summary>
        /// POST method. Creates the Subscriptioon
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("CreateSubscription")]
        public HttpResponseMessage CreateSubscription(SubscriptionType type)
        {
            var subscriptionType = logic.CreateSubscriptionWithProduct(type);
            if (subscriptionType != null)
                return Request.CreateResponse(HttpStatusCode.Created, subscriptionType);
            else
                return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, logic.ErrorMessage);
        }
    }
}
