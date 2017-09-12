using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Description;
using DM106ProjetoFinal.Models;
using System.Diagnostics;
using DM106ProjetoFinal.CRMClient;
using DM106ProjetoFinal.br.com.correios.ws;

namespace DM106ProjetoFinal.Controllers
{
    [Authorize]
    public class OrdersController : ApiController
    {
        private DM106ProjetoFinalContext db = new DM106ProjetoFinalContext();
        
        // GET: api/Orders
        // Lista todos os pedidos
        [Authorize(Roles = "ADMIN")]
        public IQueryable<Order> GetOrders()
        {
            return db.Orders;
        }
        
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

            if (IsAuthorized(order.Email))
            {
                return Ok(order);
            }
            else
            {
                return BadRequest("Acesso não autorizado.");
            }           
        }
        
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
            order.DataPedido = DateTime.Now;
            order.DataEntrega = DateTime.Now;

            // Calcular preço total
            decimal precoTotal = 0;
            
            foreach(OrderItem item in order.OrderItems)
            {
                // precoTotal += item.Product.Preco * item.Quantidade;

                Product p = db.Products.Where(i => i.Id == item.ProductId).FirstOrDefault();
                if(p == null)
                {
                    return BadRequest("O item de ID = " + item.ProductId + " não existe.");
                }

                precoTotal += (p.Preco * item.Quantidade);
            }
            order.PrecoTotal = precoTotal;

            // User.Identity.Name retorna o email do usuário autenticado
            order.Email = User.Identity.Name;            

            db.Orders.Add(order);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = order.Id }, order);
        }
        
        // PUT: api/Orders/byEmail?email={email}
        // Lista todos os pedidos de um usuário através de seu email        
        [ResponseType(typeof(Order))]
        [HttpPut]
        [Route("api/Orders/Close")]
        public IHttpActionResult CloseOrder(int id)
        {
            Order order = db.Orders.Find(id);
            if (order == null)
            {
                return BadRequest("O pedido não existe.");
            }

            if (IsAuthorized(order.Email))
            {
                if(order.PrecoFrete != 0)
                {
                    // Frete já foi calculado, portanto pedido pode ser fechado
                    order.Status = "fechado";

                    db.Entry(order).State = EntityState.Modified;
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
        
        // DELETE: api/Orders/5
        [ResponseType(typeof(Order))]
        public IHttpActionResult DeleteOrder(int id)
        {
            Order order = db.Orders.Find(id);
            if (order == null)
            {
                return BadRequest("O pedido não existe.");
            }

            if (IsAuthorized(order.Email))
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


        // PUT: api/Orders/Freight?id={orderId}
        // Calcula o frete de um pedido através de seu id
        [ResponseType(typeof(Order))]
        [HttpPut]
        [Route("api/Orders/Freight")]
        public IHttpActionResult GetFreightAndDate(int id)
        {
            Order order = db.Orders.Find(id);
            if (order == null)
            {
                return BadRequest("O pedido não existe.");
            }

            if (IsAuthorized(order.Email))
            {
                Customer customer;

                try
                {
                    CRMRestClient client = new CRMRestClient();
                    customer = client.GetCustomerByEmail(User.Identity.Name);
                }
                catch(Exception e)
                {
                    return BadRequest("Falha ao consultar CEP: ocorreu um erro ao acessar o serviço de CRM.");
                }

                if(customer == null)
                {
                    return BadRequest("Falha ao consultar CEP: usuário não existente.");
                }
                else
                {
                    if(order.OrderItems.Count <= 0)
                    {
                        return BadRequest("O pedido não contêm itens.");
                    }

                    decimal alturaFinal = 0;
                    decimal larguraFinal = 0;
                    decimal comprimentoFinal = 0;
                    decimal diametroFinal = 0;

                    decimal pesoFinal = 0;
                    
                    foreach (OrderItem oi in order.OrderItems)
                    {
                        // Incrementando peso
                        pesoFinal += oi.Product.Peso * oi.Quantidade;

                        // Incrementando a largura
                        larguraFinal += (oi.Product.Largura * oi.Quantidade);

                        // Pegando o maior comprimento
                        if(oi.Product.Comprimento > comprimentoFinal)
                        {
                            comprimentoFinal = oi.Product.Comprimento;
                        }

                        // Pegando a maior largura
                        if (oi.Product.Altura > alturaFinal)
                        {
                            alturaFinal = oi.Product.Altura;
                        }

                        // Pegando o maior diametro
                        if (oi.Product.Diametro > diametroFinal)
                        {
                            diametroFinal = oi.Product.Diametro;
                        }
                    }

                    CalcPrecoPrazoWS correios = new CalcPrecoPrazoWS();

                    string nCdServico = "40010"; // SEDEX
                    string sCdCepOrigem = "69096010";
                    string sCdCepDestino = customer.zip.Trim().Replace("-","");
                    string nVIPeso = pesoFinal.ToString();
                    int nCdFormato = 1; // Caixa
                    decimal nVIComprimento = comprimentoFinal;
                    decimal nVIAltura = alturaFinal;
                    decimal nVILargura = larguraFinal;
                    decimal nVIDiametro = diametroFinal;
                    string sCdMaoPropria = "N";
                    decimal nVIValorDeclarado = order.PrecoTotal;
                    string sCdAvisoRecebimento = "S";

                    cResultado corresiosResult;
                    try
                    {
                        corresiosResult = correios.CalcPrecoPrazo("", "", nCdServico, sCdCepOrigem, sCdCepDestino, nVIPeso, nCdFormato,
                            nVIComprimento, nVIAltura, nVILargura, nVIDiametro, sCdMaoPropria, nVIValorDeclarado, sCdAvisoRecebimento);
                    }
                    catch (Exception e)
                    {
                        return BadRequest("Falha ao calcular o frete e prazo de entrega: ocorreu um erro ao tentar acessar o serviço dos correios.");
                    }

                    if(!corresiosResult.Servicos[0].MsgErro.Equals(""))
                    {
                        return BadRequest("Falha ao calcular o frete e prazo de entrega: " + corresiosResult.Servicos[0].MsgErro);
                    }

                    int prazo = int.Parse(corresiosResult.Servicos[0].PrazoEntrega);
                    decimal valorFrete = decimal.Parse(corresiosResult.Servicos[0].Valor);

                    // Atualizar pedido
                    order.PrecoFrete = valorFrete;
                    order.DataEntrega = DateTime.Now.AddDays(prazo);
                    order.PesoTotal = pesoFinal;
                    order.PrecoTotal += valorFrete;
                    
                    db.Entry(order).State = EntityState.Modified;
                    db.SaveChanges();
                    
                    return Ok(db.Orders.Find(id));
                }
            }
            else
            {
                return BadRequest("Acesso não autorizado.");
            }
        }

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
        
        private bool IsAuthorized(string email)
        {
            bool userOwnOrder = User.Identity.Name.Equals(email);

            Trace.TraceInformation(User.Identity.Name);

            return (User.IsInRole("ADMIN") || userOwnOrder);
        }
    }
}