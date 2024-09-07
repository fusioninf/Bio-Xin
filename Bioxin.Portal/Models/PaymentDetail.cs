using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebSolution.Models
{
    public class PaymentDetail
    {
        public string PaymentType { get; set; }
        public string Bank { get; set; }
        public string CardNo { get; set; }
        public string Tranid { get; set; }
        public double Amount { get; set; }
    }
}