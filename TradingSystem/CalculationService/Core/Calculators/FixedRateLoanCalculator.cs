using System.Linq;
using CalculationService.Core.Models;
using CalculationService.Core.Interfaces;
using CalculationService.Core.Enums;
using CalculationService.Core.Factories;
using CalculationService.Core.States;


namespace CalculationService.Core.Calculators {
    public class FixedRateLoanCalculator : ILoanCalculator {
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
                //TotalPayment = totalInterest,
                YearlyCalculations = yearlyCalculations
            };
        }
    }
}