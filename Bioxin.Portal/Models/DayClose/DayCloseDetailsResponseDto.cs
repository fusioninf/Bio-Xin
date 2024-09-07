using System.Collections.Generic;

namespace WebSolution.Models.DayClose
{
    public class DayCloseDetailsResponseDto: DayCloseResponseDto
    {
        public List<DayCloseLineResponseDto> Lines { get; set; }
        public List<BankReceiveResponseDto> Banks { get; set; }
        public List<ReceivedResponseDto> Receiveds { get; set; }
    }
}