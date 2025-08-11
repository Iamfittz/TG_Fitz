using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalculationService.Core.Interfaces;
using CalculationService.Core.States;
using CalculationService.Core.Models;

namespace CalculationService.Core.Interfaces {
    public interface IFloatingRateCalculator : ILoanCalculator {
        List<PeriodInterestDetail> GetInterestBreakdown(UserState state);
        decimal CalculateTotalInterest(UserState state);
    }
}
