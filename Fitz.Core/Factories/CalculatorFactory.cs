using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fitz.Core.Interfaces;
using TelegramBot_Fitz.Core;
using Fitz.Core.Enums;

namespace Fitz.Core.Factories {
    public class CalculatorFactory {
        public static ILoanCalculator GetCalculator(CalculationType type) {
            return type switch {
                CalculationType.FixedRate => new FixedRateLoanCalculator(),
                CalculationType.FloatingRate => new FloatingRateLoanCalculator(),
                CalculationType.OIS => new OISCalculator(),
                _ => throw new ArgumentException("Unsupported calculation type")
            };
        }
    }
}