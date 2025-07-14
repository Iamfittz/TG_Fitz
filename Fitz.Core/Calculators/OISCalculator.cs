using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fitz.Core.States;
using Fitz.Core.Interfaces;
using Fitz.Core.Models;
using Fitz.Core.Enums;
using Microsoft.VisualBasic;

namespace TelegramBot_Fitz.Core
{
    public class OISCalculator : ILoanCalculator
    {
        private decimal CalculateInterest(decimal amount, decimal rate, int days, DayCountConvention dayCountConvention)
        {
            decimal denominator = dayCountConvention switch {
                DayCountConvention.Actual360 => 360,
                DayCountConvention.Actual365 =>365,
                DayCountConvention.Thirty360 =>360,
                DayCountConvention.ActualActual =>DateTime.IsLeapYear(DateTime.Now.Year) ? 366 : 365,
                _ =>360
            };

            decimal dailyRate = rate / denominator;
            return amount * (dailyRate / 100) * days;
        }
        public decimal CalculateTotalPayment(UserState state, DayCountConvention dayCountConvention)
        {
            return state.LoanAmount + CalculateInterest(state.LoanAmount, state.FirstRate, state.Days, dayCountConvention);
        }
        public OISCalculationResult CalculateOIS(UserState state, DayCountConvention dayCountConvention)
        {
            decimal denominator = dayCountConvention switch {
                DayCountConvention.Actual360 => 360,
                DayCountConvention.Actual365 => 365,
                DayCountConvention.Thirty360 => 360,
                DayCountConvention.ActualActual => DateTime.IsLeapYear(DateTime.Now.Year) ? 366 : 365,
                _ => 360
            };

            decimal dailyRate = state.FirstRate / denominator;
            decimal totalInterest = CalculateInterest(state.LoanAmount, state.FirstRate, state.Days, dayCountConvention);
            decimal totalPayment = CalculateTotalPayment(state, dayCountConvention);
            return new OISCalculationResult
            {
                DailyRate = dailyRate,
                TotalInterest = totalInterest,
                TotalPayment = totalPayment
            };
        }
    }
}
