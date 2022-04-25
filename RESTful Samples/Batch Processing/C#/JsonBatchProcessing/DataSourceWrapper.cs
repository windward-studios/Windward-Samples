using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WindwardRestApi.src.Model;

public class DataSourceWrapper
{
	private List<DataSource> dataSources = new List<DataSource>();

	// Process the orders in the given JSON file, creating a
	// datasource object for each order and retrieving the name of 
	// the customer associated with that order
	public List<string> processOrders(string filepath)
	{
		string records = File.ReadAllText(filepath);

		// Parse the JSON into a list of orders
		JObject orderJson = JObject.Parse(records);
		IList<JToken> orders = orderJson["Orders"].Children().ToList();
		List<string> names = new List<string>();
		foreach (JToken order in orders)
		{
			// Extract the customer's last name from the order
			names.Add(order.ToObject<Order>().LastName);

			// Convert the record into a byte array to be sent to the server
			byte[] data = System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(order));

			// Create a datasource for the order
			addJsonDataSource("Order", data);
		}
		return names;
	}
	public void addJsonDataSource(string name, byte[] data)
	{
		dataSources.Add(new JsonDataSource(name, data));
	}

	public List<DataSource> GetDataSources()
	{
		return dataSources;
	}
}
