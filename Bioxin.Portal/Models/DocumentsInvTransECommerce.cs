using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebSolution.Models
{
    public class DocumentsInvTransECommerce
    {
        public int TotalRows { get; set; }
        public int DocEntry { get; set; }
        public string DocNum { get; set; }
        public string SeriesName { get; set; }
        public string SAPDocNum { get; set; }
        public string DocStatus { get; set; }
        public string DocStatusDesc { get; set; }
        public string CardCode { get; set; }
        public string CardName { get; set; }
        public string DocDate { get; set; }
        public string DocDueDate { get; set; }
        public string TaxDate { get; set; }
        public string ShipToCode { get; set; }
        public string FromWhsCode { get; set; }
        public string FromWhsName { get; set; }
        public string FromBranchCode { get; set; }
        public string FromBranchName { get; set; }
        public string ToWhsCode { get; set; }
        public string ToWhsName { get; set; }
        public string ToBranchCode { get; set; }
        public string ToBranchName { get; set; }
        public string ContactPersonCode { get; set; }
        public string SalesEmployeeCode { get; set; }
        public string CreatedBy { get; set; }
        public string Remarks { get; set; }
        public string TrackingID { get; set; }
        public string CourierCompany { get; set; }
        public string StatusCode { get; set; }
        public string StatusDesc { get; set; }
        public string DelChannelCode { get; set; }
        public string DelChannelDesc { get; set; }

        public string DelAgentCode { get; set; }
        public string DelAgentDesc { get; set; }
        public string Mobile { get; set; }
        public string AreaCode { get; set; }
        public string AreaName { get; set; }
        public decimal TotalValue { get; set; }
        public string CnfDate { get; set; }
        public string Remarks1 { get; set; }
        public string SODocNum { get; set; }
        public string SODocDate { get; set; }
        public string SOCardCode { get; set; }
        public string SOCardName { get; set; }
        public int SODocEntry { get; set; }

        public string RefNo { get; set; }
        public string DeliveryAddress { get; set; }
        public int UnAutorized { get; set; }
        public string ReturnCode { get; set; }


    }
}