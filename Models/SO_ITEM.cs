using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PROFESICIPTA.Models
{
	public class SO_ITEM
	{
		public long SO_ITEM_ID { get; set; }
		public long SO_ORDER_ID { get; set; }
		public string? ITEM_NAME { get; set; }
		public int? QUANTITY { get; set; }
		[Required]
		[Range(0, double.MaxValue, ErrorMessage = "Price must be a positive number.")]
		public double PRICE { get; set; }
		[NotMapped]
		public double TOTAL { get; set; }
		[NotMapped]
		public int ItemIndex { get; set; }

	}

}
