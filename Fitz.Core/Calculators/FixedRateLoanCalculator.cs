using System.Linq;
using Fitz.Core.Models;
using Fitz.Core.Interfaces;
using Fitz.Core.Enums;
using Fitz.Core.Factories;
using Fitz.Core.States;


namespace TelegramBot_Fitz.Core {
    public class FixedRateLoanCalculator : ILoanCalculator {
        //public decimal CalculateInterest(UserState state) {
        //    var strategy = InterestCalculationFactory.GetStrategy(state.InterestCalculationType);
        //    return strategy.CalculateInterest(state.LoanAmount, state.YearlyRates.Average(), state.LoanYears);
        //}

        public LoanCalculationResult CalculateLoan(decimal loanAmount, decimal[] yearlyRates, InterestCalculationType calculationType) {
            var strategy = InterestCalculationFactory.GetStrategy(calculationType);
            var yearlyCalculations = new YearlyCalculation[yearlyRates.Length];
            decimal totalInterest = 0;
            decimal currentAmount = loanAmount;

            for (int i = 0; i < yearlyRates.Length; i++) {
                decimal baseAmount = calculationType == InterestCalculationType.Simple
                    ? loanAmount
                    : currentAmount;

                decimal yearlyInterest = strategy.CalculateInterest(baseAmount, yearlyRates[i], 1);

                if (calculationType == InterestCalculationType.Compound) {
                    currentAmount += yearlyInterest;
                }

                yearlyCalculations[i] = new YearlyCalculation {
                    Year = i + 1,
                    Rate = yearlyRates[i],
                    Interest = yearlyInterest,
                    AccumulatedAmount = calculationType == InterestCalculationType.Compound
                        ? currentAmount
                        : loanAmount + yearlyInterest
                };

                totalInterest += yearlyInterest;
            }


            return new LoanCalculationResult {
                TotalInterest = totalInterest,
                TotalPayment = loanAmount + totalInterest,
                YearlyCalculations = yearlyCalculations
            };
        }
    }
}