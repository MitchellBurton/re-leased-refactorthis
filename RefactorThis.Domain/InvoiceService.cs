using System;
using System.Linq;
using RefactorThis.Persistence;

namespace RefactorThis.Domain
{
	public class InvoiceService
	{
		private readonly InvoiceRepository _invoiceRepository;

		public InvoiceService(InvoiceRepository invoiceRepository)
		{
			_invoiceRepository = invoiceRepository;
		}

		public string ProcessPayment(Payment payment)
		{
			var inv = _invoiceRepository.GetInvoice(payment.Reference);

			// Check if the invoice is in a valid state.
			if (inv == null)
			{
				throw new InvalidOperationException("There is no invoice matching this payment");
			}

			if (inv.Amount == 0)
			{
				if (inv.Payments == null || !inv.Payments.Any())
				{
					return "no payment needed";
				}
				else
				{
					throw new InvalidOperationException("The invoice is in an invalid state, it has an amount of 0 and it has payments."); // Test for this
				}
			}

			if (inv.Type != InvoiceType.Standard && inv.Type != InvoiceType.Commercial)
			{
				throw new ArgumentOutOfRangeException("The invoice is in an invalid state, it has an invalid type."); // Test for this
			}


			// We know the invoice is valid, now we can process the payment.


			if (inv.Payments != null && inv.Payments.Any())
			{
				if (inv.Payments.Sum(x => x.Amount) != 0 && inv.Amount == inv.Payments.Sum(x => x.Amount))
				{
					return "invoice was already fully paid";
				}
				else if (inv.Payments.Sum(x => x.Amount) != 0 && payment.Amount > (inv.Amount - inv.AmountPaid))
				{
					return "the payment is greater than the partial amount remaining";
				}
				else
				{
					if ((inv.Amount - inv.AmountPaid) == payment.Amount)
					{
						inv.AmountPaid += payment.Amount;
						if (inv.Type == InvoiceType.Commercial)
						{
							inv.TaxAmount += payment.Amount * 0.14m;
						}
						inv.Payments.Add(payment);
						inv.Save();
						return "final partial payment received, invoice is now fully paid"; // Need to test for both cases?
					}
					else
					{
						inv.AmountPaid += payment.Amount;
						if (inv.Type == InvoiceType.Commercial)
						{
							inv.TaxAmount += payment.Amount * 0.14m;
						}
						inv.Payments.Add(payment);
						inv.Save();
						return "another partial payment received, still not fully paid"; // Need to test for both cases?
					}
				}
			}
			else
			{
				if (payment.Amount > inv.Amount)
				{
					return "the payment is greater than the invoice amount";
				}
				else if (inv.Amount == payment.Amount)
				{
					inv.AmountPaid = payment.Amount;
					inv.TaxAmount = payment.Amount * 0.14m; // Tax shouldn't be added for both types?
					inv.Payments.Add(payment);
					inv.Save();
					return "invoice is now fully paid"; // Need to test for both cases?
					
				}
				else
				{
					inv.AmountPaid = payment.Amount;
					inv.TaxAmount = payment.Amount * 0.14m; // Tax shouldn't be added for both types?
					inv.Payments.Add(payment);
					inv.Save();
					return "invoice is now partially paid"; // Need to test for both cases?
				}
			}
		}
	}
}