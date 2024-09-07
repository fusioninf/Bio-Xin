using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebSolution.Models
{
    public class ItemsViewModel
    {
        public int DocEntry { get; set; }
        public int VisOrder { get; set; }
        public int LineNum { get; set; }
        public string ObjType { get; set; }
        public string LineStatus { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string unitMsr { get; set; }
        public string Remarks { get; set; }
        public decimal Quantity { get; set; }
        public decimal OpenQty { get; set; }
        public decimal Price { get; set; }
        public decimal PaidAmount { get; set; }
        public string FromWhsCode { get; set; }
        public string FromWhsName { get; set; }
        public string FromBranchCode { get; set; }
        public string FromBranchName { get; set; }
        //Inventory Transfer
        public string ToBranchCode { get; set; }
        public string ToWhsName { get; set; }
        public string ToBranchCode1 { get; set; }
        public string ToBranchName { get; set; }
        //Inventory Transfer
        //Goods Issue
        public string DepartmentCode { get; set; }
        public string DepartmentName { get; set; }
        public string EmployeeCode { get; set; }
        public string EmployeeName { get; set; }
        public string SlpName { get; set; }
        public string SlpCode { get; set; }
        //Good Issue
        //Goods Receipt
        public string WhsCode { get; set; }
        public string WhsName { get; set; }
        //Goods Receipt
        public string Days { get; set; }
        public string TimesPerDay { get; set; }
        public string Dinner { get; set; }
        public string DinnerDesc { get; set; }
        public string Lunch { get; set; }
        public string LunchDesc { get; set; }
        public string Breakfast { get; set; }
        public string BreakFastDesc { get; set; }
        public string BranchCode { get; set; }
        public string BranchName { get; set; }
        public string TaxCode { get; set; }
        public string TaxRate { get; set; }
        public string DiscountAmount { get; set; }
        public string ItemTypeCode { get; set; }
        public string ItemTypeDesc { get; set; }

        public string RefNo { get; set; }
        public string DiscountPercentage { get; set; }
        public string DeliveryDate { get; set; }
        public string BatchNumberEnable { get; set; }
        public string SerialNumberEnable { get; set; }
        public string Stock { get; set; }
        public string DocNum { get; set; }
        public string SeriesName { get; set; }
        public string SAPDocNum { get; set; }
        public string CardCode { get; set; }
        public string CardName { get; set; }
        public string Mobile { get; set; }
        public string DocStatus { get; set; }
        public string DocStatusDesc { get; set; }
        public string DocDate { get; set; }
        public string DocDueDate { get; set; }
        public string TaxDate { get; set; }
        public string CreatedBy { get; set; }
        public int SequenceNo { get; set; }
        public string Details { get; set; }
        public string Series { get; set; }
        public int UnAutorized { get; set; }
        public string ReturnCode { get; set; }
        public string ItemGroup { get; set; }

        public List<BatchesViewModel> BatchDetails { get; set; }
    }
}