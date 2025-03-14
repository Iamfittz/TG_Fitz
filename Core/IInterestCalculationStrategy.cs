using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBot_Fitz.Core
{
    public interface IInterestCalculationStrategy
    {
        decimal CalculateInterest(decimal amount, decimal rate, int period);
    }
}
