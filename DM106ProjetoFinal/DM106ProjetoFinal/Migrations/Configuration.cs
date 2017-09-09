namespace DM106ProjetoFinal.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<DM106ProjetoFinal.Models.DM106ProjetoFinalContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(DM106ProjetoFinal.Models.DM106ProjetoFinalContext context)
        {
            context.Products.AddOrUpdate(
                p => p.Id,
                new Models.Product
                {
                    Id = 0,
                    Nome = "P_01",
                    Descricao = "DESC_01",
                    Cor = "COR_1",
                    Modelo = "MODEL_01",
                    Codigo = "CODE_01",
                    Preco = 15,
                    Peso = 150,
                    Altura = 15,
                    Largura = 15,
                    Comprimento = 15,
                    Diametro = 15,
                    Url = "URL_01"
                });
        }
    }
}
