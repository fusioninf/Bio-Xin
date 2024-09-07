using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebSolution.Models
{
    public class DiscountItem
    {
        public int LineNum { get; set; }
        public string Item_Code { get; set; }
        public double Quantity { get; set; }
    }
}