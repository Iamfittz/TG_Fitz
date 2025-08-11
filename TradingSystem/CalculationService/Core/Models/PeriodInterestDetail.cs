using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TG_Fitz.Core;
using CalculationService.Core.Interfaces;
using CalculationService.Core.Enums;


namespace CalculationService.Core.Models {
    public class PeriodInterestDetail {
        public int PeriodNumber { get; set; }
        public decimal Rate { get; set; }
        public decimal Interest { get; set; }
    }
}
