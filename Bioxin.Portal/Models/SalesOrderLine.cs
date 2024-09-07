using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebSolution.Models
{

    public class SalesOrderLine
    {
        [Key]
        public int DocEntry { get; set; }
        public int LineNum { get; set; }
        public string LineStatus { get; set; }
        public string ItemCode { get; set; }
        public string ItemDescription { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string Currency { get; set; }
        public string WarehouseCode { get; set; }
        public DateTime ShipDate { get; set; }
        public decimal LineTotal { get; set; }
        public string VatGroup { get; set; }
        public string TaxCode { get; set; }
        public decimal TaxPercentagePerRow { get; set; }
        public decimal DiscountPercent { get; set; }
        public bool IsDelete { get; set; }
    }
}