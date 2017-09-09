using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DM106ProjetoFinal.Models
{
    public class Order
    {
        public Order()
        {
            this.OrderItems = new HashSet<OrderItem>();
        }

        public int Id { get; set; }

        public string Email { get; set; }

        public DateTime DataPedido { get; set; }
        
        public DateTime DataEntrega { get; set; }

        public string Status { get; set; }

        public decimal PrecoTotal { get; set; }

        public decimal PesoTotal { get; set; }

        public decimal PrecoFrete { get; set; }

        public virtual ICollection<OrderItem> OrderItems { get; set; }
    }
}