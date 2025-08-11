using CalculationService.Core.Enums;
using CalculationService.Core.Strategies;


namespace Fitz.Core.Tests {
    public class SimpleInterestStrategyTests {
        [Fact]
        public void CalculateInterest_With1000AmountAnd5PercentFor2Years_Returns100() {
            // Arrange
            var strategy = new SimpleInterestStrategy();
            decimal expected = 100;
            // Act
            decimal actual = strategy.CalculateInterest(1000, 5, 2);
            // Assert
            Assert.Equal(expected, actual);
        }
        [Fact]
        public void CalculateInterest_With5000AmountAnd7_5PercentFor3Years_Returns1125() {
            //Arrange
            var strategy = new SimpleInterestStrategy();
            decimal expected = 1125;
            //Act
            decimal actual = strategy.CalculateInterest(5000, 7.5m, 3);
            //Assert
            Assert.Equal(expected, actual);
        }
        [Fact]
        public void CalculateInterest_WithZeroAmountAndZeroPercentForZeroYears_Returns() {
            //Arrange
            var strategy = new SimpleInterestStrategy();
            //Act
            decimal actual = strategy.CalculateInterest(0, 0m, 0);
            //Assert
            Assert.Equal(0, actual);
        }
        [Fact]
        public void CalculateInterest_With0_1AmountAnd0_1PercentFor3Years_Returns0_0003() {
            //Arrange
            var strategy = new SimpleInterestStrategy();
            //Act
            decimal actual = strategy.CalculateInterest(0.1m, 0.1m, 3);
            //Assert
            Assert.Equal(0.0003m, actual);
        }
        [Fact]
        public void CalculateInterest_With_NegativeAmount_NegativeRate_NegativePeriod_ThrowsArgumetnException() {
            var strategy = new SimpleInterestStrategy();

            Assert.Throws<ArgumentException>(() =>
            strategy.CalculateInterest(-1000,-5,-2)
            );
        }
        [Fact]
        public void CalculateInterest_With_1_000_000_000_Amount_5PercentRate_For10YearsPeriod() {
            var strategy = new SimpleInterestStrategy();
            decimal expected = 500000000;
            decimal actual = strategy.CalculateInterest(1000000000m, 5, 10);

            Assert.Equal(expected, actual);
        }
        [Theory]
        [InlineData(1000,5,2,100)]
        [InlineData(2000,3.5,1,70)]
        public void CalculateInterest_ValidInputs_ReturnsExpected(decimal amount, decimal rate, int period, decimal expected) {
            var strategy = new SimpleInterestStrategy();
            decimal actual = strategy.CalculateInterest(amount, rate, period);
            Assert.Equal(expected, actual);
        }
    }
}