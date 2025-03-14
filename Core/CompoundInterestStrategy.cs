using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBot_Fitz.Core
{
    public class CompoundInterestStrategy : IInterestCalculationStrategy
    {
        public decimal CalculateInterest(decimal amount, decimal rate, int period)
        {
            return amount * (decimal)Math.Pow((double)(1 + rate / 100), period) - amount;
        }
    }
}
