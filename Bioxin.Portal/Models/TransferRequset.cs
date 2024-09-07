using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebSolution.Models
{
    public class TransferRequset
    {
        public string CardCode { get; set; }
        public string PostingDate { get; set; }
        public string DueDate { get; set; }
        public string RefDate { get; set; }
        public string ShiptoCode { get; set; }
        public string FromWarehouse { get; set; }
        public string ContactPerson { get; set; }
        public string Remarks { get; set; }
        public string SalesEmployee { get; set; }

        public string BaseEntry { get; set; }
        public string ApprovedBy { get; set; }
        public string ApprovedDate { get; set; }
        public string SOEntry { get; set; }

        public List<TransferRequsetDetail> Items { get; set; }
    }
}