using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DM106ProjetoFinal.Models
{
    public class Product
    {
        public int Id { get; set; }
     
        [Required(ErrorMessage = "O campo nome é obrigatório")]
        public string Nome { get; set; }
     
        public string Descricao { get; set; }
     
        public string Cor { get; set; }
     
        [Required(ErrorMessage = "O campo modelo é obrigatório")]
        public String Modelo { get; set; }
     
        [Required(ErrorMessage = "O campo codigo é obrigatório")]
        public string Codigo { get; set; }
     
        public decimal Preco { get; set; }
     
        public decimal Peso { get; set; }
     
        public decimal Altura { get; set; }
     
        public decimal Largura { get; set; }
     
        public decimal Comprimento { get; set; }
     
        public decimal Diametro { get; set; }
     
        public string Url { get; set; }
    }
}