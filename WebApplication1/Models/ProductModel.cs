﻿using System.ComponentModel.DataAnnotations;

namespace License.MetCalWeb.Models
{
    public class Product
    {
        public int ProductID { get; set; }

        public string ProductName { get; set; }

        //[Required, StringLength(10000), Display(Name = "Product Description"), DataType(DataType.MultilineText)]
        public string Description { get; set; }

        public string ImagePath { get; set; }

        [Display(Name = "Price")]
        public double? UnitPrice { get; set; }

        public int? CategoryID { get; set; }


    }
    /// <summary>
    /// This class holds credit card details 
    /// </summary>
    public class CardDetails
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public CardDetails()
        {

        }
        /// <summary>
        /// Credit card holder name 
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Credit card holder Number
        /// </summary>
        public string Number { get; set; }
        /// <summary>
        /// Credit card holder Expiry month
        /// </summary>
        public string Month { get; set; }
        /// <summary>
        /// Credit card expiry Year
        /// </summary>
        public short Years { get; set; }
        /// <summary>
        /// Credit card CVV number
        /// </summary>
        public short CVVNum { get; set; }
    }
}