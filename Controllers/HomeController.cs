using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PROFESICIPTA.Data;
using PROFESICIPTA.Models;
using System.Text;
using System.IO;
using System.Xml;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc.Rendering;
using PROFESICIPTA.ViewModels;

namespace PROFESICIPTA.Controllers
{
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;
		private readonly ApplicationDbContext _context;

		public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
		{
			_logger = logger;
			_context = context;  
		}

		public IActionResult Index(string searchQuery, string orderDate)
		{
			var orders = _context.SO_ORDER
				.Join(_context.COM_CUSTOMER,
					order => order.COM_CUSTOMER_ID,
					customer => customer.COM_CUSTOMER_ID,
					(order, customer) => new { order, customer })
				.AsQueryable();

			if (!string.IsNullOrEmpty(searchQuery))
			{
				orders = orders.Where(o => o.order.ORDER_NO.Contains(searchQuery) || o.customer.CUSTOMER_NAME.Contains(searchQuery));
			}

			if (!string.IsNullOrEmpty(orderDate))
			{
				var parsedDate = DateTime.ParseExact(orderDate, "dd/MM/yyyy", null);
				orders = orders.Where(o => o.order.ORDER_DATE.HasValue && o.order.ORDER_DATE.Value.Date == parsedDate.Date);
			}

			var orderList = orders.Select(o => new SOOrder
			{
				SO_ORDER_ID = o.order.SO_ORDER_ID,
				ORDER_NO = o.order.ORDER_NO,
				ORDER_DATE = o.order.ORDER_DATE,
				COM_CUSTOMER = new COM_CUSTOMER
				{
					CUSTOMER_NAME = o.customer.CUSTOMER_NAME
				}
			}).ToList();

			return View(orderList);
		}

