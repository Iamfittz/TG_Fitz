using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalculationService.Core.Interfaces;


namespace CalculationService.Core.Strategies {
    public class SimpleInterestStrategy : IInterestCalculationStrategy {
        public decimal CalculateInterest(decimal amount, decimal rate, int period) {
            if (amount < 0 || rate < 0 || period < 0) {
                throw new ArgumentException("Values must be positive");
            }
            return amount * (rate / 100) * period;
        }
    }
}
