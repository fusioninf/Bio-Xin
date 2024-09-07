using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebSolution.Models
{
    public class Warehouse
    {
        public string WhsCode { get; set; }
        public string WhsName { get; set; }
        public string WarehouseTypeCode { get; set; }
        public string WarehouseTypeName { get; set; }
        public int CompanyCode { get; set; }
        public string CompanyName { get; set; }
        public string BuisnessUnitCode { get; set; }
        public string BuisnessUnitName { get; set; }
    }
}