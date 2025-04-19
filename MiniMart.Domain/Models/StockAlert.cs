using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace MiniMart.Domain.Models
{
    public class StockAlert : EntityBase
    {
        public DateTime Date { get; set; }
        public string AlertMessage { get; set; }
    }
}
