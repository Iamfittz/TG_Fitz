using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fitz.Core.States;
using Fitz.Core.Interfaces;


namespace TelegramBot_Fitz.Core {
    public class FloatingRateLoanCalculator : ILoanCalculator {
        public decimal LoanAmount { get; set; }
        public int TotalYears { get; set; }
        public decimal FirstRate { get; set; }
        public decimal SecondRate { get; set; }
        public decimal CalculateInterest(UserState state) {
            return CalculateFirstPeriodInterest(state) + CalculateSecondPeriodInterest(state);
        }
        public decimal CalculateFirstPeriodInterest(UserState state) {
            return LoanAmount * (FirstRate / 100) * (6 / 12m);
        }
        public decimal CalculateSecondPeriodInterest(UserState state) {
            return LoanAmount * (SecondRate / 100) * (6 / 12m);
        }
        public decimal CalculateTotalInterest(UserState state) {
            decimal total = 0;
            foreach (var rate in state.FloatingRates) {
                total += state.LoanAmount * (rate / 100) * ((int)state.FloatingRateResetPeriod / 12m);
            }
            return total;
        }
        public decimal CalculateTotalPayment(UserState state) {
            return LoanAmount + CalculateTotalInterest(state);
        }
    }
}
