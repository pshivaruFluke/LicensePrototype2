﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using License.MetCalWeb.Models;
using LicenseServer.Logic;
using LicenseServer.DataModel;

namespace License.MetCalWeb.Controllers
{
    [Authorize(Roles ="Admin,SuperAdmin")]
    public class ProductController : BaseController
    {
        CartLogic cartLogic = null;
        SubscriptionTypeLogic subscriptionTypeLogic = null;
        public ProductController()
        {
            cartLogic = new CartLogic();
            subscriptionTypeLogic = new SubscriptionTypeLogic();
        }

        public ActionResult ProductCatalog()
        {
            var obj = subscriptionTypeLogic.GetSubscriptionType();
            return View(obj);
        }

        public ActionResult AddProductToCart(int? Id)
        {
            CartItem item = new CartItem();
            item.SubscriptionTypeId = Convert.ToInt32(Id);
            item.Quantity = 2;
            item.DateCreated = DateTime.Now;
            item.UserId = LicenseSessionState.Instance.User.ServerUserId;
            item.Price = subscriptionTypeLogic.GetById(item.SubscriptionTypeId).Price;
            bool status = cartLogic.CreateCartItem(item);
            return  RedirectToAction("ProductCatalog", "Product");
        }
    }
}