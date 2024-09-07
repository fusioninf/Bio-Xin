using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebSolution.Models
{
    public class SalesOrderNewPaymentDetail
    {
        public string PaymentType { get; set; }
        public string UpiName { get; set; }
        public string CardNo { get; set; }
        public string CardHolderName { get; set; }
        public string Tranid { get; set; }
        public decimal Amount { get; set; }
    }
}