﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace License.MetCalWeb.Models
{
    /// <summary>
    /// This entire region is used to sync the subscription details between 2 servers.
    /// </summary>
    /// 
    #region syncSubscription 

    public class SubscriptionList
    {
        public string UserId { get; set; }

        public List<Subscription> Subscriptions { get; set; }

        public SubscriptionList()
        {
            Subscriptions = new List<Subscription>();
        }
    }

    public class Subscription
    {
        public int SubscriptionTypeId { get; set; }

        public DateTime SubscriptionDate { get; set; }
        public SubscriptionType SubscriptionType { get; set; }

        public List<LicenseKeyProductMapping> LicenseKeyProductMapping { get; set; }

        public Subscription()
        {
            LicenseKeyProductMapping = new List<LicenseKeyProductMapping>();
        }

        public int OrderdQuantity { get; set; }
    }

    public class LicenseKeyProductMapping
    {
        public string LicenseKey { get; set; }
        public int ProductId { get; set; }

    }

    public class SubscriptionType
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ImagePath { get; set; }
        public int ActiveDays { get; set; }

        public int ActivationMonth { get; set; }
        public double Price { get; set; }

        public string CreatedBy { get; set; }
        public IEnumerable<Product> Products { get; set; }
    }

    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ProductCode { get; set; }
        public int Quantity { get; set; }
        public double Price { get; set; }
        public ICollection<ProductCategory> Categories { get; set; }
        public ICollection<Feature> AssociatedFeatures { get; set; }

    }

    public class UserSubscriptionData
    {

        public string UserId { get; set; }

        public int SubscriptionId { get; set; }

        public DateTime SubscriptionDate { get; set; }

        public Subscription Subscription { get; set; }

        public int Quantity { get; set; }

        public List<LicenseKeyProductMapping> LicenseKeys { get; set; }
    }


    #endregion
    public class SubscriptionDetails
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int UserSubscriptionId { get; set; }
        public List<ProductDetails> Products { get; set; }
        public SubscriptionDetails()
        {
            Products = new List<ProductDetails>();
        }
    }

    public class ProductDetails
    {
        public int Id { get; set; }
        public string ProductCode { get; set; }
        public string Name { get; set; }
        public String Description { get; set; }
        public int TotalLicenseCount { get; set; }
        public int UsedLicenseCount { get; set; }
        public bool IsSelected { get; set; }
        public bool IsDisabled { get; set; }
        public bool InitialState { get; set; }
        public List<Feature> Features { get; set; }
        public DateTime ExpireDate { get; set; }
        public ProductDetails()
        {
            Features = new List<Feature>();
        }

    }

    public class Feature
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
        public string Version { get; set; }
        public bool IsEnabled { get; set; }
        public bool Selected { get; set; }
        
    }


    public class ProductCategory
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsSelected { get; set; }
        public ICollection<Product> Products { get; set; }
        public int ActivationMonth { get; set; }
        public double Price { get; set; }
    }

    public class ProductDependency
    {
        public List<ProductCategory> Categories { get; set; }

        public List<Feature> Features { get; set; }
    }

}
