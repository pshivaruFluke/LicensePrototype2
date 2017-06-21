﻿using License.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace License.Logic.DataLogic
{
    public class VISMADataLogic:BaseLogic
    {
        /// <summary>
        /// Creating VISMA  record in db
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public List<VISMAData> CreateVISMAData(List<VISMAData> listData)
        {
            List<VISMAData> list = new List<VISMAData>();
            foreach (var data in listData)
            {
                var obj = AutoMapper.Mapper.Map<License.Core.Model.VISMAData >(data);
                obj = Work.VISMADataRepository.Create(obj);
                Work.ProductLicenseRepository.Save();
                list.Add(AutoMapper.Mapper.Map<VISMAData>(obj));
            }
            return list ;
        }

        public VISMAData UpdateVISMAData(VISMAData _VISMAData)
        {
            List<VISMAData> list = new List<VISMAData>();
            var obj = Work.VISMADataRepository.GetById(_VISMAData.Id);
            if (obj != null)
            {
                obj.TestDevice = _VISMAData.TestDevice;
                obj.ExpirationDate = _VISMAData.ExpirationDate;
                obj = Work.VISMADataRepository.Update(obj);
                Work.VISMADataRepository.Save();
                return AutoMapper.Mapper.Map<VISMAData>(obj);
            }
            else
                ErrorMessage = "Specified data not exist";
            return null;
        }

        public bool DeleteVISMAData(int  _VISMADataId)
        {
            List<VISMAData> dataList = new List<VISMAData>();
            bool status = Work.VISMADataRepository.Delete(_VISMADataId);
            Work.VISMADataRepository.Save();
            return status;
        }
        public List<VISMAData> GetVISMADataByTestDevice(string testDevice)
        {
            List<VISMAData> dataList = new List<VISMAData>();
            var list = Work.VISMADataRepository.GetData(t => t.TestDevice == testDevice);
            dataList = list.Select(l => AutoMapper.Mapper.Map<VISMAData>(l)).ToList();
            return dataList;
        }
        public List<VISMAData> GetAllVISMAData()
        {
            List<VISMAData> dataList = new List<VISMAData>();
            var list = Work.VISMADataRepository.GetData();
            dataList = list.Select(l => AutoMapper.Mapper.Map<VISMAData>(l)).ToList();
            return dataList;
        }

    }
}