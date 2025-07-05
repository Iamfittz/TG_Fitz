using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fitz.Core.Interfaces;


namespace Fitz.Core.Strategies {
    public class SimpleInterestStrategy : IInterestCalculationStrategy {
        public decimal CalculateInterest(decimal amount, decimal rate, int period) {
            return amount * (rate / 100) * period;
        }
    }
}
