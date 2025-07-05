using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fitz.Core.States;
using Fitz.Core.Interfaces;
using Fitz.Core.Models;

namespace TelegramBot_Fitz.Core
{
    public class OISCalculator : ILoanCalculator
    {
        public decimal CalculateInterest(UserState state)
        {
            return CalculateInterest(state.LoanAmount, state.FirstRate, state.Days);
        }
        private decimal CalculateInterest(decimal amount, decimal rate, int days)
        {
            decimal dailyRate = rate / 360;
            return amount * (dailyRate / 100) * days;
        }
        public decimal CalculateTotalPayment(UserState state)
        {
            return state.LoanAmount + CalculateInterest(state.LoanAmount, state.FirstRate, state.Days);
        }
        public OISCalculationResult CalculateOIS(UserState state)
        {
            decimal dailyRate = state.FirstRate / 360;
            decimal totalInterest = CalculateInterest(state.LoanAmount, state.FirstRate, state.Days);
            decimal totalPayment = CalculateTotalPayment(state);
            return new OISCalculationResult
            {
                DailyRate = dailyRate,
                TotalInterest = totalInterest,
                TotalPayment = totalPayment
            };
        }
        public string FormatCalculationResult(OISCalculationResult result, UserState state)
        {
            return $"OIS Calculation Results:\n" +
                   $"Daily Rate: {result.DailyRate:F6}%\n" +
                   $"Total Interest: {result.TotalInterest:F2} USD\n" +
                   $"Total Payment: {result.TotalPayment:F2} USD\n" +
                   $"Period: {state.Days} days\n" +
                   $"Overnight Rate: {state.FirstRate}%";
        }
    }
}
