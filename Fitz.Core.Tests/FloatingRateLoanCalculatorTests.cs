using CalculationService.Core.Enums;
using CalculationService.Core.Interfaces;
using CalculationService.Core.States;
using CalculationService.Core.Strategies;
using CalculationService.Core.Calculators;
using CalculationService.Core;
using TG_Fitz.Core;
using Xunit;

namespace Fitz.Core.Tests {
    public class FloatingRateLoanCalculatorTests {
        [Fact]
        public void GetInterestBreakdown_WithTwelweRates_ShouldReturnCorrectResults() {
            //Arrange
            var state = new UserState {
                LoanAmount = 150000,
                FloatingRates = new List<decimal> { 0.02m, 0.97m, 1.9954m, 2.0564m, 3.0018m, 4.1234m, 5.0890m, 6.0156m, 7.1999m, 8.0008m, 9.6701m, 10.1435m },// 12 periods
                FloatingRateResetPeriod = FloatingRateResetPeriod.OneMonth
            };
            var calculator = new FloatingRateLoanCalculator();

            //Act
            var breakdown = calculator.GetInterestBreakdown(state);
            decimal totalInterest = breakdown.Sum(s => s.Interest);

            //Assert
            Assert.Equal(12, breakdown.Count);

            //15000 * 0.02% * (1/12) = 2.50
            Assert.Equal(1, breakdown[0].PeriodNumber);
            Assert.Equal(0.02m, breakdown[0].Rate);
            Assert.Equal(2.50m, Math.Round(breakdown[0].Interest,2));

            //15000 * 0.97% * (1/12) = 121.25
            Assert.Equal(2, breakdown[1].PeriodNumber);
            Assert.Equal(0.97m, breakdown[1].Rate);
            Assert.Equal(121.25m, Math.Round(breakdown[1].Interest, 2));

            //15000 * 1.9954% * (1/12) = 249.42
            Assert.Equal(3, breakdown[2].PeriodNumber);
            Assert.Equal(1.9954m, breakdown[2].Rate);
            Assert.Equal(249.42m, Math.Round(breakdown[2].Interest, 2));

            //15000 * 2.0564% * (1/12) = 257.05
            Assert.Equal(4, breakdown[3].PeriodNumber);
            Assert.Equal(2.0564m, breakdown[3].Rate);
            Assert.Equal(257.05m, Math.Round(breakdown[3].Interest, 2));

            //15000 * 3.0018% * (1/12) = 375.22
            Assert.Equal(5, breakdown[4].PeriodNumber);
            Assert.Equal(3.0018m, breakdown[4].Rate);
            Assert.Equal(375.22m, Math.Round(breakdown[4].Interest, 2));

            //15000 * 4.1234% * (1/12) = 515.42
            Assert.Equal(6, breakdown[5].PeriodNumber);
            Assert.Equal(4.1234m, breakdown[5].Rate);
            Assert.Equal(515.42m, Math.Round(breakdown[5].Interest, 2));

            //15000 * 5.0890% * (1/12) = 636.12
            Assert.Equal(7, breakdown[6].PeriodNumber);
            Assert.Equal(5.0890m, breakdown[6].Rate);
            Assert.Equal(636.12m, Math.Round(breakdown[6].Interest, 2));

            //15000 * 6.0156% * (1/12) = 751.95
            Assert.Equal(8, breakdown[7].PeriodNumber);
            Assert.Equal(6.0156m, breakdown[7].Rate);
            Assert.Equal(751.95m, Math.Round(breakdown[7].Interest, 2));

            //15000 * 7.1999% * (1/12) = 899.99
            Assert.Equal(9, breakdown[8].PeriodNumber);
            Assert.Equal(7.1999m, breakdown[8].Rate);
            Assert.Equal(899.99m, Math.Round(breakdown[8].Interest, 2));

            //15000 * 8.0008% * (1/12) = 1000.10
            Assert.Equal(10, breakdown[9].PeriodNumber);
            Assert.Equal(8.0008m, breakdown[9].Rate);
            Assert.Equal(1000.10m, Math.Round(breakdown[9].Interest, 2));

            //15000 * 9.6701% * (1/12) = 1208.76
            Assert.Equal(11, breakdown[10].PeriodNumber);
            Assert.Equal(9.6701m, breakdown[10].Rate);
            Assert.Equal(1208.76m, Math.Round(breakdown[10].Interest, 2));

            //15000 * 10.1435% * (1/12) = 1267.94
            Assert.Equal(12, breakdown[11].PeriodNumber);
            Assert.Equal(10.1435m, breakdown[11].Rate);
            Assert.Equal(1267.94m, Math.Round(breakdown[11].Interest, 2));

            //Total
            Assert.Equal(7285.74m, Math.Round(totalInterest,2));

        }
        [Fact]
        public void GetInterestBreakdown_WithFourRates_ShouldReturnCorrectResults() {
            //Arrange
            var state = new UserState {
                LoanAmount = 10000,
                FloatingRates = new List<decimal> { 5, 4.2m, 4.8m, 11.02m },
                FloatingRateResetPeriod = (CalculationService.Core.Enums.FloatingRateResetPeriod)3 // each 3 months
            };
            var calculator = new FloatingRateLoanCalculator();

            //Act
            var breakdown = calculator.GetInterestBreakdown(state);
            decimal totalInterest = breakdown.Sum(s => s.Interest);

            //Assert
            Assert.Equal(4, breakdown.Count);

            //10000 * 5% * 0.25 = 125
            Assert.Equal(1, breakdown[0].PeriodNumber);
            Assert.Equal(5, breakdown[0].Rate);
            Assert.Equal(125, breakdown[0].Interest);

            //10000 * 4.2% * 0.25 = 105
            Assert.Equal(2, breakdown[1].PeriodNumber);
            Assert.Equal(4.2m, breakdown[1].Rate);
            Assert.Equal(105, breakdown[1].Interest);

            //10000 * 4.8% * 0.25 = 120
            Assert.Equal(3, breakdown[2].PeriodNumber);
            Assert.Equal(4.8m, breakdown[2].Rate);
            Assert.Equal(120, breakdown[2].Interest);

            //10000 * 11.02% * 0.25 = 275.50
            Assert.Equal(4, breakdown[3].PeriodNumber);
            Assert.Equal(11.02m, breakdown[3].Rate);
            Assert.Equal(275.50m, breakdown[3].Interest);
        }
        [Fact]
        public void GetInterestBreakdown_WithTwoRates_ShouldReturnCorrectResults() {
            // Arrange
            var state = new UserState {
                LoanAmount = 1000,
                FloatingRates = new List<decimal> { 4, 6 }, // 2 periods: 4%, 6%
                FloatingRateResetPeriod = (CalculationService.Core.Enums.FloatingRateResetPeriod)6 // each 6 months
            };

            var calculator = new FloatingRateLoanCalculator();

            // Act
            var breakdown = calculator.GetInterestBreakdown(state);
            decimal totalInterest = breakdown.Sum(s => s.Interest);

            // Assert
            Assert.Equal(2, breakdown.Count);

            // 1000 * 4% * 0.5 = 20
            Assert.Equal(1, breakdown[0].PeriodNumber);
            Assert.Equal(4, breakdown[0].Rate);
            Assert.Equal(20, breakdown[0].Interest);

            // 1000 * 6% * 0.5 = 30
            Assert.Equal(2, breakdown[1].PeriodNumber);
            Assert.Equal(6, breakdown[1].Rate);
            Assert.Equal(30, breakdown[1].Interest);

            // Total = 20 + 30 = 50
            Assert.Equal(50, totalInterest);
        }
    }
}
