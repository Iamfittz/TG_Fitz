using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace TG_Fitz.Data
{
    public class User
    {
        public int Id { get; set; }
        public long TG_ID { get; set; }
      
        public ICollection<Trade> Trades { get; set; } = new List<Trade>();
        [MaxLength(255)]
        public string? Username { get; set; }
    }
}
