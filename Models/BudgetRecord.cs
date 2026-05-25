using System;

namespace BudgetTracker.Models
{
	public class BudgetRecord
	{
		public int Id { get; set; }
		public string Type { get; set; }
		public string Category { get; set; }
		public string Tag { get; set; }
		public decimal Amount { get; set; }
		public DateTime Date { get; set; }
		public string Description { get; set; }
	}
}