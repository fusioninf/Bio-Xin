using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebSolution.Models
{
    public class DiscBasedOnItem
    {
        public string PostingDate { get; set; }
        public string PostingTime { get; set; }
        public string DiscCode { get; set; }
        public string HappyHrs { get; set; }
        public string BusinesUnit { get; set; }
        public string CardCode { get; set; }
        public List<DiscountItem> Item { get; set; }
    }
}