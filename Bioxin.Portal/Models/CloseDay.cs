using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebSolution.Models
{
    public class CloseDay
    {
        public string CloseDate { get; set; }
        public string BuisnessUnit { get; set; }
        public string DayClose { get; set; }

        public string ItmsGrpCo { get; set; }
        public string ItmsGrpNam { get; set; }
        public double? TotalValue { get; set; }



        public int CreditCard { get; set; }
        public string CardName { get; set; }
        public double? Amount { get; set; }



        public int SL { get; set; }
        public double? Currency { get; set; }
        public string TypeCode { get; set; }
        public string TypeCodeDesc { get; set; }
        public int NoofNotes { get; set; }
        public double? TotalAmount { get; set; }
        public string Remarks { get; set; }


        public string PostingDate { get; set; }
        public double? CashAmount { get; set; }
        public double? OthersAmount { get; set; }
        public double? ExtraCashAmount { get; set; }
        public List<CloseDayDetail> Items { get; set; }








      


    }
}