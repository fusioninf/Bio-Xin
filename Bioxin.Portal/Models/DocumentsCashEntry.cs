using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebSolution.Models
{
    public class DocumentsCashEntry
    {
        public int TransId { get; set; }
        public int Number { get; set; }
        public string SeriesName { get; set; }
        public string DocNum { get; set; }
        public string RefDate { get; set; }
        public string AcctCode { get; set; }
        public string AcctName { get; set; }
        public double Total { get; set; }
        public string Memo { get; set; }
        public List<DocumentsCashEntryDetail> cashEntryDetails { get; set; }
        public int UnAutorized { get; set; }
        public string ReturnCode { get; set; }
    }
}