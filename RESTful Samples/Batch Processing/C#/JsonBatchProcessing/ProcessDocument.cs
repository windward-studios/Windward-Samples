using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JsonBatchProcessing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WindwardRestApi.src.Api;
using WindwardRestApi.src.Model;
using Document = WindwardRestApi.src.Model.Document;
using Parameter = WindwardRestApi.src.Model.Parameter;

public class DocumentProcessor
{
    private WindwardClient client;
    private string saveDirectory;
    private SaveDocuments docSaver;

    public DocumentProcessor(string engineUrl, string licenseKey, string saveLocation)
    {
        client = new WindwardClient(new Uri(engineUrl));
        client.LicenseKey = licenseKey;
        saveDirectory = saveLocation;
    }

    // Function specific to processing our Json orders
    public async Task ProcessOrdersJson(string filepath, byte[] templateBytes)
    {
        docSaver = new SaveDocuments(saveDirectory);
        Console.WriteLine("Processing JSON data...");

        // For this particular template, we are using a JSON file that contains an array of orders.
        // Each order is a JSON object that contains the information needed to generate an invoice.
        // Here we are splitting the JSON file into an individual JSON file for each order.
        // Each document will then be generated using one of these JSON files, so we create a DataSource object for each JSON file.
        // In a production scenario each of these JSON files would likely be the result of an API call or be generated on the fly for the purpose of document generation.
        // For a template that was setup differently you could use a single DataSource object that contains all of the data for all of the documents, then use an input parameter, so the engine is able to filter on the data required for the document.
        JObject orderJson = JObject.Parse(await File.ReadAllTextAsync(filepath));
        IList<JToken> orders = orderJson["Orders"].Children().ToList();

        List<(string, DataSource)> dataSources = new List<(string, DataSource)>();

        // Here we are creating a DataSource object for each order, and adding it to our list of data sources.
        // We'll store it in a tuple alongside the last name of the person the invoice is for, so we can use that info to save the document later.
        foreach (var order in orders)
        {
            byte[] data = System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(order));
            dataSources.Add((order["LastName"].ToString(), new JsonDataSource("Order", data)));
        }


        // Now that we have our list of data sources we will kick of a document generation job for each one. If you were instead using a single DataSource object, you would loop through a set of input parameters instead.
        HashSet<Task<DocumentResult>> jobList = new HashSet<Task<DocumentResult>>();
        foreach (var dataSource in dataSources)
        {
            Template template = CreateTemplate(templateBytes, "docx", "pdf");
            // Here we are setting the DataSource for the template to be the current order. If we were using a single DataSource object, we would set this to the same value every time.
            template.Datasources.Add(dataSource.Item2);
            // Here we are setting an input parameter for the template. In this case the value passed in doesn't matter much, since the template doesn't use it, but this is where you would set any input parameters that your template needs.
            // This could be set differently each time.
            template.Parameters.Add(new Parameter("InvoiceNum", 1));

            // Notice how on this line we kick of a new task that handles each individual document generation job.
            Task<DocumentResult> result = new Task<DocumentResult>(() =>
            {
                // here we are sending the post request for the engine to start generating the document.
                string jobId = PostTemplate(template).Result;
                if(jobId == null)
                {
                    Console.WriteLine($"Error starting document generation for job name: {dataSource.Item1}");
                    return null;
                }
                // here we are waiting for the document to be generated, and then retrieving it.
                Document doc = GetDocument(jobId, dataSource.Item1).Result;
                return new DocumentResult { JobName = dataSource.Item1, Document = doc, JobId = jobId};
            });
            // Now we start running the task and add it to our list of jobs.
            result.Start();
            jobList.Add(result);

        }

