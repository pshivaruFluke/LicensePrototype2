﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net.Http;
using License.MetCalWeb.Common;
using System.Threading.Tasks;
using Newtonsoft.Json;
using License.MetCalWeb.Models;

namespace License.MetCalWeb.Controllers
{
    [Authorize]
    [SessionExpire]
    public class CartController : BaseController
    {
        public CartController()
        {


        }

        public async Task<ActionResult> CartItem()
        {
            List<CartItem> itemList = new List<CartItem>();
            if (LicenseSessionState.Instance.User == null)
            {
                return RedirectToAction("LogIn", "Account");
            }
            itemList = await GetCartItems();
            return View(itemList);
        }

        public async Task<ActionResult> RemoveItem(int id)
        {
            HttpClient client = WebApiServiceLogic.CreateClient(ServiceType.CentralizeWebApi);
            var response = await client.DeleteAsync("api/cart/Delete/" + id);
            return RedirectToAction("CartItem", "Cart");
        }

        public ActionResult PaymentGateway(string total)
        {
            ViewData["Total"] = total;
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> DoPayment()
        {
            await Purchase();
            return View();
        }

        public async Task Purchase()
        {
            if (TempData["RenewSubscription"] != null)
                await Common.CentralizedSubscriptionLogic.RenewSubscription((RenewSubscriptionList)TempData["RenewSubscription"]);
            else
                await Common.CentralizedSubscriptionLogic.UpdateUserSubscription();
        }

        public void SyncRenewData()
        {
            HttpClient client = WebApiServiceLogic.CreateClient(ServiceType.CentralizeWebApi);
            var response = client.PostAsJsonAsync("api/UserSubscription/RenewSubscription/" + LicenseSessionState.Instance.User.ServerUserId, TempData["RenewSubscription"]).Result;
            if (response.IsSuccessStatusCode)
            {
                var jsonData = response.Content.ReadAsStringAsync().Result;
            }
        }

        public async Task<ActionResult> OfflinePayment()
        {
            PurchaseOrder poOrder = new PurchaseOrder();
            HttpClient client = WebApiServiceLogic.CreateClient(ServiceType.CentralizeWebApi);
            var response = await client.PostAsync("api/cart/offlinepayment/" + LicenseSessionState.Instance.User.ServerUserId, null);
            if (response.IsSuccessStatusCode)
            {
                var jsonData = response.Content.ReadAsStringAsync().Result;
                if (!string.IsNullOrEmpty(jsonData))
                    poOrder = JsonConvert.DeserializeObject<PurchaseOrder>(jsonData);
            }
            else
            {
                var jsonData = response.Content.ReadAsStringAsync().Result;
                var obj = JsonConvert.DeserializeObject<ResponseFailure>(jsonData);
                ModelState.AddModelError("", response.ReasonPhrase + " - " + obj.Message);
            }
            return View(poOrder);

        }

        private async Task<List<CartItem>> GetCartItems()
        {
            List<CartItem> itemList = null;
            HttpClient client = WebApiServiceLogic.CreateClient(ServiceType.CentralizeWebApi);
            var response = await client.GetAsync("api/cart/getItems/" + LicenseSessionState.Instance.User.ServerUserId);
            if (response.IsSuccessStatusCode)
            {
                var jsonData = response.Content.ReadAsStringAsync().Result;
                itemList = JsonConvert.DeserializeObject<List<CartItem>>(jsonData);
            }
            else
            {
                var jsonData = response.Content.ReadAsStringAsync().Result;
                var obj = JsonConvert.DeserializeObject<ResponseFailure>(jsonData);
                ModelState.AddModelError("", response.ReasonPhrase + " - " + obj.Message);
            }
            return itemList;
        }


        public async Task<ActionResult> AddProductToCart(int? Id)
        {
            CartItem item = new CartItem()
            {
                SubscriptionTypeId = Convert.ToInt32(Id),
                Quantity = 1,
                DateCreated = DateTime.Now,
                UserId = LicenseSessionState.Instance.User.ServerUserId
            };
            HttpClient client = WebApiServiceLogic.CreateClient(ServiceType.CentralizeWebApi);
            var response = await client.PostAsJsonAsync("api/Cart/Create", item);
            if (response.IsSuccessStatusCode)
                return RedirectToAction("Index", "Subscription");
            else
            {
                var jsonData = response.Content.ReadAsStringAsync().Result;
                var obj = JsonConvert.DeserializeObject<ResponseFailure>(jsonData);
                ModelState.AddModelError("", response.ReasonPhrase + " - " + obj.Message);
            }
            return null;
        }

    }
}