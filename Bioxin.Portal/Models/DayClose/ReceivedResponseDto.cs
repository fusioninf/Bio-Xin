using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebSolution.Models.DayClose
{
    public class ReceivedResponseDto
    {
        public string ItmsGrpCo { get; set; }
        public string ItmsGrpNam { get; set; }
        public decimal TotalValue { get; set; }
    }
}