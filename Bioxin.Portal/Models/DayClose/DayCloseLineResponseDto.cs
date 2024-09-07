

namespace WebSolution.Models.DayClose
{
    public class DayCloseLineResponseDto : DayCloseResponseDto
    {
        public string Currency { get; set; }
        public string CurrencyType { get; set; }
        public string NumberOfAmount { get; set; }
        public string TotalAmount { get; set; }
        public string LineRemarks { get; set; }
    }
}