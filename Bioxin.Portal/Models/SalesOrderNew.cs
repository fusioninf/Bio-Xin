using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebSolution.Models
{
    public class SalesOrderNew
    {
        public string DocEntry { get; set; }
        public string PaymentAccount { get; set; }
        public string CardCode { get; set; }
        public string Branch { get; set; }
        public string PostingDate { get; set; }
        public string DocDueDate { get; set; }
        public string RefNo { get; set; }
        public string RefDate { get; set; }
        public string Remarks { get; set; }
        public string SalesEmployee { get; set; }
        public string ToBranch { get; set; }
        public string PaymentAccountCode { get; set; }
        public double FullAmount { get; set; }
        public string ItemType { get; set; }
        public double freight { get; set; }
        public string saleschannel { get; set; }
        public string DeliveryAddress { get; set; }
        public List<SalesOrderNewDetail> Items { get; set; }
        public List<PaymentDetail> PaymentDetails { get; set; }
    }
}