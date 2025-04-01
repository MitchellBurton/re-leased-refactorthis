using RefactorThis.Persistence;
using System.Collections.Generic;

namespace RefactorThis.Domain.Tests
{
	public class TestInvoiceRepository : IInvoiceRepository
	{

		private Dictionary<string, Invoice> _invoices = new Dictionary<string, Invoice>();
		public void Add(Invoice invoice)
		{
			_invoices.Add(invoice.Reference, invoice);
		}

		public Invoice GetInvoice(string reference)
		{
			_invoices.TryGetValue(reference, out Invoice invoice);
			return invoice;
		}

		public void SaveInvoice(Invoice invoice)
		{
			// Save the invoice to the database
		}
	}
}
