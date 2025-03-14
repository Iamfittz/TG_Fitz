using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBot_Fitz.Core
{
    public class LoanCalculationResult
    {
        public decimal TotalInterest { get; set; }
        public decimal TotalPayment { get; set; }

        public YearlyCalculation[] YearlyCalculations { get; set; } = Array.Empty<YearlyCalculation>(); // Теперь не null
    }
}
