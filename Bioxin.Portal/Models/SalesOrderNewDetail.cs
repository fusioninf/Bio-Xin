using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebSolution.Models
{
    public class SalesOrderNewDetail
    {
        public string ItemCode { get; set; }
        public decimal Quantity { get; set; }
        public decimal PriceBeforeDiscount { get; set; }
        public string UOM { get; set; }
        public string DiscountPercentage { get; set; }
        public string VisOrder { get; set; }
        public string BaseType { get; set; }
        public string BaseEntry { get; set; }
        public string BaseLine { get; set; }
        public string DocDueDate { get; set; }
        public string DueDate { get; set; }
        public string TaxCode { get; set; }
        public string Discountamount { get; set; }
        public string DeliveryDate { get; set; }
        public string WhsCode { get; set; }
    }
}