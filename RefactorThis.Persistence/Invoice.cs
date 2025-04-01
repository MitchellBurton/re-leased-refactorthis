using System.Collections.Generic;
using System.Linq;

namespace RefactorThis.Persistence
{
	public class Invoice
	{
		public decimal Amount { get; set; }
		private readonly decimal amountPaid;

		// Turning this into a calculated property assumes that we don't need to
		// store the value in the database.
		// And that the payments will not be a large list, impacting performance.
		public decimal GetAmountPaid()
		{
			if (Payments == null)
			{
				return 0;
			}
			return Payments.Sum(x => x.Amount);
		}

		public decimal TaxAmount { get; set; }
		public List<Payment> Payments { get; set; }
		
		public InvoiceType Type { get; set; }
	}

	public enum InvoiceType
	{
		Standard,
		Commercial
	}
}