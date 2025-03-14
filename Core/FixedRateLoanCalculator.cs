using System.Linq;
using TelegramBot_Fitz.Bot;

namespace TelegramBot_Fitz.Core
{
    public class FixedRateLoanCalculator : ILoanCalculator
    {
        public decimal CalculateInterest(UserState state)
        {
            var strategy = InterestCalculationFactory.GetStrategy(state.InterestCalculationType);
            return strategy.CalculateInterest(state.LoanAmount, state.YearlyRates.Average(), state.LoanYears);
        }

        public LoanCalculationResult CalculateLoan(decimal loanAmount, decimal[] yearlyRates, InterestCalculationType calculationType)
        {
            var strategy = InterestCalculationFactory.GetStrategy(calculationType);
            var yearlyCalculations = new YearlyCalculation[yearlyRates.Length];
            decimal totalInterest = 0;
            decimal currentAmount = loanAmount;

            for (int i = 0; i < yearlyRates.Length; i++)
            {
                decimal yearlyInterest = strategy.CalculateInterest(currentAmount, yearlyRates[i], 1);
                currentAmount += yearlyInterest;

                yearlyCalculations[i] = new YearlyCalculation
                {
                    Year = i + 1,
                    Rate = yearlyRates[i],
                    Interest = yearlyInterest,
                    AccumulatedAmount = currentAmount
                };

                totalInterest += yearlyInterest;
            }

            return new LoanCalculationResult
            {
                TotalInterest = totalInterest,
                TotalPayment = loanAmount + totalInterest,
                YearlyCalculations = yearlyCalculations
            };
        }


    }
}