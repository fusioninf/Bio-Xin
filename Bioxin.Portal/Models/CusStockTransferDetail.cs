using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebSolution.Models
{
    public class CusStockTransferDetail
    {
        public string ItemCode { get; set; }
        public string ItemDescription { get; set; }
        public double Quantity { get; set; }
        public double UnitPrice { get; set; }
        public string FromWarehouseCode { get; set; }
        public string WarehouseCode { get; set; }
    }
}