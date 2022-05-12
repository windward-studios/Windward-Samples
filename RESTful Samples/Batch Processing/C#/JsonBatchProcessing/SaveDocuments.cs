using System;
using System.IO;
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
	public void saveInvoiceDocument(Document doc, string name)
	{
		string filepath = Path.GetFullPath(saveDirectory + name + ".pdf");
		File.WriteAllBytes(filepath, doc.Data);
	}
}
