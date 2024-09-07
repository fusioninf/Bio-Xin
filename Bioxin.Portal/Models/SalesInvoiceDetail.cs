using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebSolution.Models
{
    public class SalesInvoiceDetail
    {
        public int DocEntry { get; set; }
        public int LineNum { get; set; }
        public string LineStatus { get; set; }
        public string ItemCode { get; set; }
        public string ItemDescription { get; set; }
        public double Quantity { get; set; }
        public double UnitPrice { get; set; }
        public string Currency { get; set; }
        public string WarehouseCode { get; set; }
        public DateTime ShipDate { get; set; }
        public double LineTotal { get; set; }
        public string VatGroup { get; set; }
        public string TaxCode { get; set; }
        public double TaxPercentagePerRow { get; set; }
        public double DiscountPercent { get; set; }
        public double VisOrder { get; set; }
        public string BaseType { get; set; }
        public double BaseEntry { get; set; }
        public double BaseLine { get; set; }
        public double Discountamount { get; set; }
        public string BatchNo { get; set; }
        public double BatchQuantity { get; set; }
        public string InternalSerialNumber { get; set; }
        public string SystemSerialNumber { get; set; }
        public string ManufacturerSerialNumber { get; set; }
    }
}