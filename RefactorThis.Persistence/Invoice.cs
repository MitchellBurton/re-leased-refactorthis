using System.Collections.Generic;
using System.Linq;

namespace RefactorThis.Persistence
{
	public class Invoice
	{
		// This should be configured against each invoice encase the tax rate changes.
		public decimal TaxRate = 0.14m;
		public string Reference { get; set; }
		public decimal Amount { get; set; }

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

		// This is a change from existing behaviour, but I'm assuming that that behaviour is a bug.
		// The old behaviour was that the tax amount was only calculated when:
		// * the invoice did not already have any payments against it (i.e. it was the first payment)
		// * the invoice was a commercial invoice.

		// The logic around calculating the tax amount only for commercial invoices makes me thing that
		// the tax amount should only calculated for commercial invoices. So I have made that assumption.

		// If that assumption is incorrect, then the test should be updated to reflect the correct behaviour.

		// Also, the tax amount is calulated on the total invoice amount, not the amount paid.
		public decimal TaxAmount
		{
			get
			{
				if (Type == InvoiceType.Commercial)
				{
					return Amount * TaxRate;
				}
				return 0;
			}
		}
		public List<Payment> Payments { get; set; }

		public InvoiceType Type { get; set; }
	}

	public enum InvoiceType
	{
		Standard,
		Commercial
	}
}