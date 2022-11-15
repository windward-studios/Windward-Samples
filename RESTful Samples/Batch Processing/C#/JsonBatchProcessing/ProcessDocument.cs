using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WindwardRestApi.src.Api;
using WindwardRestApi.src.Model;

public class DocumentProcessor
{
	private WindwardClient client;
	private DataSourceWrapper dataSourceWrapper = new DataSourceWrapper();
	private string saveDirectory;
    private SaveDocuments docSaver;

	public DocumentProcessor(string engineUrl, string licenseKey)
	{
        client = new WindwardClient(new Uri(engineUrl));
        client.LicenseKey = licenseKey;
	}

	// Function specific to processing our Json orders
	public async Task ProcessOrdersJson(string filepath, string templateUrl)
	{
		docSaver = new SaveDocuments(saveDirectory);
		Console.WriteLine("Processing JSON data...");

		// Process the datafile
		List<string> names = dataSourceWrapper.processOrders(filepath);
		List<DataSource> dataSources = dataSourceWrapper.GetDataSources();

        // Our Invoice template has an input parameter to track the number assigned to the invoice
		// on creation
		int invoiceNum = 1;

		Console.WriteLine("Creating Requests...");

        var jobs = await PostAllTemplates(dataSources, templateUrl);

        var dsJobPairs = names.Zip(jobs, (name, job) => (name, job));

        Console.WriteLine("Retrieving Documents");
        await GetAllDocuments(dsJobPairs);
    }


    private async Task<IEnumerable<string>> PostAllTemplates(IEnumerable<DataSource> dataSources, string templateUrl)
    {
        List<Task<string>> jobList = new List<Task<string>>();
        foreach (var dataSource in dataSources)
        {
            Template template = CreateTemplate(templateUrl, "docx", "pdf");
            template.Datasources.Add(dataSource);
            template.Parameters.Add(new Parameter("InvoiceNum", 1));

            jobList.Add(PostTemplate(template));
        }

        await Task.WhenAll(jobList);
        return jobList.Select(job => job.Result);
    }

    private async Task GetAllDocuments(IEnumerable<(string, string)> stringJobPairs)
    {
        foreach (var stringJobPair in stringJobPairs)
        {
            Console.WriteLine($"Retrieving documentL: {stringJobPair.Item1}");
            Document document;
            try
            {
                document = await GetDocument(stringJobPair.Item2);
            }
            catch (Exception)
            {
                Console.WriteLine($"Processing Failed for Document: {stringJobPair.Item1}");
                continue;
            }

            await docSaver.SaveInvoiceDocument(document, stringJobPair.Item1);
        }
    }

    private Template CreateTemplate(string templateUrl, string templateFormatExtension, string outputFormatExtension)
    {
        Template.OutputFormatEnum outputFormat;
        Template.FormatEnum templateFormat;
        switch (outputFormatExtension.ToLower())
        {
            case "pdf":
                outputFormat = Template.OutputFormatEnum.Pdf;
                break;
            case "docx":
                outputFormat = Template.OutputFormatEnum.Docx;
                break;
            case "xlsx":
                outputFormat = Template.OutputFormatEnum.Xlsx;
                break;
            case "pptx":
                outputFormat = Template.OutputFormatEnum.Pptx;
                break;
            default:
                outputFormat = Template.OutputFormatEnum.Pdf;
                break;
        }
        switch (templateFormatExtension.ToLower())
        {
            case "docx":
                templateFormat = Template.FormatEnum.Docx;
                break;
            case "xlsx":
                templateFormat = Template.FormatEnum.Xlsx;
                break;
            case "pptx":
                templateFormat = Template.FormatEnum.Pptx;
                break;
            default:
                templateFormat = Template.FormatEnum.Docx;
                break;
        }

        // Create the template object based on the given information
        return  new Template(outputFormat, templateUrl, templateFormat);
	}

    private async Task<string> PostTemplate(Template template)
    {
        try
        {
            return (await client.PostDocument(template)).Guid;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
	}

    private async Task<Document> GetDocument(string id)
    {
        System.Net.HttpStatusCode status = await client.GetDocumentStatus(id);

        // Status code 302 means the document is completed and ready to be retrieved
        while (status != (System.Net.HttpStatusCode)302)
        {
            // Check if there were any errors while processing the template
            if (status == (System.Net.HttpStatusCode)401 || status == (System.Net.HttpStatusCode)404 || status == (System.Net.HttpStatusCode)500)
            {
                try
                {
                    ServerException err = await client.GetError(id);
                    Console.WriteLine("Error with request: " + id);
                    Console.WriteLine(err.Type);
                    Console.WriteLine(err.Message);
                    throw err;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
            // Pause before checking the status again
            await Task.Delay(100);
            status = await client.GetDocumentStatus(id);
        }

        // Retrieve the generated document, and remove the completed request from the server
        Document doc = await client.GetDocument(id);
        await client.DeleteDocument(id);
        return doc;
    }

	public void SetSaveDirectory(string filepath)
	{
		saveDirectory = filepath;
	}
}
