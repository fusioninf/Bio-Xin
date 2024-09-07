using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebSolution.Models
{
    public class CashExpenseJournal
    {
        public string PostingDate { get; set; }
        public string Remarks { get; set; }
        public string AccountCode { get; set; }
        public double TotalValue { get; set; }
        public List<CashExpenseJournalDetail> Items { get; set; }
    }
}