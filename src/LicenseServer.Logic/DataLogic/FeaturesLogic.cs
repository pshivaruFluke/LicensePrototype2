﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LicenseServer.Core.Model;
using FeatureDataModel = LicenseServer.DataModel.Feature;

namespace LicenseServer.Logic
{
    public class FeaturesLogic : BaseLogic
    {
        public List<FeatureDataModel> GetFeatureList()
        {
            var features = Work.FeaturesRepository.GetData().ToList();
            List<FeatureDataModel> featureList = new List<FeatureDataModel>();
            foreach (Feature f in features)
            {
                FeatureDataModel f1 = AutoMapper.Mapper.Map<FeatureDataModel>(f);
                featureList.Add(f1);
            }
            return featureList;
        }

        public bool CreateFeature(FeatureDataModel f)
        {
            Feature fet = AutoMapper.Mapper.Map<Feature>(f);
            fet = Work.FeaturesRepository.Create(fet);
            Work.FeaturesRepository.Save();
            return fet.Id > 0;
        }

        public bool DeleteFeature(int id)
        {
            var status = Work.FeaturesRepository.Delete(id);
            Work.FeaturesRepository.Save();
            return status;
        }

        public bool Update(int id, FeatureDataModel f)
        {
            Feature fet = Work.FeaturesRepository.GetById(id);
            fet.Name = f.Name;
            fet.Description = f.Description;
            fet.Version = f.Version;
            Work.FeaturesRepository.Save();
            return true;
        }

        public FeatureDataModel GetFeatureById(int id)
        {
            var f = Work.FeaturesRepository.GetById(id);
            return AutoMapper.Mapper.Map<FeatureDataModel>(f);
        }

        public List<FeatureDataModel> GetFeatureByCategoryId(int categoryId)
        {
            List<FeatureDataModel> featureList = new List<FeatureDataModel>();
            var category = Work.ProductCategoryRepository.GetById(categoryId);
            if (category != null)
            {
                if (category.Features != null && category.Features.Count > 0)
                {
                    foreach (var fet in category.Features)
                    {
                        var objFeture = AutoMapper.Mapper.Map<FeatureDataModel>(fet);
                        featureList.Add(objFeture);
                    }
                }
            }
            return featureList;
        }
    }
}
