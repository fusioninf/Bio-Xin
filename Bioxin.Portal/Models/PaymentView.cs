using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebSolution.Models
{
    public class PaymentView
    {
        public string Type { get; set; }
        public string TypeDesc { get; set; }
        public string Bank { get; set; }
        public double Amount { get; set; }
        public string CardNo { get; set; }
        public string VoucherNo { get; set; }
        public string TransId { get; set; }
    }
}