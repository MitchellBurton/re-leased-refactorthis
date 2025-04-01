using System;
using System.Linq;
using RefactorThis.Persistence;

namespace RefactorThis.Domain
{
	public class InvoiceService
	{
		private readonly IInvoiceRepository _invoiceRepository;

		public InvoiceService(IInvoiceRepository invoiceRepository)
		{
			_invoiceRepository = invoiceRepository;
		}

		public string ProcessPayment(Payment payment)
		{
			CheckPaymentValidity(payment);

			var inv = _invoiceRepository.GetInvoice(payment.Reference);

			CheckInvoiceValidity(inv);


			if (inv.Amount == 0)
			{
				return "no payment needed";
			}

			// We know the invoice is valid, now we can process the payment.
			var remainingAmount = inv.Amount - inv.GetAmountPaid();

			// Invoice is already fully paid, no need to process the payment.
			// Don't save the invoice, processing up the chain somewhere will have to reject this payment.
			if (inv.GetAmountPaid() != 0 && remainingAmount <= 0)
			{
				return "invoice was already fully paid";
			}

			// The payment is greater than the amount remaining.
			// Don't save the invoice, processing up the chain somewhere will have to reject this payment.
			if (payment.Amount > remainingAmount)
			{
				if (inv.GetAmountPaid() != 0)
				{
					return "the payment is greater than the partial amount remaining";
				}

				return "the payment is greater than the invoice amount";
			}


			// Payment will be processed and the invoice will be saved.

			// Assume that the payment is a partial payment, change the message if it is the final payment.
			var paymentProcessedMessage = "invoice is now partially paid";
			var hasExistingPayments = inv.GetAmountPaid() != 0;
			var isFinalPayment = remainingAmount == payment.Amount;

			// If the invoice has existing payments, check if the payment is a partial payment or the final payment.
			if (hasExistingPayments)
			{
				// Assume that the payment is a partial payment, change the message if it is the final payment.
				paymentProcessedMessage = "another partial payment received, still not fully paid";
				if (isFinalPayment)
				{
					paymentProcessedMessage = "final partial payment received, invoice is now fully paid";
				}
			}
			// If the invoice has no existing payments, check if the payment is the final payment.
			else if (isFinalPayment)
			{
				paymentProcessedMessage = "invoice is now fully paid";
			}

			inv.Payments.Add(payment);
			_invoiceRepository.SaveInvoice(inv);
			return paymentProcessedMessage;
		}

		private static void CheckInvoiceValidity(Invoice inv)
		{
			// Check if the invoice is in a valid state.
			if (inv == null)
			{
				throw new InvalidOperationException("There is no invoice matching this payment");
			}

			if (inv.Amount == 0 && (inv.Payments != null && inv.Payments.Any()))
			{
				throw new InvalidOperationException("The invoice is in an invalid state, it has an amount of 0 and it has payments.");
			}

			if (inv.Type != InvoiceType.Standard && inv.Type != InvoiceType.Commercial)
			{
				throw new ArgumentOutOfRangeException("The invoice is in an invalid state, it has an invalid type.");
			}
		}

		private static void CheckPaymentValidity(Payment payment)
		{
			// Check the payment is in a valid state.
			if (payment.Amount <= 0)
			{
				throw new InvalidOperationException("The payment amount must be greater than 0");
			}
			if (string.IsNullOrWhiteSpace(payment.Reference))
			{
				throw new InvalidOperationException("The payment reference must be provided");
			}
		}
	}
}