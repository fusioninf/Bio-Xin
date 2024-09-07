using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebSolution.Models
{
    public class GoodsIssue
    {
        public string Branch { get; set; }
        public string PostingDate { get; set; }
        public string RefNo { get; set; }
        public string RefDate { get; set; }
        public string Remarks { get; set; }
        public List<GoodsIssueDetail> Items { get; set; }
    }
}