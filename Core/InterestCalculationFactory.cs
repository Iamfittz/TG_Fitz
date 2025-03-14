using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBot_Fitz.Core
{
    public static class InterestCalculationFactory
    {
        public static IInterestCalculationStrategy GetStrategy(InterestCalculationType type)
        {
            return type switch
            {
                InterestCalculationType.Simple => new SimpleInterestStrategy(),
                InterestCalculationType.Compound => new CompoundInterestStrategy(),
                _ => throw new ArgumentException("Неподдерживаемый тип расчета процентов")
            };
        }
    }
}
