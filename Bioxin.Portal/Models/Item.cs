using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebSolution.Models
{
    public class Item
    {
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string FrgnName { get; set; }
        public int ItemGroupCode { get; set; }
        public string ItemGroupName { get; set; }
        public string SellItem { get; set; }
        public string PrchseItem { get; set; }
        public string InvntItem { get; set; }
        public double LastPurchasePrice { get; set; }
        public double LastEaluatedPrice { get; set; }
        public double CompanyWiseStock { get; set; }
        public string BatchNumberEnable { get; set; }
        public string SerialNumberEnable { get; set; }
        public string InventoryUOM { get; set; }
        public string PurchaseUOM { get; set; }
        public string SalesUOM { get; set; }
        public int ManufacturerCode { get; set; }
        public string ManufacturerName { get; set; }
        public string Value { get; set; }
        public string Description { get; set; }
        public string TaxCode { get; set; }
        public double TaxRate { get; set; }
        public double Price { get; set; }
        public string DeliveryDate { get; set; }
        public string VoucherItem { get; set; }
        public int VisOrder { get; set; }
        public string Type { get; set; }
        public string BaseType { get; set; }
        public int BaseEntry { get; set; }
        public int BaseLine { get; set; }
        public double Quantity { get; set; }
        public double Discountamount { get; set; }
        public double Stock { get; set; }
        public string WhsCode { get; set; }
        public double DiscountPercentage { get; set; }
        public double PriceBeforeDiscount { get; set; }
        public double DifferenceAmount { get; set; }
        public double Discount { get; set; }
        public string UOM { get; set; }
        public string DocDueDate { get; set; }
        public string VoucherNo { get; set; }
        public string ValidTill { get; set; }
        public string ReturnCode { get; set; }
        public int SequenceNo { get; set; }
        public List<Batches> Batches { get; set; }
        public List<Serial> Serial { get; set; }
    }
}