namespace PROFESICIPTA.Models
{
	public class COM_CUSTOMER
	{
		public int COM_CUSTOMER_ID { get; set; }
		public string? CUSTOMER_NAME { get; set; }
		public ICollection<SOOrder> SOOrders { get; set; }
	}

}
