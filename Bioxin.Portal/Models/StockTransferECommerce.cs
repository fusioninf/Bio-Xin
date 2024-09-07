using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebSolution.Models
{
    public class StockTransferECommerce
    {
        public string DocEntry { get; set; }
        public string TrackingID { get; set; }
        public string CourierCompany { get; set; }
        public string StatusCode { get; set; }
        public string DelChannel { get; set; }
        public string DelAgent { get; set; }
        public string Area { get; set; }
        public string ConfDate { get; set; }
        public string Remarks { get; set; }
    }
}