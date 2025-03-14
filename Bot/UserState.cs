using TelegramBot_Fitz.Core;

namespace TelegramBot_Fitz.Bot
{
    public enum CalculationType
    {
        None,
        FixedRate,
        FloatingRate,
        OIS
    }

    public class UserState
    {
        public int Step { get; set; } = 0;
        public decimal LoanAmount { get; set; }
        public int LoanYears { get; set; }
        public int Days { get; set; }
        
        public decimal[] YearlyRates { get; set; } = Array.Empty<decimal>(); // Гарантированно не null
        public int CurrentYear { get; set; }
        public decimal FirstRate { get; set; }
        public decimal SecondRate { get; set; }
        public CalculationType CalculationType { get; set; } = CalculationType.None; // Тип расчета (Fixed или Floating)
        public InterestCalculationType InterestCalculationType { get; set; }

        public void InitilizeYearlyRates()
        {
            YearlyRates = new decimal[LoanYears];
        }
        public void Reset()
        {
            Step = 0;
            LoanAmount = 0;
            LoanYears = 0;
            Days = 0;
            YearlyRates = Array.Empty<decimal>(); // Теперь не null
            CurrentYear = 0;
            FirstRate = 0;
            SecondRate = 0;
            CalculationType = CalculationType.None; // сбрасываем тип расчета

        }
    }
}
