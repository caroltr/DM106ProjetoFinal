using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using DM106ProjetoFinal.Models;

namespace DM106ProjetoFinal.Controllers
{
    public class ProductsController : ApiController
    {
        private DM106ProjetoFinalContext db = new DM106ProjetoFinalContext();

        // GET: api/Products
        // Lista todos os produtos
        [Authorize(Roles = "ADMIN,USER")]
        public IQueryable<Product> GetProducts()
        {
            return db.Products;
        }

        // GET: api/Products/5
        // Recuperar informação de um produto através de seu id
        [Authorize(Roles = "ADMIN,USER")]
        [ResponseType(typeof(Product))]
        public IHttpActionResult GetProduct(int id)
        {
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return NotFound();
            }

            return Ok(product);
        }

        // PUT: api/Products/5
        // Alterar a informação de um produto através de seu id
        [Authorize(Roles = "ADMIN")]
        [ResponseType(typeof(void))]
        public IHttpActionResult PutProduct(int id, Product product)
        {
            
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != product.Id)
            {
                return BadRequest();
            }

            // Não deverá permitir a alteração do código e/ou do modelo para um valor já existente
            bool existProductWithSameCode = db.Products.Where(p => p.Codigo != product.Codigo).Any(p => p.Codigo == product.Codigo);
            bool existProductWithSameModel = db.Products.Where(p => p.Codigo != product.Codigo).Any(p => p.Modelo == product.Modelo);

            if (existProductWithSameCode && existProductWithSameModel)
            {
                return BadRequest("Falha na alteração! Código e modelo já existente.");
            }
            else if (existProductWithSameCode)
            {
                return BadRequest("Falha na alteração! Código já existente.");
            }
            else if (existProductWithSameModel)
            {
                return BadRequest("Falha na alteração! Modelo já existente.");
            }

            db.Entry(product).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/Products
        // Incluir novo produto
        [Authorize(Roles = "ADMIN")]
        [ResponseType(typeof(Product))]
        public IHttpActionResult PostProduct(Product product)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Não deverá permitir a inclusão de um produto com código e/ou modelo com um valor que já exista
            bool existProductWithSameCode = db.Products.Any(p => p.Codigo == product.Codigo);
            bool existProductWithSameModel = db.Products.Any(p => p.Modelo == product.Modelo);

            if (existProductWithSameCode && existProductWithSameModel)
            {
                return BadRequest("Falha na inclusão! Código e modelo já existente.");
            }
            else if (existProductWithSameCode)
            {
                return BadRequest("Falha na inclusão! Código já existente.");
            }
            else if (existProductWithSameModel)
            {
                return BadRequest("Falha na inclusão! Modelo já existente.");
            }

            db.Products.Add(product);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = product.Id }, product);
        }

        // DELETE: api/Products/5
        // Apagar um produto através de seu id
        [Authorize(Roles = "ADMIN")]
        [ResponseType(typeof(Product))]
        public IHttpActionResult DeleteProduct(int id)
        {
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return NotFound();
            }

            db.Products.Remove(product);
            db.SaveChanges();

            return Ok(product);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool ProductExists(int id)
        {
            return db.Products.Count(e => e.Id == id) > 0;
        }
    }
}