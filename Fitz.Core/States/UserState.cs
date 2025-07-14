using TelegramBot_Fitz.Core;
using TG_Fitz.Core;
using Fitz.Core.Enums;

namespace Fitz.Core.States {

    public class UserState {
        public int Step { get; set; } = 0;
        public decimal LoanAmount { get; set; }
        public int LoanYears { get; set; }
        public int Days { get; set; }
        public string? CompanyName { get; set; }
        public decimal[] YearlyRates { get; set; } = Array.Empty<decimal>(); // Гарантированно не null
        public int CurrentYear { get; set; }
        public decimal FirstRate { get; set; }
        public decimal SecondRate { get; set; }
        public CalculationType CalculationType { get; set; } = CalculationType.None; // Тип расчета (Fixed или Floating)
        public InterestCalculationType InterestCalculationType { get; set; }
        public FloatingRateResetPeriod FloatingRateResetPeriod { get; set; } = new();
        public int CurrentFloatingPeriod { get; set; } = 1;
        public List<decimal> FloatingRates { get; set; } = new();
        public DayCountConvention DayCountConvention { get; set; } = DayCountConvention.Actual360;
        public int TotalFloatingPeriods {
            get {
                int months = LoanYears * 12;
                return months / (int)FloatingRateResetPeriod;
            }
        }
        public void InitilizeYearlyRates() {
            YearlyRates = new decimal[LoanYears];
        }
        public void Reset() {
            Step = 0;
            LoanAmount = 0;
            LoanYears = 0;
            Days = 0;
            YearlyRates = Array.Empty<decimal>(); // Теперь не null
            CurrentYear = 0;
            FirstRate = 0;
            SecondRate = 0;
            CalculationType = CalculationType.None; // сбрасываем тип расчета
            CompanyName = null;
        }
    }
}
