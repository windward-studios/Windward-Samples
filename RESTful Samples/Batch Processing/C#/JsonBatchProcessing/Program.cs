using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WindwardRestApi.src.Api;
using WindwardRestApi.src.Model;

namespace JsonBatchProcessing
{
    // The Item and Order classes are for parsing the JSON datasource that we are interested in
    public class Item
    {
        public string Name { get; set; }
        public double Price { get; set; }
        public int Quantity { get; set; }
    }
    public class Order
    {
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public double OrderTotal { get; set; }
        public List<Item> ItemList { get; set; }
    }

    class Program
    {
        // Creates a task that creates and posts a request to the RESTful engine, using the given template, JSON data, and client
        public static async Task<string[]> createRequest(Template template, byte[] data, string name, WindwardClient client, int count)
        {
            JsonDataSource dat = new JsonDataSource("Order", data);
            template.Datasources.Add(dat);

            // The template has an input parameter InvoiceNum, here we set that for this request
            Parameter invoiceNum = new Parameter("InvoiceNum", count);
            template.Parameters.Add(invoiceNum);
            Document doc = await client.PostDocument(template);
            return new string[] { doc.Guid, name };
        }

        // Creates a task to retrieve the document with a given id and save it, with minimal error checking
        public static async Task<Document> processDocument(string id, string name, WindwardClient client)
        {
            System.Net.HttpStatusCode status = await client.GetDocumentStatus(id);

            // Status code 302 means the document is completed and ready to be retrieved
            while (status != (System.Net.HttpStatusCode)302)
            {
                // Check if there were any errors while processing the template
                if (status == (System.Net.HttpStatusCode)401 || status == (System.Net.HttpStatusCode)404 || status == (System.Net.HttpStatusCode)500)
                {
                    ServerException err = await client.GetError(id);
                    Console.WriteLine("Error with request: " + id);
                    Console.WriteLine(err.Type);
                    Console.WriteLine(err.Message);
                    return null;
                }
                // Pause before checking the status again
                await Task.Delay(100);
                status = await client.GetDocumentStatus(id);
                continue;
            }

            // Document has been generated successfully, time to retrieve it
            Console.WriteLine("Request for {0} completed...", name);
            Document doc = await client.GetDocument(id);

            // Save the completed document to disk, in the GeneratedDocs folder of the sample directory
            string filepath = AppDomain.CurrentDomain.BaseDirectory.Replace("\\bin\\Debug\\netcoreapp3.1\\", "\\GeneratedDocs\\");
            filepath = Path.GetFullPath(filepath + name + ".pdf");
            File.WriteAllBytes(filepath, doc.Data);

            // Remove the successfully completed request from the server
            await client.DeleteDocument(id);
            return doc;
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Starting...");

            // Instantiate the client, connecting to the RESTful engine hosted at the given url
            string url = "http://ec2-34-201-126-96.compute-1.amazonaws.com";
            WindwardClient client = new WindwardClient(new Uri(url));

            // Set the license key these requests will be ran with
            // Only required if the server doesn't have it set in the config file
            // Replace [LICENSE_KEY] with your license key
            client.LicenseKey = "[LICENSE_KEY]";

            // Get the location of our template, located in a hosted S3 bucket
            string tempURL = "https://windward-private-bucket.s3.amazonaws.com/LemonadeStandInvoice.docx";

            // Parse our list of records, creating a list of JSON objects corresponding to each order
            IList<Order> orderList = new List<Order>();
            try
            {
                string dataFile = AppDomain.CurrentDomain.BaseDirectory.Replace("\\bin\\Debug\\netcoreapp3.1\\", "\\Data\\LemonadeStandOrders.json");
                string records = File.ReadAllText(dataFile);
                JObject lemonadeOrders = JObject.Parse(records);
                IList<JToken> orders = lemonadeOrders["Orders"].Children().ToList();
                foreach (JToken order in orders)
                {
                    Order temp = order.ToObject<Order>();
                    orderList.Add(temp);
                }
                Console.WriteLine("Parsed Records...");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            
            List<Task<string[]>> requests = new List<Task<string[]>>();
            int InvoiceNum = 1;
            // Create a task to send a request for each record
            foreach (Order order in orderList)
            {
                // Convert one record into a byte array to be sent to the server
                byte[] data = System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(order));
                
                // Create our template object for this record
                Template template = new Template(Template.OutputFormatEnum.Pdf, tempURL, Template.FormatEnum.Docx);

                // Create a task to send the request, and add it to our list of tasks
                requests.Add(createRequest(template, data, order.LastName, client, InvoiceNum));
                InvoiceNum += 1;
            }

            // Because the requests are sent asynchronously, we need to wait for them to be sent before
            // checking their status and waiting for them to complete
            Task docRequests = Task.WhenAll(requests);
            try
            {
                docRequests.Wait();
            }
            catch { }

            // Each request returns an id associated with that request, which we will use to 
            // check the status of the request, and retrieve the completed document
            List<string[]> guids = new List<string[]>();
            foreach (Task<string[]> request in requests)
            {
                guids.Add(request.Result);
            }

            // Process each request, waiting for all of them to complete
            Console.WriteLine("Waiting for documents...");
            List<Task<Document>> docTasks = new List<Task<Document>>();
            foreach (string[] idAndName in guids)
            {
                docTasks.Add(processDocument(idAndName[0], idAndName[1], client));
            }

            Task docGen = Task.WhenAll(docTasks);
            
            // Wait for all of our tasks to retreive the documents to complete
            try
            {
                docGen.Wait();
            }
            catch { }

            Console.WriteLine("Finished processing documents...");
            Console.WriteLine("Done. Press any key to exit.");
            Console.ReadKey();
        }

    }
}