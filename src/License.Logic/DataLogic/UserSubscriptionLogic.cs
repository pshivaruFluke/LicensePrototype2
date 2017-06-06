﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using License.DataModel;

namespace License.Logic.DataLogic
{
    /// <summary>
    /// History 
    ///     Created By :
    ///     Created Date :
    ///     Purpose : 1. CRU action the user subscription
    ///               2. Creation of the Json File about the subscription and Product in the encrypted JSON format
    /// </summary>
    public class UserSubscriptionLogic : BaseLogic
    {
        // Get List of the User Subscription list based on the User ID
        public List<UserSubscription> GetSubscription(string userId)
        {
            List<UserSubscription> subscriptionList = new List<UserSubscription>();
            ProductLicenseLogic logic = new ProductLicenseLogic();
            var subsList = Work.UserSubscriptionRepository.GetData(us => us.UserId == userId);
            subscriptionList = subsList.Select(obj => AutoMapper.Mapper.Map<UserSubscription>(obj)).ToList();
            return subscriptionList;
        }

        // Get User Subscription based on the user subscription Id
        public List<UserSubscription> GetSubscriptionByIDList(List<int> idList)
        {
            List<UserSubscription> subscriptionList = new List<UserSubscription>();
            ProductLicenseLogic logic = new ProductLicenseLogic();
            var subsList = Work.UserSubscriptionRepository.GetData(us => idList.Contains(us.Id));
            subscriptionList = subsList.Select(obj => AutoMapper.Mapper.Map<UserSubscription>(obj)).ToList();
            return subscriptionList;
        }

        //Create User Subscription
        public int CreateSubscription(UserSubscription subs)
        {
            var obj = AutoMapper.Mapper.Map<UserSubscription, License.Core.Model.UserSubscription>(subs);
            obj = Work.UserSubscriptionRepository.Create(obj);
            Work.UserSubscriptionRepository.Save();
            return obj.Id;
        }

        // Update Bulk Subscriptions
        public void UpdateSubscriptions(List<UserSubscription> subs)
        {
            int i = 0;
            foreach (var sub in subs)
            {
                var subObj = Work.UserSubscriptionRepository.GetById(sub.Id);
                subObj.RenewalDate = sub.RenewalDate;
                Work.UserSubscriptionRepository.Update(subObj);
                i++;
            }
            if (i > 0)
                Work.UserSubscriptionRepository.Save();
        }
    }
}
