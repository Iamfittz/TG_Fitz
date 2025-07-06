using Fitz.Core.Enums;
using TelegramBot_Fitz.Core;

namespace Fitz.Core.Tests {
    public class FixedRateLoanCalculatorTests {
        [Fact]
        public void CalculateLoan_ShouldReturnCorrectTotalPayment() {
            // Arrange
            var calculator = new FixedRateLoanCalculator();
            decimal loanAmount = 1000m;
            decimal[] yearlyRates = { 5, 10 }; // 5% in 1st year, 10 in second 
            //1st 1000*5% = 50
            //Second 1000 * 10% =100
            // Total = 1000+50+100=1150
            var calculationType = InterestCalculationType.Simple;
            // Act
            var result = calculator.CalculateLoan(loanAmount, yearlyRates, calculationType);
            // Assert
            Assert.Equal(150, result.TotalInterest);
            Assert.Equal(1150, result.TotalPayment);

            Assert.Equal(2, result.YearlyCalculations.Length);
            Assert.Equal(50, result.YearlyCalculations[0].Interest);
            Assert.Equal(100, result.YearlyCalculations[1].Interest);
        }

    }
}