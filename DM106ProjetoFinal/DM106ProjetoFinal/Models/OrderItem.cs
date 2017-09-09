using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DM106ProjetoFinal.Models
{
    public class OrderItem
    {
        public int Id { get; set; }

        public int Quantidade { get; set; }

        public int ProductId { get; set; }

        public int OrderId { get; set; }

        public virtual Product Product { get; set; }        
    }
}