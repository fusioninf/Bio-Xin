using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebSolution.Models
{
    public class CustomerVoucherBalance
    {
        public string AcctCode { get; set; }
        public string VoucherType { get; set; }
        public string CardCode { get; set; }
        public string CardId { get; set; }
        public string CardNo { get; set; }
        public string ValidTo { get; set; }
        public double Balance { get; set; }
    }
}