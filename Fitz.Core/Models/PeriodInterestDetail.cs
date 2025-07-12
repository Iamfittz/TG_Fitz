using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TG_Fitz.Core;
using Fitz.Core.Interfaces;
using Fitz.Core.Enums;


namespace Fitz.Core.Models {
    public class PeriodInterestDetail {
        public int PeriodNumber { get; set; }
        public decimal Rate { get; set; }
        public decimal Interest { get; set; }
    }
}
