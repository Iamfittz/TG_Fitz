using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TG_Fitz.Core;

namespace TelegramBot_Fitz.Core
{
    public class OISCalculationResult :ICalculationResult
    {
        public decimal TotalInterest { get; set; }
        public decimal TotalPayment { get; set; }
        public decimal DailyRate { get; set; }

    }
}
