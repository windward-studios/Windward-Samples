using System.Collections.Generic;

public class Order
{
    public string LastName { get; set; }
    public string FirstName { get; set; }
    public double OrderTotal { get; set; }
    public List<Item> ItemList { get; set; }
}