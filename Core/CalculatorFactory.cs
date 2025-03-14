using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramBot_Fitz.Bot;

namespace TelegramBot_Fitz.Core
{
    public class CalculatorFactory
    {
        public static ILoanCalculator GetCalculator(CalculationType type)
        {
            return type switch
            {
                CalculationType.FixedRate => new FixedRateLoanCalculator(),
                CalculationType.FloatingRate => new FloatingRateLoanCalculator(),
                CalculationType.OIS => new OISCalculator(),
                _ => throw new ArgumentException("Неподдерживаемый тип расчета")

            };
        }
    }
}
