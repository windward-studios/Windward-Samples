﻿using System;
using System.Threading.Tasks;


namespace JsonBatchProcessing
{
    class JSONBatchProcessing
    {
		static async Task Main(string[] args)
		{
			Console.WriteLine("Starting...");

			// The location of the RESTful engine to use
			string engineUrl = "http://ec2-44-204-11-110.compute-1.amazonaws.com";

			// Replace [LICENSE_KEY] with a valid RESTful engine license key
			// A trial key will work
			string licenseKey = "[LICENSE_KEY]";
            
            // The location of the template to use
			string tempURL = "https://windward-private-bucket.s3.amazonaws.com/LemonadeStandInvoice.docx";

			// The location of the JSON data file, and where we want to save the 
			// completed documents
			string jsonFilePath = AppDomain.CurrentDomain.BaseDirectory.Replace("\\bin\\Debug\\netcoreapp3.1\\", "\\Data\\LemonadeStandOrders.json");
			string saveLocation = AppDomain.CurrentDomain.BaseDirectory.Replace("\\bin\\Debug\\netcoreapp3.1\\", "\\GeneratedDocs\\");

			// Create a processor to handle document generation 
			DocumentProcessor generator = new DocumentProcessor(engineUrl, licenseKey);

			// Set the location that the generated documents will be stored at locally
			generator.SetSaveDirectory(saveLocation);

			// Process all of the orders, generating a document from our template for each order
			await generator.ProcessOrdersJson(jsonFilePath, tempURL);
			Console.WriteLine("Finished. Press any key to exit...");

            Console.ReadKey();
        }
	}
}