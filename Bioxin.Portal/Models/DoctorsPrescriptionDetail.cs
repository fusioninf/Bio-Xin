using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebSolution.Models
{
    public class DoctorsPrescriptionDetail
    {
        public string ItemCode { get; set; }
        public decimal? Quantity { get; set; }
        public decimal Price { get; set; }
        public string UOM { get; set; }
        public string Dinner { get; set; }
        public string Lunch { get; set; }
        public string Breakfast { get; set; }
        public string Days { get; set; }
        public string TimesPerDay { get; set; }
        public string DinnerDesc { get; set; }
        public string LunchDesc { get; set; }
        public string BreakFastDesc { get; set; }
        public string Day { get; set; }
        public string Details { get; set; }
    }
}