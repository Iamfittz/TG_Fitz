using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fitz.Core.Models;
using TelegramBot_Fitz.Core;
using Fitz.Core.States;

namespace TG_Fitz.Core
{
    public class OISResultFormatter
    {
        public static string FormatCalculationResult(OISCalculationResult result, UserState state)
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