        // Now that all of our jobs are running, we will wait for them to complete.
        int numJobs = jobList.Count;
        int jobsCompleted = 0;
        while (jobsCompleted < numJobs)
        {
            // These jobs will not necessarily complete in order, so we'll use Task.WhenAny to get the first one that completes.
            Task<DocumentResult> result = (await Task.WhenAny<DocumentResult>(jobList));
            // We can then remove it from our list of jobs, and save the document.
            bool removeResult = jobList.Remove(result);
            if (!removeResult)
            {
                Console.WriteLine("Error removing result from job list");
            }
            DocumentResult docResult = result.Result;
            if (docResult.Document == null)
            {
                Console.WriteLine($"Error generating document. Document Name: {docResult.JobName}; Document Job Id: {docResult.JobId}");
                continue;
            }
            await docSaver.SaveInvoiceDocument(docResult.Document, docResult.JobName);
            jobsCompleted++;
        }
    }

    /// <summary>
    /// This will create our template object that we will send to our RESTful engine.
    /// </summary>
    /// <param name="templateBytes"></param>
    /// <param name="templateFormatExtension"></param>
    /// <param name="outputFormatExtension"></param>
    /// <returns></returns>
    private Template CreateTemplate(byte[] templateBytes, string templateFormatExtension, string outputFormatExtension)
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
        return new Template(outputFormat, templateBytes, templateFormat);
    }

    /// <summary>
    /// Posts the template to the RESTful engine to start document generation.
    /// </summary>
    /// <param name="template"></param>
    /// <returns></returns>
    private async Task<string> PostTemplate(Template template)
    {
        try
        {
            Console.WriteLine("Starting Document Generation...");
            var guid = (await client.PostDocument(template)).Guid;
            Console.WriteLine($"Document Generation started with id: {guid}");
            return guid;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return null;
        }
    }

    /// <summary>
    /// Waits for the document to be generated, and retrieves it from the RESTful engine.
    /// </summary>
    /// <param name="documentJobId"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    private async Task<Document> GetDocument(string documentJobId, string documentName)
    {
        string documentLogIdentifier = $"(Job ID: {documentJobId}, Document Name: {documentName})";
        Console.WriteLine($"Waiting for document {documentLogIdentifier} to be generated...");
        System.Net.HttpStatusCode status = await client.GetDocumentStatus(documentJobId);

        int notFoundCount = 0;
        // Status code 302 means the document is completed and ready to be retrieved
        while (status != (System.Net.HttpStatusCode)302)
        {
            Console.WriteLine($"Document {documentLogIdentifier} status: {status}");

            // If there is a 404 error, the document likely hasn't been written to persistent storage yet. We'll wait a bit and try again.
            if (status == (System.Net.HttpStatusCode)404)
            {
                notFoundCount++;
                if (notFoundCount > 5)
                {
                    Console.WriteLine($"Document {documentLogIdentifier} not found after 5 attempts");
                    return null;
                }
                Console.WriteLine($"Document {documentLogIdentifier} not found, retrying...");
            }
            // Check if there were any errors while processing the template
            if (status == (System.Net.HttpStatusCode)401 ||
                status == (System.Net.HttpStatusCode)500)
            {
                try
                {
                    ServerException err = await client.GetError(documentJobId);
                    Console.WriteLine("Error with request: " + documentLogIdentifier);
                    Console.WriteLine(err.Type);
                    Console.WriteLine(err.Message);
                    return null;
                }
                catch (Exception e)
                {
                    Console.WriteLine($"An error occurred getting document error: {documentLogIdentifier}");
                    Console.WriteLine(e);
                    return null;
                }
            }

            // Pause before checking the status again
            await Task.Delay(100);
            try
            {
                status = await client.GetDocumentStatus(documentJobId);
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred getting document status: {documentLogIdentifier}");
                Console.WriteLine(e);
                return null;
            }
            
        }
        Console.WriteLine($"Document {documentLogIdentifier} completed");
        // Retrieve the generated document, and remove the completed request from the server
        Document doc;
        try
        {
            doc = await client.GetDocument(documentJobId);
        }
        catch (Exception e)
        {
            Console.WriteLine($"An error occurred getting document: {documentLogIdentifier}");
            Console.WriteLine(e);
            return null;
        }

        Console.WriteLine($"Document {documentLogIdentifier} retrieved");
        try
        {
            await client.DeleteDocument(documentJobId);
        }
        catch (Exception e)
        {
            Console.WriteLine($"An error occurred attempting to delete the document {documentLogIdentifier} from the server. Still able to return generated document:");
            Console.WriteLine(e);
        }

        
        return doc;
    }
}
