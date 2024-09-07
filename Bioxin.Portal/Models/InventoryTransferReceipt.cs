using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebSolution.Models
{
    public class InventoryTransferReceipt
    {
        public string PostingDate { get; set; }
        public string DocEntry { get; set; }
        public string ReceiptWarehouse { get; set; }
    }
}