using Microsoft.AspNetCore.Mvc.Rendering;

namespace PROFESICIPTA.ViewModels
{
	public class SalesOrderViewModel
	{
		public string OrderNo { get; set; }
		public DateTime OrderDate { get; set; }
		public int CustomerId { get; set; }
		public string Address { get; set; }
		public List<OrderItemViewModel> Items { get; set; }
		public SelectList Customers { get; set; } 
	}

	public class OrderItemViewModel
	{
		public string ItemName { get; set; }
		public float Qty { get; set; }
	}
}
				