		public IActionResult DownloadExcel(string searchQuery, string orderDate)
		{
			var orders = _context.SO_ORDER
				.Join(_context.COM_CUSTOMER,
					order => order.COM_CUSTOMER_ID,
					customer => customer.COM_CUSTOMER_ID,
					(order, customer) => new { order, customer })
				.AsQueryable();

			if (!string.IsNullOrEmpty(searchQuery))
			{
				orders = orders.Where(o => o.order.ORDER_NO.Contains(searchQuery) || o.customer.CUSTOMER_NAME.Contains(searchQuery));
			}

			if (!string.IsNullOrEmpty(orderDate))
			{
				if (DateTime.TryParseExact(orderDate, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out var parsedDate))
				{
					orders = orders.Where(o => o.order.ORDER_DATE.HasValue && o.order.ORDER_DATE.Value.Date == parsedDate.Date);
				}
				else
				{
					ModelState.AddModelError("InvalidDate", "Invalid Date Format");
					return View("Index");
				}
			}

			var orderList = orders.Select(o => new SOOrder
			{
				SO_ORDER_ID = o.order.SO_ORDER_ID,
				ORDER_NO = o.order.ORDER_NO,
				ORDER_DATE = o.order.ORDER_DATE,
				COM_CUSTOMER = new COM_CUSTOMER
				{
					CUSTOMER_NAME = o.customer.CUSTOMER_NAME
				}
			}).ToList();  


			if (!orderList.Any())
			{
				return Content("No data found for the specified filter.");
			}

			using (var workbook = new XLWorkbook())
			{
				var worksheet = workbook.Worksheets.Add("Orders");
				var currentRow = 1;

				worksheet.Cell(currentRow, 1).Value = "SO Order ID";
				worksheet.Cell(currentRow, 2).Value = "Order No";
				worksheet.Cell(currentRow, 3).Value = "Order Date";
				worksheet.Cell(currentRow, 4).Value = "Customer Name";

				foreach (var order in orderList)
				{
					currentRow++;
					worksheet.Cell(currentRow, 1).Value = order.SO_ORDER_ID;
					worksheet.Cell(currentRow, 2).Value = order.ORDER_NO;
					worksheet.Cell(currentRow, 3).Value = order.ORDER_DATE?.ToString("dd/MM/yyyy");
					worksheet.Cell(currentRow, 4).Value = order.COM_CUSTOMER.CUSTOMER_NAME;
				}

				using (var memoryStream = new MemoryStream())
				{
					workbook.SaveAs(memoryStream);
					memoryStream.Seek(0, SeekOrigin.Begin);
					return File(memoryStream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Orders_" + DateTime.Now.ToString("yyyyMMdd") + ".xlsx");
				}
			}
		}

		public IActionResult Add()
		{
			var obj = new SOOrder();
			ViewBag.Mode = "Create";

			ViewBag.CustomerList = _context.COM_CUSTOMER.ToList();

			return View("Add", obj);
		}

		[HttpPost]
		public async Task<IActionResult> Add(string ORDER_NO, DateTime ORDER_DATE, int COM_CUSTOMER_ID, string ADDRESS, List<SO_ITEM> items)
		{
			try
			{
				var newOrder = new SOOrder
				{
					ORDER_NO = ORDER_NO,
					ORDER_DATE = ORDER_DATE,
					COM_CUSTOMER_ID = COM_CUSTOMER_ID,
					ADDRESS = ADDRESS
				};

				await _context.SO_ORDER.AddAsync(newOrder);

				await _context.SaveChangesAsync();

				foreach (var item in items)
				{
					item.SO_ORDER_ID = newOrder.SO_ORDER_ID; 
					await _context.SO_ITEM.AddAsync(item); 
				}

				await _context.SaveChangesAsync();

				return RedirectToAction("Index");
			}
			catch (Exception ex)
			{
				ModelState.AddModelError(string.Empty, ex.Message);
			}

			ViewBag.Mode = "Add";
			return View();
		}

		[HttpPost]
		public JsonResult DeleteSOOrder(int id)
		{
			try
			{
				var order = _context.SO_ORDER.FirstOrDefault(o => o.SO_ORDER_ID == id);
				if (order == null)
				{
					return Json(new { success = false, message = "Order not found" });
				}

				_context.SO_ORDER.Remove(order);
				_context.SaveChanges();

				return Json(new { success = true, message = "Order deleted successfully" });
			}
			catch (Exception ex)
			{
				// Log the exception (optional)
				return Json(new { success = false, message = "Error: " + ex.Message });
			}
		}


		public async Task<IActionResult> Edit(long? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var sOOrder = await _context.SO_ORDER
			.Include(o => o.COM_CUSTOMER)
			.Include(o => o.SOItems)
			.Select(o => new SOOrder
			{
				SO_ORDER_ID = o.SO_ORDER_ID,
				COM_CUSTOMER_ID = o.COM_CUSTOMER_ID,
				ORDER_NO = o.ORDER_NO,
				ADDRESS = o.ADDRESS,
				ORDER_DATE = o.ORDER_DATE,
				SOItems = o.SOItems.Select(i => new SO_ITEM
				{
					SO_ITEM_ID = i.SO_ITEM_ID,
					ITEM_NAME = i.ITEM_NAME,
					QUANTITY = i.QUANTITY,  
					PRICE = Convert.ToDouble(i.PRICE),  
					TOTAL = Convert.ToDouble(i.TOTAL)    
				}).ToList()
			})
			.FirstOrDefaultAsync(o => o.SO_ORDER_ID == id);

			if (sOOrder == null)
			{
				return NotFound();
			}

			ViewBag.CustomerList = await _context.COM_CUSTOMER.ToListAsync();
			ViewBag.SelectedCustomerID = sOOrder.COM_CUSTOMER_ID;

			return View(sOOrder);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(long id, SOOrder updatedOrder, List<SO_ITEM> updatedItems)
		{
			if (id != updatedOrder.SO_ORDER_ID)
			{
				return NotFound();
			}

			try
			{
				var existingOrder = await _context.SO_ORDER
					.Include(o => o.SOItems)  
					.FirstOrDefaultAsync(o => o.SO_ORDER_ID == id);

				if (existingOrder == null)
				{
					return NotFound();
				}

				existingOrder.ORDER_NO = updatedOrder.ORDER_NO;
				existingOrder.ORDER_DATE = updatedOrder.ORDER_DATE;
				existingOrder.ADDRESS = updatedOrder.ADDRESS;
				existingOrder.COM_CUSTOMER_ID = updatedOrder.COM_CUSTOMER_ID;

				foreach (var updatedItem in updatedItems)
				{
					var existingItem = existingOrder.SOItems
						.FirstOrDefault(i => i.SO_ITEM_ID == updatedItem.SO_ITEM_ID);

					if (existingItem != null)
					{
						existingItem.ITEM_NAME = updatedItem.ITEM_NAME;
						existingItem.QUANTITY = updatedItem.QUANTITY;
						existingItem.PRICE = updatedItem.PRICE;
						existingItem.TOTAL = updatedItem.TOTAL;
					}
				}

				_context.Update(existingOrder);  
				_context.UpdateRange(existingOrder.SOItems); 

				await _context.SaveChangesAsync();

				return RedirectToAction(nameof(Index));  
			}
			catch (Exception ex)
			{
				ModelState.AddModelError(string.Empty, ex.Message);
				return View(updatedOrder);
			}
		}

		private bool SOOrderExists(long id)
		{
			return _context.SO_ORDER.Any(e => e.SO_ORDER_ID == id);
		}


		public IActionResult Privacy()
		{
			return View();
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}
