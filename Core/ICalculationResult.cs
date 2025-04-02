using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TG_Fitz.Core
{
    public interface ICalculationResult
    {
        decimal TotalInterest { get; set; }
        decimal TotalPayment { get; set; }
    }
}
