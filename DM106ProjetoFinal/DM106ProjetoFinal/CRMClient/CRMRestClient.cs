using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;

namespace DM106ProjetoFinal.CRMClient
{
    public class CRMRestClient
    {
        private HttpClient client;

        public CRMRestClient()
        {
            client = new HttpClient();

            // Endereço do CRM
            client.BaseAddress = new Uri("http://siecolacrm.azurewebsites.net/api/");
            
            // Header para o JSON
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
                        
            // Montando as credenciais em Base64
            byte[] str1Byte = System.Text.Encoding.UTF8.GetBytes(string.Format("{0}:{1}", "crmwebapi", "crmwebapi"));
            String plaintext = Convert.ToBase64String(str1Byte);
            
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", plaintext);
        }

        public Customer GetCustomerByEmail(string email)
        {
            HttpResponseMessage response = client.GetAsync("customers/byemail?email=" + email).Result;
            if (response.IsSuccessStatusCode)
            {
                // Parse da resposta
                Customer customer = (Customer)response.Content.ReadAsAsync<Customer>().Result;
                return customer;
            }
            return null;
        }
    }
}