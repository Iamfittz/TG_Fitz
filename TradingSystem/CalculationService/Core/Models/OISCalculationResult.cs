using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TG_Fitz.Core;
using CalculationService.Core.Interfaces;
using CalculationService.Core.Models;

namespace CalculationService.Core.Models {
    public class OISCalculationResult : ICalculationResult {
        public decimal TotalInterest { get; set; }
        public decimal TotalPayment { get; set; }
        public decimal DailyRate { get; set; }

    }
}
