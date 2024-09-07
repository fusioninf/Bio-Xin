using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebSolution.Models
{
    public class TransferRequsetDetail
    {
        public string ItemCode { get; set; }
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
        public string Remarks { get; set; }

        public string BaseEntry { get; set; }
        public string BaseLine { get; set; }
        public string BaseType { get; set; }
    }
}