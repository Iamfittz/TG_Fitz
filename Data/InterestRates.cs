using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace TG_Fitz.Data
{
    public class InterestRates
    {
        [Key]
        public int Id { get; set; }
        public DateTime Date { get; set; }

        public decimal InterestRateValue { get; set; }

        public string? RateType { get; set; }

    }
}
