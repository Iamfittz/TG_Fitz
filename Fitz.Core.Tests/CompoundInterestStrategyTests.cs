using Fitz.Core.Enums;
using Fitz.Core.Interfaces;
using Fitz.Core.Strategies;
using TelegramBot_Fitz.Core;

namespace Fitz.Core.Tests {
    public class CompoundInterestStrategyTests {
        [Fact]
        public void CalculateInterest_With10000AmountAnd5PercentFor2Years_Returns102_5m() {
            // Arrange
            var strategy = new CompoundInterestStrategy();
            decimal expected = 102.5m;
            // Act
            decimal actual = strategy.CalculateInterest(1000, 5, 2);
            // Assert
            Assert.Equal(expected, actual);
        }
        [Fact]
        public void CalculateInterest_With5000AmountAnd7_5PercentFor3Years_Returns6211_48m() {
            //Arrange
            var strategy = new CompoundInterestStrategy();
            //5000+375 = 5375 + 403.125 = 5778.125 = 433.359375
            decimal expected = 1211.48m;
            //Act
            decimal actual = strategy.CalculateInterest(5000, 7.5m, 3);
            //Assert
            Assert.Equal(expected, Math.Round( actual,2));
        }
        [Fact]
        public void CalculateInterest_WithZeroAmountAndZeroPercentForZeroYears_Returns() {
            //Arrange
            var strategy = new CompoundInterestStrategy();
            //Act
            decimal actual = strategy.CalculateInterest(0, 0m, 0);
            //Assert
            Assert.Equal(0, actual);
        }
        [Fact]
        public void CalculateInterest_With0_1AmountAnd0_1PercentFor3Years_Returns0_0003() {
            //0.1+0.0001
            //0.1001+0.0001001=0.102001
            //0.102001+0.0001002001=0.1021012001

            //Arrange
            var strategy = new CompoundInterestStrategy();
            decimal expected = 0.0003003001m;
            decimal tolerance = 0.00000001m;
            //Act
            decimal actual = strategy.CalculateInterest(0.1m, 0.1m, 3);
            //Assert
            Assert.True(Math.Abs(actual-expected)<tolerance);
        }
        [Fact]
        public void CalculateInterest_With_NegativeAmount_NegativeRate_NegativePeriod_ThrowsArgumetnException() {
            var strategy = new CompoundInterestStrategy();

            Assert.Throws<ArgumentException>(() =>
            strategy.CalculateInterest(-1000, -5, -2)
            );
        }
        [Fact]
        public void CalculateInterest_With_1_000_000_000_Amount_5PercentRate_For10YearsPeriod() {
            var strategy = new CompoundInterestStrategy();
            decimal expected = 628894626.77744m;
            decimal actual = strategy.CalculateInterest(1000000000m, 5, 10);
            decimal tolerance = 0.00000001m;
            Assert.True(Math.Abs(actual-expected)<tolerance);
        }

    }
}