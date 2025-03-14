using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramBot_Fitz.Bot;

namespace TelegramBot_Fitz.Core
{
    public interface ILoanCalculator
    {
        decimal CalculateInterest(UserState state);
    }
}
