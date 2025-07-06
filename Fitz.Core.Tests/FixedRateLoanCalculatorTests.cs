using Fitz.Core.Enums;
using TelegramBot_Fitz.Core;

namespace Fitz.Core.Tests {
    public class CalculateLoanTests {
        [Fact]
        public void CalculateLoan_ShouldReturnCorrectTotalPayment() {
            // Arrange
            var calculator = new FixedRateLoanCalculator();
            decimal loanAmount = 100_000m;
            decimal[] yearlyRates = Enumerable.Repeat(6.0m, 15).ToArray(); 
            var calculationType = InterestCalculationType.Simple;

            // Act
            var result = calculator.CalculateLoan(loanAmount, yearlyRates, calculationType);

            // Assert
            decimal expectedInterest = loanAmount * 0.06m * 15; // простые проценты
            decimal expectedTotalPayment = loanAmount + expectedInterest;

            Assert.Equal(expectedTotalPayment, result.TotalPayment);
        }

    }
}