using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalculationService.Core.Enums;
using CalculationService.Core.Interfaces;
using CalculationService.Core.Strategies;

namespace CalculationService.Core.Factories {
    public static class InterestCalculationFactory {
        public static IInterestCalculationStrategy GetStrategy(InterestCalculationType type) {
            return type switch {
                InterestCalculationType.Simple => new SimpleInterestStrategy(),
                InterestCalculationType.Compound => new CompoundInterestStrategy(),
                _ => throw new ArgumentException("Неподдерживаемый тип расчета процентов")
            };
        }
    }
}
