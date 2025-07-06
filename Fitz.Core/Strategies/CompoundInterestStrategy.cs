using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fitz.Core.Interfaces;


namespace Fitz.Core.Strategies {
    public class CompoundInterestStrategy : IInterestCalculationStrategy {
        public decimal CalculateInterest(decimal amount, decimal rate, int period) {
            if (amount < 0 || rate < 0|| period < 0) {
                throw new ArgumentException("Values nust be positive");
            }
            return amount * (decimal)Math.Pow((double)(1 + rate / 100), period) - amount;
        }
    }
}
