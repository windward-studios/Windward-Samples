using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WindwardRestApi.src.Model;

public class DocumentProcessor
{
	private WindwardClientWrapper client;
	private DataSourceWrapper dataSourceWrapper = new DataSourceWrapper();
	private string saveDirectory;
	private List<Task> requests = new List<Task>();
	private SaveDocuments docSaver;

	public DocumentProcessor(string engineUrl, string licenseKey)
	{
		client = new WindwardClientWrapper(new Uri(engineUrl), licenseKey);
	}

	// Function specific to processing our Json orders
	public void processOrdersJson(string filepath, string templateUrl)
	{
		docSaver = new SaveDocuments(saveDirectory);
		Console.WriteLine("Processing JSON data...");

		// Process the datafile
		List<string> names = dataSourceWrapper.processOrders(filepath);
		List<DataSource> dataSources = dataSourceWrapper.GetDataSources();

		// Create an object pairing each datasource to the corresponding customer name
		var nameAndDataSource = names.Zip(dataSources, (n, d) => new { Name = n, Source = d });

		// Our Invoice template has an input parameter to track the number assigned to the invoice
		// on creation
		int invoiceNum = 1;

		Console.WriteLine("Creating Requests...");

		// Create a request for each order that was parsed in
		foreach (var nD in nameAndDataSource)
		{
			// Create our template object for this order, and add corresponding data source
			// and input parameter to it
			TemplateWrapper temp = new TemplateWrapper(templateUrl, "docx", "pdf");
			temp.addDataSource(nD.Source);
			temp.setInputParameterInt("InvoiceNum", invoiceNum);

			// Create an async task to handle processing the request,
			// and add it to a list of all requests
			requests.Add(processOrderRequest(client.sendRequest(temp), nD.Name));
			Console.WriteLine("Created request for {0}'s invoice...", nD.Name);
		}

		Task complete = Task.WhenAll(requests);
		try
		{
			complete.Wait();
		}
		catch { }
	}
	public void setSaveDirectory(string filepath)
	{
		saveDirectory = filepath;
	}

	// Handle the created requests after the request has been sent
	// to the RESTful engine
	private async Task processOrderRequest(Task<string> task, string name)
	{
		while (!task.IsCompleted)
		{
			await Task.Delay(100);
		}
		
		// Retreive the completed document
		Document doc = await client.getDocument(task.Result);
		Console.WriteLine("{0}'s invoice is complete...", name);

		// Save the completed document
		docSaver.saveInvoiceDocument(doc, name);
	}


}
