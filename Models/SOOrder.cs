namespace PROFESICIPTA.Models
{
	public class SOOrder
	{
		public long SO_ORDER_ID { get; set; }
		public string? ORDER_NO { get; set; }
		public DateTime? ORDER_DATE { get; set; }
		public int? COM_CUSTOMER_ID { get; set; }
		public string? ADDRESS { get; set; }
		public COM_CUSTOMER COM_CUSTOMER { get; set; }
		public List<SO_ITEM> SOItems { get; set; } = new List<SO_ITEM>();
	}

}
