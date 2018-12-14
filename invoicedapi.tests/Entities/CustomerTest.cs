using System;
using Xunit;
using Invoiced;
using System.Net.Http;
using System.Collections.Generic;
using RichardSzalay.MockHttp;
using Newtonsoft.Json;


namespace InvoicedTest
{

    public class CustomerTest
    {

      private Customer createDefaultCustomer(HttpClient client) {
           var json =  @"{'id': 15444
                }";

            var customer = JsonConvert.DeserializeObject<Customer>(json);

            var connection  = new Connection("voodoo",Invoiced.Environment.test);

            connection.TestClient(client);

            customer.ChangeConnection(connection);

            return customer;

      }

        [Fact]
        public void Deserialize() {
           var json =  @"{'id': 15444,
                'object': 'customer',
                'name': 'Acme',
                'number': 'CUST-0001',
                'email': 'billing@acmecorp.com',
                'autopay': true,
                'payment_terms': null,
                'payment_source': {
                    'id': 850,
                    'object': 'card',
                    'brand': 'Visa',
                    'last4': '4242',
                    'exp_month': 2,
                    'exp_year': 20,
                    'funding': 'credit'
                },
                'taxes': [],
                'type': 'company',
                'attention_to': null,
                'address1': null,
                'address2': null,
                'city': null,
                'state': null,
                'postal_code': null,
                'country': 'US',
                'tax_id': null,
                'phone': null,
                'notes': null,
                'sign_up_page': null,
                'sign_up_url': null,
                'statement_pdf_url': 'https://dundermifflin.invoiced.com/statements/t3NmhUomra3g3ueSNnbtUgrr/pdf',
                'created_at': 1415222128,
                'metadata': {}
                }";

           var customer = JsonConvert.DeserializeObject<Customer>(json);

            Assert.True(customer.Number == "CUST-0001");
            Assert.True(customer.Name == "Acme");
            Assert.True(customer.Email == "billing@acmecorp.com");
        }


        [Fact]
        public void Create()
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When("https://testmode/customers/4").Respond("application/json", "{'id' : 4, 'name' : 'Test McGee'}");

            
            var client = mockHttp.ToHttpClient();

            var conn = new Connection("voodoo",Invoiced.Environment.test);
           
            conn.TestClient(client);

            var customerConn = conn.NewCustomer();

            var customer = customerConn.Retrieve(4);

            Console.WriteLine(customer);

            Assert.True(customer.Id == 4);
    
        }

        [Fact]
        public void Save()
        {

        var  jsonReponseSave = @"{
            'id': 15444,
            'object': 'customer',
            'number': 'CUST-0001',
            'name': 'Acme',
            'email': 'billing@acmecorp.com',
            'autopay': false,
            'payment_terms': 'NET 14',
            'payment_source': null,
            'taxes': [],
            'type': 'company',
            'attention_to': 'Sarah Fisher',
            'address1': '342 Amber St',
            'address2': null,
            'city': 'Hill Valley',
            'state': 'CA',
            'postal_code': '94523',
            'country': 'US',
            'tax_id': '893-934835',
            'phone': '(820) 297-2983',
            'notes': null,
            'sign_up_page': null,
            'sign_up_url': null,
            'statement_pdf_url': 'https://dundermifflin.invoiced.com/statements/t3NmhUomra3g3ueSNnbtUgrr/pdf',
            'created_at': 1415222128,
            'metadata': {}
            }";


//              -d payment_terms="NET 14" \
//   -d attention_to="Sarah Fisher" \
//   -d address1="342 Amber St" \
//   -d city="Hill Valley" \
//   -d state="CA" \
//   -d postal_code="94523" \
//   -d tax_id="893-934835" \
//   -d phone="(820) 297-2983" \

            var JsonRequest = @"{
                'payment_terms': 'NET 14',
                }";

            var mockHttp = new MockHttpMessageHandler();
            var httpPatch = new HttpMethod("PATCH");
            var request = mockHttp.When(httpPatch,"https://testmode/customers/15444").WithJson(JsonRequest).Respond("application/json",jsonReponseSave);

            var client = mockHttp.ToHttpClient();

            var customer = createDefaultCustomer(client);


            customer.PaymentTerms = "NET 14";
            // customer.AttentionTo = "Sarah Fisher";
            // customer.Address1 = "342 Amber St";
            // customer.City = "Hill Valley";
            // customer.State = "CA";
            // customer.PostalCode = "94523";
            // customer.TaxId = "893-934835";
            // customer.Phone = "(820) 297-2983";

           
       
            customer.SaveAll();
          

            Console.WriteLine(customer);


        }



        

    }
}