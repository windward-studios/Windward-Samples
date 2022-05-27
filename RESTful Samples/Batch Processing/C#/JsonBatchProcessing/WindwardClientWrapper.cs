using System;
using System.Threading.Tasks;
using WindwardRestApi.src.Api;
using WindwardRestApi.src.Model;

public class WindwardClientWrapper
{
	private WindwardClient client;
	public WindwardClientWrapper(Uri engineUrl, string licenseKey)
	{
		client = new WindwardClient(engineUrl);
		client.LicenseKey = licenseKey;
	}

    // Sends a request to process the given template to the RESTful engine
	public async Task<string> sendRequest(TemplateWrapper template)
	{
		Document doc = await client.PostDocument(template.GetTemplate());

        // Return the id associated with the request for later tracking/retrieval of
        // the completed request
		return doc.Guid;
	}

    // Creates an async task to periodically check the status of the given request,
    // noting any errors that occur and retrieving the completed document when 
    // generation has completed
	public async Task<Document> getDocument(string id)
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
                    return null;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return null;
                }
            }
            // Pause before checking the status again
            await Task.Delay(100);
            status = await client.GetDocumentStatus(id);
            continue;
        }

        // Retrieve the generated document, and remove the completed request from the server
        Document doc = await client.GetDocument(id);
        await client.DeleteDocument(id);
        return doc;
    }
}
