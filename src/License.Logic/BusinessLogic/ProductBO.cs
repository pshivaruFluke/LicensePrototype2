﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using License.Logic.DataLogic;
using License.Models;

namespace License.Logic.BusinessLogic
{
    public class ProductBO
    {
        /// <summary>
        /// Get the product list based on the Availablity of the License for the Product and Expire Date of the Product
        /// </summary>
        /// <returns></returns>
        public List<Product> GetProductFromLicenseData(string adminId, string userId = "")
        {
            UserSubscriptionLogic userSubscriptionLogic = new UserSubscriptionLogic();
            List<Product> prodList = new List<Product>();
            SubscriptionFileIO subscriptionBO = new SubscriptionFileIO();
            ProductLicenseLogic proLicenseLogic = new ProductLicenseLogic();
            var productIdList = new List<int>();

            // Get The Products which are  team License Products to exclude these products from the listing in the Product for the User Assign
            List<int> proList = new List<int>();
            if (!String.IsNullOrEmpty(userId))
            {
                bool isAdmin = adminId == userId;
                TeamBO teamBo = new TeamBO();
                proList = teamBo.GetTeamProductByUserId(userId, isAdmin);
            }

            // Get the list of user Subscription by admin Id.
            var userSubscriptionList = userSubscriptionLogic.GetSubscription(adminId);

            // Fetches the Subscription Id Which are not expired and get the ID List
            userSubscriptionList = userSubscriptionList.Where(l => (l.ExpireDate.Date - DateTime.Today).Days >= 0).ToList();

            // If there are no subscription then the Empty product list will kbe returned
            if (userSubscriptionList == null || userSubscriptionList.Count == 0)
                return new List<Product>();

            var userSubscriptionIdList = userSubscriptionList.Select(us => us.Id).ToList();

            userSubscriptionList.ForEach(us =>
            {
                var licList = proLicenseLogic.GetLicenseList(us.Id).Where(ld => ld.IsMapped == false).ToList();
                if (licList != null && licList.Count > 0)
                    productIdList.AddRange(licList.Select(ld => ld.ProductId));
            });

            if (productIdList == null || productIdList.Count == 0)
                return new List<Product>();


            var prodIdList = (from id in productIdList
                              group id by id into list
                              select new { list.Key, Count = list.Count() }).ToList();

            for (int index = 0; index < prodIdList.Count; index++)
            {
                if (!proList.Contains(prodIdList[index].Key))
                {
                    var product = subscriptionBO.GetProductFromJsonFile(prodIdList[index].Key);
                    if (product != null)
                    {
                        product.AvailableCount = prodIdList[index].Count;
                        prodList.Add(product);
                    }
                }
            }
            if (!string.IsNullOrEmpty(userId))
            {
                UserLicenseLogic userLicLogic = new UserLicenseLogic();
                var userLicense = userLicLogic.GetUserLicenseByUserId(userId);
                var productIds = userLicense.Select(l => l.License.ProductId).ToList();
                prodList.ForEach(p =>
                {
                    if (productIds.Contains(p.Id))
                        p.IsSelected = true;

                });

            }
            return prodList;
        }
    }
}
