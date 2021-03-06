﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace License.Core.Model
{
    public class UserSubscription
    {
        [Key]
        public int Id { get; set; }

        public string UserId { get; set; }

        public int SubscriptionId { get; set; }

        public DateTime SubscriptionDate { get; set; } = new DateTime(1900, 01, 01);
        public DateTime RenewalDate { get; set; } = new DateTime(1900, 01, 01);

        [ForeignKey("UserId")]
        public AppUser User { get; set; }

        public int Quantity { get; set; }

        //[ForeignKey("SubscriptionId")]
        //public Subscription Subscription { get; set; }
    }
}
