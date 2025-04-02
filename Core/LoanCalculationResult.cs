using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TG_Fitz.Core;

namespace TelegramBot_Fitz.Core
{
    public class LoanCalculationResult : ICalculationResult
    {
        public decimal TotalInterest { get; set; }
        public decimal TotalPayment { get; set; }

        public YearlyCalculation[] YearlyCalculations { get; set; } = Array.Empty<YearlyCalculation>(); // Теперь не null
    }
}
