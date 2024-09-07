using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebSolution.Models
{
    public class SalesOrder
    {
        [Key]
        public int DocEntry { get; set; }
        public int DocNum { get; set; }
        public string DocStatus { get; set; }
        public string CardCode { get; set; }
        public string CardName { get; set; }
        public int ContactPersonCode { get; set; }
        public string ContactPersonName { get; set; }
        //Customer Reference No
        public string NumAtCard { get; set; }
        public string DocCurrency { get; set; }
        public DateTime DocDate { get; set; }
        public DateTime DocDueDate { get; set; }
        //Place of Supply
        public string ShipPlace { get; set; }
        public decimal DocTotal { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal VatPercent { get; set; }
        public decimal VatSum { get; set; }
        public string ShipToCode { get; set; }
        public string PayToCode { get; set; }
        //Bill To
        public string Address { get; set; }
        //Ship To
        public string Address2 { get; set; }
        public int TransportationCode { get; set; }
        public int SalesPersonCode { get; set; }
        public int PaymentGroupCode { get; set; }
        public string Comments { get; set; }

        public List<SalesOrderLine> SalesOrderLines { get; set; }

    }
}