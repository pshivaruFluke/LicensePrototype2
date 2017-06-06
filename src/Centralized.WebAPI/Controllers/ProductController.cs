﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using LicenseServer.Logic;
using LicenseServer.Logic.BusinessLogic;
using LicenseServer.DataModel;

namespace Centralized.WebAPI.Controllers
{
    [Authorize]
    [RoutePrefix("api/Product")]
    public class ProductController : BaseController
    {
        ProductLogic logic = null;
        public ProductController()
        {
            logic = new ProductLogic();
        }

        /// <summary>
        /// GET Method. Gets all the Products
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("All")]
        public IHttpActionResult GetAllProducts()
        {
            var proList = logic.GetProducts();
            return Ok(proList);
        }

        /// <summary>
        /// POST Method. Create Product Data
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Create")]
        public HttpResponseMessage CreateProduct(Product model)
        {
            var pro = logic.CreateProduct(model);
            if (pro != null && pro.Id > 0)
                return Request.CreateResponse(HttpStatusCode.Created, pro);
            else
                return Request.CreateResponse(HttpStatusCode.ExpectationFailed, "Failed to create Product due to internal error");
        }

        /// <summary>
        /// Put Method. Update the Product details based on the Product ID.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("update/{id}")]
        public HttpResponseMessage UpdateProduct(int id, Product model)
        {
            var pro = logic.UpdateProduct(id, model);
            if (pro != null && pro.Id > 0)
                return Request.CreateResponse(HttpStatusCode.OK, pro);
            else
                return Request.CreateResponse(HttpStatusCode.ExpectationFailed, "Failed to Update Product due to internal error");
        }


        /// <summary>
        /// Delete Method. Delete Product based on the Product ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("Delete/{id}")]
        public HttpResponseMessage DeleteProduct(int id)
        {
            var status = logic.DeleteProduct(id);
            if (status)
                return Request.CreateResponse(HttpStatusCode.Accepted, "Deleted the Product");
            else
                return Request.CreateResponse(HttpStatusCode.ExpectationFailed, "Failed to Delete Product due to internal error");
        }

        /// <summary>
        /// Get Method: To get the Product by Product ID
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>

        [HttpGet]
        [Route("GetById/{Id}")]
        public HttpResponseMessage GetProductById(int Id)
        {
            var obj = logic.GetProductById(Id);
            if (obj != null)
                return Request.CreateResponse(HttpStatusCode.OK, obj);
            else
                return Request.CreateResponse(HttpStatusCode.ExpectationFailed, logic.ErrorMessage);
        }

        /// <summary>
        /// Get Method: To Get the CMMS Products
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetCMMSProducts")]
        public IHttpActionResult GetCMMSProducts()
        {
            var productList = logic.GetCMMSProducts();
            return Ok(productList);
        }

        /// <summary>
        /// Get Methos: Get Products by the Category Id
        /// </summary>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("ProductByCategory/{categoryId}")]
        public HttpResponseMessage GetProductByCategory(int categoryId)
        {
            var obj = logic.GetProductByCategoryId(categoryId);
            if (obj != null)
                return Request.CreateResponse(HttpStatusCode.OK, obj);
            else
                return Request.CreateResponse(HttpStatusCode.ExpectationFailed, logic.ErrorMessage);
        }

        /// <summary>
        /// Post method: to get the Product canges based on the Product Id. Input to this service is list Products with Product Id and Last Modified changes.
        /// </summary>
        /// <param name="products"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("CheckProductUpdates")]
        public IHttpActionResult GetProductUpdatesByProductId(List<Product> products)
        {
            var data = logic.GetProductUpdatesByProductId(products);
            return Ok(data);
        }

    }
}
