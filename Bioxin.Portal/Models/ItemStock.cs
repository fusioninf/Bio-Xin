using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebSolution.Models
{
    public class ItemStock
    {
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string FrgnName { get; set; }
        public string WarehouseTypeCode { get; set; }
        public string WarehouseTypeName { get; set; }
        public int CompanyCode { get; set; }
        public string CompanyName { get; set; }
        public string BuisnessUnitCode { get; set; }
        public string BuisnessUnitName { get; set; }
        public string WhsCode { get; set; }
        public string WhsName { get; set; }
        public double OnHand { get; set; }
        public double IsCommited { get; set; }
        public double OnOrder { get; set; }
        public double MinStock { get; set; }
        public double MaxStock { get; set; }
    }
}