using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebSolution.Models
{
    public class CusStockTransfer
    {
        public string CardCode { get; set; }
        public string CardName { get; set; }
        public int ContactPerson { get; set; }
        public DateTime DocDate { get; set; }
        public DateTime DueDate { get; set; }
        public string FromWarehouse { get; set; }
        public string ToWarehouse { get; set; }
        public string Comments { get; set; }
        public List<CusStockTransferDetail> CusStockTransferDetails { get; set; }


    }
}