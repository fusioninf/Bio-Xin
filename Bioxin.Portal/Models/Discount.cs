using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebSolution.Models
{
    public class Discount
    {
        public string DiscCode { get; set; }
        public string DiscName { get; set; }
        public int LineNum { get; set; }
        public string Item_Code { get; set; }
        public double DISCOUNT { get; set; }
        public string ReturnCode { get; set; }
        public string ReturnMsg { get; set; }
    }
}