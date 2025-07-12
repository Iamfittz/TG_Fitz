using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fitz.Core.States;
using Fitz.Core.Interfaces;
using Fitz.Core.Models;


namespace TelegramBot_Fitz.Core {
    public class FloatingRateLoanCalculator : IFloatingRateCalculator {
        public decimal LoanAmount { get; set; }
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
        public List<PeriodInterestDetail> GetInterestBreakdown(UserState state) {
            var list = new List<PeriodInterestDetail>();

            for (int i = 0; i < state.FloatingRates.Count; i++) {
                decimal rate = state.FloatingRates[i];
                decimal interest = state.LoanAmount * (rate / 100) * ((int)state.FloatingRateResetPeriod / 12m);

                list.Add(new PeriodInterestDetail {
                    PeriodNumber = i + 1,
                    Rate = rate,
                    Interest = interest
                });
            }
            return list;
        }
    }
}
