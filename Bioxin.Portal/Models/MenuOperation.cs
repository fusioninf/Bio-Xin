using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebSolution.Models
{
    public class MenuOperation
    {
        public int SL { get; set; }

        public string MenuName { get; set; }

        public int? ParantId { get; set; }

        public string ParantMenu { get; set; }

        public int MenuStep { get; set; }

        public string MenuStepName { get; set; }


        public string ControllerName { get; set; }

        public string ActionName { get; set; }

        public int IsActive { get; set; }


    }
}