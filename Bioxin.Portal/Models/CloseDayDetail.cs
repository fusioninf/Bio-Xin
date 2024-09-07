using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebSolution.Models
{
    public class CloseDayDetail
    {
        public int SerialNo { get; set; }
        public decimal Currency { get; set; }
        public string TypeCode { get; set; }
        public int No { get; set; }
        public decimal TotalAmount { get; set; }
        public string Remarks { get; set; }
    }
}