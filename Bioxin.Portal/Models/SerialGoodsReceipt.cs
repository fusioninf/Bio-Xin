﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebSolution.Models
{
    public class SerialGoodsReceipt
    {
        public int VisOrder { get; set; }
        public string InternalSerialNumber { get; set; }
        public string ManufacturerSerialNumber { get; set; }
    }
}