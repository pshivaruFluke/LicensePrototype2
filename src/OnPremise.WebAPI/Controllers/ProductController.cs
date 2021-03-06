﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using License.DataModel;
using License.Logic.DataLogic;

namespace OnPremise.WebAPI.Controllers
{
    [RoutePrefix("api/Product")]
    [Authorize]
    public class ProductController : BaseController
    {
        ProductLogic productLogic = null;

        public ProductController()
        {
            productLogic = new ProductLogic();
        }

        [HttpGet]
        [Route("GetProductsByAdminId/{adminUserId}")]
        public IHttpActionResult GetProductsByAdmin(string adminUserId)
        {
            var productList = productLogic.GetProductbyAdmin(adminUserId);
            return Ok(productList);
        }

        [HttpPost]
        [Route("UpdateProducts")]
        public HttpResponseMessage UpdateProducts(List<Product> productList)
        {
            productLogic.UpdateProducts(productList);
            return Request.CreateResponse(HttpStatusCode.OK, "success");
        }
        
    }
}
