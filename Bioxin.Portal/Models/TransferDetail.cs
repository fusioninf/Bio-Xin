using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace WebSolution.Models
{
    public class TransferDetail
    {
        public string VisOrder { get; set; }
        public string BaseType { get; set; }
        public int? BaseEntry { get; set; }
        public int? BaseLine { get; set; }
        public string ItemCode { get; set; }
        public string FromWareHouse { get; set; }
        public string ToWareHouse { get; set; }
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
        public string Remarks { get; set; }
        public List<Batches> Batches { get; set; }
        public List<Serial> Serial { get; set; }
    }
}