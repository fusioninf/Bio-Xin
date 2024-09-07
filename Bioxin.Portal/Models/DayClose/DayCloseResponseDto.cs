using System;

namespace WebSolution.Models.DayClose
{
    public class DayCloseResponseDto
    {
        public long DocEntry { get; set; }
        public string Status { get; set; }
        public string Code { get; set; }
        public string DayCloseDate { get; set; }
        public int BranchId { get; set; }
        public string BranchName { get; set; }
        public decimal CashAmount { get; set; }
        public decimal OtherAmount { get; set; }
        public decimal ExtraCash { get; set; }
        public string Remarks { get; set; }
    }
}