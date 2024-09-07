using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace WebSolution.Models
{
    public class Transfer
    {
        public string CardCode { get; set; }
        public string PostingDate { get; set; }
        public string DueDate { get; set; }
        public string RefDate { get; set; }
        public string ShiptoCode { get; set; }
        public string ContactPerson { get; set; }
        public string SalesEmployee { get; set; }
        public string FromWarehouse { get; set; }
        public string ToWarehouse { get; set; }
        public string Remarks { get; set; }
        public string EnteredBy { get; set; }
        public List<TransferDetail> Items { get; set; }

        public Transfer()
        {
            CardCode = "";
            PostingDate = "";
            DueDate= "";
            RefDate = "";
            EnteredBy = "";
        }
    }
}