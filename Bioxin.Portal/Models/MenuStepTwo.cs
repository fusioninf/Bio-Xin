﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebSolution.Models
{
    public class MenuStepTwo
    {
        public int SL { get; set; }
        public string MenuName { get; set; }
        public string ControllerName { get; set; }
        public string ActionName { get; set; }
        public string ParantMenu { get; set; }
        //public List<MenuStepThree> MenuStepThreeList { get; set; }
    }
}