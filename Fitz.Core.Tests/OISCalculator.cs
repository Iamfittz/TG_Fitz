using Fitz.Core.Enums;
using Fitz.Core.Interfaces;
using Fitz.Core.States;
using Fitz.Core.Strategies;
using TelegramBot_Fitz.Core;

namespace Fitz.Core.Tests {
    public class OISCalculatorTests {
        [Fact]
        public void CalculateOIS_WithActual360_ReturnsCorrectResult() {
            // Arrange
            var calculator = new OISCalculator();
            var state = new UserState {
                LoanAmount = 100000m,
                FirstRate = 5,
                Days = 30
            };
            var convention = DayCountConvention.Actual360;
            // Act
            var actual = calculator.CalculateOIS(state, convention);
            // Assert
            decimal expectedInterest = 100000 * ((5.0m / 360) / 100) * 30; // 41.67
            Assert.Equal(expectedInterest, actual.TotalInterest, precision: 2);
        }
        [Fact]
        public void CalculateOIS_WithActual365_ReturnsCorrectResult() {
            //Arrange
            var calculator = new OISCalculator();
            var state = new UserState {
                LoanAmount = 100000m,
                FirstRate = 5,
                Days = 30
            };
            var convention = DayCountConvention.Actual365;
            //Act
            var actual = calculator.CalculateOIS(state, convention);
            //Assert
            decimal expectedInterest = 100000 * ((5.0m / 365) / 100) * 30; // ≈ 41.10
            Assert.Equal(expectedInterest, actual.TotalInterest, precision: 2);
        }
        [Fact]
        public void CalculateOIS_WithActualActual_ReturnsCorrectResult() {
            var calculator = new OISCalculator();
            var state = new UserState {
                LoanAmount = 100000m,
                FirstRate = 5,
                Days = 30
            };
            var convention = DayCountConvention.ActualActual;

            var actual = calculator.CalculateOIS(state, convention);

            int year = DateTime.Now.Year;
            decimal denominator = DateTime.IsLeapYear(year) ? 366 : 365;
            decimal expectedInterest = 100000 * ((5.0m / denominator) / 100) * 30;

            Assert.Equal(expectedInterest, actual.TotalInterest, precision: 2);
        }
        [Fact]
        public void CalculateOIS_WithThirty360_ReturnsCorrectResult() {
            var calculator = new OISCalculator();
            var state = new UserState {
                LoanAmount = 100000m,
                FirstRate = 5,
                Days = 30
            };
            var convention = DayCountConvention.Thirty360;

            var actual = calculator.CalculateOIS(state, convention);

            decimal expectedInterest = 100000 * ((5.0m / 360) / 100) * 30; // такой же, как Actual/360
            Assert.Equal(expectedInterest, actual.TotalInterest, precision: 2);
        }
    }
}