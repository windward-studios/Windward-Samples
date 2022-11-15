using System;
using System.IO;
using System.Threading.Tasks;
using WindwardRestApi.src.Model;

public class SaveDocuments
{
	private string saveDirectory;
	public SaveDocuments(string filepath)
	{
		saveDirectory = filepath;
	}

	// Saves the generated invoice to the location given,
	// naming the file according to the customer's last name
	public async Task SaveInvoiceDocument(Document doc, string name)
    {
        Directory.CreateDirectory(saveDirectory);
		string filepath = Path.GetFullPath(saveDirectory + name + ".pdf");
        await File.WriteAllBytesAsync(filepath, doc.Data);
	}
}
