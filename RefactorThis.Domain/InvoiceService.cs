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

			// If the invoice has existing payments, check if the payment is a partial payment or the final payment.
			if (inv.Payments != null && inv.Payments.Any())
			{
				if (inv.Payments.Sum(x => x.Amount) != 0 && inv.Amount == inv.Payments.Sum(x => x.Amount))
				{
					return "invoice was already fully paid";
				}
				if (inv.Payments.Sum(x => x.Amount) != 0 && payment.Amount > (inv.Amount - inv.GetAmountPaid()))
				{
					return "the payment is greater than the partial amount remaining";
				}

				// Assume that the payment is a partial payment, change the message if it is the final payment.
				var partialPaymentMessage = "another partial payment received, still not fully paid";
				if ((inv.Amount - inv.GetAmountPaid()) == payment.Amount)
				{
					partialPaymentMessage = "final partial payment received, invoice is now fully paid";
				}

				if (inv.Type == InvoiceType.Commercial)
				{
					inv.TaxAmount += payment.Amount * 0.14m;
				}
				inv.Payments.Add(payment);
				_invoiceRepository.SaveInvoice(inv);
				return partialPaymentMessage;
			}

			// If the invoice has no existing payments, check if the payment is more then the invoice amount.
			// Don't save the invoice, processing up the chain somewhere will have to reject this payment.
			if (payment.Amount > inv.Amount)
			{
				return "the payment is greater than the invoice amount";
			}

			// If the invoice has no existing payments, check if the payment is the final payment.
			var fullPaymentMessage = "invoice is now partially paid";
			if (inv.Amount == payment.Amount)
			{
				fullPaymentMessage = "invoice is now fully paid";
			}
			
			inv.TaxAmount = payment.Amount * 0.14m;
			inv.Payments.Add(payment);
			_invoiceRepository.SaveInvoice(inv);
			return fullPaymentMessage;
		}
	}
}