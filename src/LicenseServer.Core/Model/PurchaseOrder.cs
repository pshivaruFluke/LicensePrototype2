﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace LicenseServer.Core.Model
{
    public class PurchaseOrder
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string PurchaseOrderNo { get; set; }
        public bool IsApproved { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public bool IsSynched { get; set; }
        public string ApprovedBy { get; set; }
        [ForeignKey("UserId")]
        public virtual Appuser User { get; set; }
        public string Comment { get; set; }
        public double Total { get; set; }
        public virtual ICollection<PurchaseOrderItem> OrderItems { get; set; }
        public PurchaseOrder()
        {
            CreatedDate = new DateTime(1900, 1, 1);
            UpdatedDate = new DateTime(1900, 1, 1);
        }
    }

    public class PurchaseOrderItem
    {
        public int Id { get; set; }
        public int SubscriptionId { get; set; }
        public int Quantity { get; set; }
        public int PurchaseOrderId { get; set; }
        [ForeignKey("PurchaseOrderId")]
        public PurchaseOrder PurchaseOrder { get; set; }
        [ForeignKey("SubscriptionId")]
        public virtual SubscriptionType Subscription { get; set; }
    }
}
