using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace DM106ProjetoFinal.Models
{
    public class DM106ProjetoFinalContext : DbContext
    {
        // You can add custom code to this file. Changes will not be overwritten.
        // 
        // If you want Entity Framework to drop and regenerate your database
        // automatically whenever you change your model schema, please use data migrations.
        // For more information refer to the documentation:
        // http://msdn.microsoft.com/en-us/data/jj591621.aspx
    
        public DM106ProjetoFinalContext() : base("name=DM106ProjetoFinalContext")
        {
        }

        public System.Data.Entity.DbSet<DM106ProjetoFinal.Models.Product> Products { get; set; }

        public System.Data.Entity.DbSet<DM106ProjetoFinal.Models.Order> Orders { get; set; }
    }
}
