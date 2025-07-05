using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fitz.Core.Models {
    public class YearlyCalculation {
        public int Year { get; set; }
        public decimal Rate { get; set; }
        public decimal Interest { get; set; }
        public decimal AccumulatedAmount { get; set; }
    }
}
