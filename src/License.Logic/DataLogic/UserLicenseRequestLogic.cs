﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using License.DataModel;
using License.Logic.BusinessLogic;

namespace License.Logic.DataLogic
{
    public class UserLicenseRequestLogic : BaseLogic
    {

        List<SubscriptionType> subList = null;
        /// <summary>
        /// Creating the single object  to DB
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        private bool CreateRequest(UserLicenseRequest req)
        {
            var obj = AutoMapper.Mapper.Map<Core.Model.UserLicenseRequest>(req);
            obj = Work.UserLicenseRequestRepo.Create(obj);
            return obj.Id > 0;
        }

        /// <summary>
        /// Updating the single object  to DB
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        private bool UpdateRequest(UserLicenseRequest req)
        {
            var obj = Work.UserLicenseRequestRepo.GetById(req.Id);
            obj.IsApproved = req.IsApproved;
            obj.IsRejected = req.IsRejected;
            obj.ApprovedBy = req.ApprovedBy;
            obj.Comment = req.Comment;
            obj = Work.UserLicenseRequestRepo.Update(obj);
            Work.UserLicenseRequestRepo.Save();
            return obj != null;
        }

        public void Create(List<UserLicenseRequest> reqList)
        {
            bool status = true;
            int i = 0;
            foreach (var obj in reqList)
            {
                status &= CreateRequest(obj);
                i++;
            }
            if (i > 0)
                Work.UserLicenseRequestRepo.Save();
        }

        public void Update(List<UserLicenseRequest> reqList)
        {
            bool status = true;
            int i = 0;
            foreach (var obj in reqList)
            {
                status &= UpdateRequest(obj);
                i++;
            }
            if (i > 0)
                Work.UserLicenseRequestRepo.Save();
        }

        /// <summary>
        /// Function to get the Licence Request of all the users based  on the admin id
        /// </summary>
        /// <param name="adminId"></param>
        /// <returns></returns>
        public List<UserLicenseRequest> GetAllRequestList(string adminId)
        {
            List<UserLicenseRequest> requestList = new List<UserLicenseRequest>();
            SubscriptionBO proSubLogic = new SubscriptionBO();
            subList = proSubLogic.GetSubscriptionFromFile();
            var teamList = Work.TeamRepository.GetData(f => f.AdminId == adminId);
            foreach (var team in teamList)
            {
                var dataList = GetRequestListByTeam(team.Id);
                requestList.AddRange(dataList);
            }
            if (requestList.Count > 0)
                return requestList;
            return null;
        }

        public List<UserLicenseRequest> GetRequestListByTeam(int teamId)
        {
            if(subList == null)
            {
                SubscriptionBO proSubLogic = new SubscriptionBO();
                subList = proSubLogic.GetSubscriptionFromFile();
            }
            var userlist = Work.TeamMemberRepository.GetData(f => f.TeamId == teamId).ToList();
            if (userlist.Count > 0)
            {
                var idList = userlist.Select(u => u.InviteeUserId).ToList();
                var licReqList = Work.UserLicenseRequestRepo.GetData(f => idList.Contains(f.Requested_UserId) && String.IsNullOrEmpty(f.ApprovedBy) && f.TeamId == teamId).ToList();
                if (licReqList.Count > 0)
                {
                    List<UserLicenseRequest> userLicReq = new List<UserLicenseRequest>();
                    foreach (var obj in licReqList)
                    {
                        var tempObj = AutoMapper.Mapper.Map<License.DataModel.UserLicenseRequest>(obj);
                        var subscription = subList.FirstOrDefault(f => f.Id == tempObj.UserSubscripption.SubscriptionId);
                        tempObj.UserSubscripption.Subscription = new SubscriptionType() { Id = subscription.Id, Name = subscription.Name };
                        tempObj.Product = subscription.Products.FirstOrDefault(p => p.Id == tempObj.ProductId);
                        userLicReq.Add(tempObj);
                    }
                    return userLicReq;
                }
            }
            return null;
        }

        /// <summary>
        /// Get the Requested License based on the UserId1
        /// </summary>
        /// <returns></returns>
        public List<UserLicenseRequest> GetLicenseRequest(string userId)
        {
            SubscriptionBO proSubLogic = new SubscriptionBO();
            var subList = proSubLogic.GetSubscriptionFromFile();

            var licReqList = Work.UserLicenseRequestRepo.GetData(f => f.Requested_UserId == userId).ToList();
            if (licReqList.Count > 0)
            {
                var list = licReqList.GroupBy(f => f.UserSubscriptionId).ToList();
                List<UserLicenseRequest> userLicReq = new List<UserLicenseRequest>();
                foreach (var obj in licReqList)
                {
                    var tempObj = AutoMapper.Mapper.Map<License.DataModel.UserLicenseRequest>(obj);
                    var subscription = subList.FirstOrDefault(f => f.Id == tempObj.UserSubscripption.SubscriptionId);
                    tempObj.UserSubscripption.Subscription = new SubscriptionType() { Id = subscription.Id, Name = subscription.Name };
                    tempObj.Product = subscription.Products.FirstOrDefault(p => p.Id == tempObj.ProductId);
                    userLicReq.Add(tempObj);
                }
                return userLicReq;
            }
            return null;
        }


        public UserLicenseRequest GetById(int id)
        {
            var data = Work.UserLicenseRequestRepo.GetById(id);
            return AutoMapper.Mapper.Map<DataModel.UserLicenseRequest>(data);
        }

    }
}
