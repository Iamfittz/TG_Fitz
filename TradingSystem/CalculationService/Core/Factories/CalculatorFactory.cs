using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalculationService.Core.Interfaces;
using CalculationService.Core.Calculators;
using CalculationService.Core.Enums;

namespace CalculationService.Core.Factories {
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