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
using System.Security.Principal;
using System.Diagnostics;

namespace DM106ProjetoFinal.Controllers
{
    [Authorize]
    public class OrdersController : ApiController
    {
        private DM106ProjetoFinalContext db = new DM106ProjetoFinalContext();

        // OK!!!!
        // GET: api/Orders
        // Lista todos os pedidos
        [Authorize(Roles = "ADMIN")]
        public IQueryable<Order> GetOrders()
        {
            return db.Orders;
        }

        // OK!!!!
        // GET: api/Orders/5
        // Recuperar informação de um pedido através de seu id
        [ResponseType(typeof(Order))]
        public IHttpActionResult GetOrder(int id)
        {
            Order order = db.Orders.Find(id);
            if (order == null)
            {
                return BadRequest("O pedido não existe.");
            }

            if (IsAuthorized(order))
            {
                return Ok(order);
            }
            else
            {
                return BadRequest("Acesso não autorizado.");
            }           
        }

        // OK!!!
        // GET: api/Orders/byEmail?email={email}
        // Lista todos os pedidos de um usuário através de seu email        
        [ResponseType(typeof(Order))]
        [HttpGet]
        [Route("api/Orders/ByEmail")]
        public IHttpActionResult GetOrdersByEmail(string email)
        {
            if(IsAuthorized(email))
            {
                List<Order> orders = db.Orders.Where(o => o.Email.ToLower().Trim().Equals(email.ToLower().Trim())).ToList();
                
                if (orders == null)
                {
                    return BadRequest("Usuário não possui pedidos.");
                }

                return Ok(orders);
            }
            else
            {
                return BadRequest("Acesso não autorizado.");
            }
        }

        // PUT: api/Orders/5
        /* [ResponseType(typeof(void))]
        public IHttpActionResult PutOrder(int id, Order order)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != order.Id)
            {
                return BadRequest();
            }

            db.Entry(order).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrderExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        } */

        // OK!!!
        // POST: api/Orders
        [ResponseType(typeof(Order))]
        public IHttpActionResult PostOrder(Order order)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Valores default
            order.Status = "novo";
            order.PesoTotal = 0;
            order.PrecoFrete = 0;
            order.PrecoTotal = 0;
            order.DataPedido = DateTime.Now;
            order.DataEntrega = DateTime.Now;

            // User.Identity.Name retorna o email do usuário autenticado
            order.Email = User.Identity.Name;            

            db.Orders.Add(order);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = order.Id }, order);
        }
        
        // GET: api/Orders/byEmail?email={email}
        // Lista todos os pedidos de um usuário através de seu email        
        [ResponseType(typeof(Order))]
        [HttpGet]
        [Route("api/Orders/Close")]
        public IHttpActionResult CloseOrder(int id)
        {
            Order order = db.Orders.Find(id);
            if (order == null)
            {
                return BadRequest("O pedido não existe.");
            }

            if (IsAuthorized(order))
            {
                if(order.PrecoFrete != 0)
                {
                    // Frete já foi calculado, portanto pedido pode ser fechado
                    order.Status = "fechado";

                    db.SaveChanges();
                    return Ok(order);
                }
                else
                {
                    return BadRequest("Frete ainda não calculado.");
                }
            }
            else
            {
                return BadRequest("Acesso não autorizado.");
            }
        }

        // OK!!!!
        // DELETE: api/Orders/5
        [ResponseType(typeof(Order))]
        public IHttpActionResult DeleteOrder(int id)
        {
            Order order = db.Orders.Find(id);
            if (order == null)
            {
                return BadRequest("O pedido não existe.");
            }

            if (IsAuthorized(order))
            {
                db.Orders.Remove(order);
                db.SaveChanges();

                return Ok(order);
            }
            else
            {
                return BadRequest("Acesso não autorizado.");
            }
        }

        // TODO GetFreightRateAndDeliveryDate(int id)

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool OrderExists(int id)
        {
            return db.Orders.Count(e => e.Id == id) > 0;
        }

        private bool IsAuthorized(Order order)
        {
            bool userOwnOrder = User.Identity.Name.Equals(order.Email);

            Trace.TraceInformation(User.Identity.Name);

            return (User.IsInRole("ADMIN") || userOwnOrder);
        }

        private bool IsAuthorized(string email)
        {
            bool userOwnOrder = User.Identity.Name.Equals(email);

            Trace.TraceInformation(User.Identity.Name);

            return (User.IsInRole("ADMIN") || userOwnOrder);
        }
    }
}