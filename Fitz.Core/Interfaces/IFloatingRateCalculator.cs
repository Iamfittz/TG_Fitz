using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fitz.Core.Interfaces;
using Fitz.Core.States;
using Fitz.Core.Models;

namespace Fitz.Core.Interfaces {
    public interface IFloatingRateCalculator : ILoanCalculator {
        List<PeriodInterestDetail> GetInterestBreakdown(UserState state);
        decimal CalculateTotalInterest(UserState state);
    }
}
