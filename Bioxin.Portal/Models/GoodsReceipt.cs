using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebSolution.Models
{
    public class GoodsReceipt
    {
        public string PostingDate { get; set; }
        public string RefNo { get; set; }
        public string RefDate { get; set; }
        public string Remarks { get; set; }
        public List<GoodsReceiptDetail> Items { get; set; }
    }
}