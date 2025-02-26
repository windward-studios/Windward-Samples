using System;
using System.Threading.Tasks;


namespace JsonBatchProcessing
{
    class JSONBatchProcessing
    {
        /// <summary>
        /// Main entry point for the program. Here we are just doing setup like grabbing the files needed to generate the document,
        /// setting the output directory, and calling the function to process the documents.
        /// </summary>
        /// <param name="args">Command line arguments</param>
        /// <returns>Task</returns>
        static async Task Main(string[] args)
		{
			Console.WriteLine("Starting...");

			// The location of the RESTful engine to use
			string engineUrl = "http://ec2-44-204-11-110.compute-1.amazonaws.com";

			// Replace [LICENSE_KEY] with a valid RESTful engine license key
			// A trial key will work
			string licenseKey = "[LICENSE_KEY]";
            
            // The location of the template to use
			string tempURL = AppDomain.CurrentDomain.BaseDirectory.Replace("\\bin\\Debug\\netcoreapp3.1\\", "\\Data\\LemonadeStandInvoice.docx");

			// The location of the JSON data file, and where we want to save the 
			// completed documents
			string jsonFilePath = AppDomain.CurrentDomain.BaseDirectory.Replace("\\bin\\Debug\\netcoreapp3.1\\", "\\Data\\LemonadeStandOrders.json");
			string saveLocation = AppDomain.CurrentDomain.BaseDirectory.Replace("\\bin\\Debug\\netcoreapp3.1\\", "\\GeneratedDocs\\");

			// Create a processor to handle document generation 
			DocumentProcessor generator = new DocumentProcessor(engineUrl, licenseKey, saveLocation);

            // Load the template into memory
            byte[] templateBytes = await System.IO.File.ReadAllBytesAsync(tempURL);

            // Process all of the orders, generating a document from our template for each order
            await generator.ProcessOrdersJson(jsonFilePath, templateBytes);
			Console.WriteLine("Finished. Press any key to exit...");

            Console.ReadKey();
        }
	}
}