using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TG_Fitz.Data
{
    public class Trade
    {
        [Key]
        public int Id { get; set; }
        [MaxLength(255)]
        public required string CompanyName { get; set; }
        public decimal LoanAmount { get; set; }
        public int Years { get; set; }

        public DateTime CreatedAt { get; set; }

        // Foreign key to User
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; }

    }
}
