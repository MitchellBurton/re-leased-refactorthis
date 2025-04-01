using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace RefactorThis.Persistence.Tests
{
	[TestFixture]
	public class InvoiceTests
	{
		[Test]
		public void Invoice_Should_NotAddTaxForStandardInvoices()
		{
			var invoice = new Invoice()
			{
				Reference = "123",
				Amount = 10,
				Type = InvoiceType.Standard,
				Payments = new List<Payment>()
			};
			
			Assert.AreEqual(0, invoice.TaxAmount);
		}

		[Test]
		public void Invoice_Should_AddTaxForCommercialInvoices()
		{
			var invoice = new Invoice()
			{
				Reference = "123",
				Amount = 10,
				Type = InvoiceType.Commercial,
				Payments = new List<Payment>()
			};

			Assert.AreEqual(1.4m, invoice.TaxAmount);
		}

		[Test]
		public void Invoice_Should_AllowTaxRateOverrideForCommercialInvoices()
		{
			var invoice = new Invoice()
			{
				Reference = "123",
				Amount = 10,
				Type = InvoiceType.Commercial,
				TaxRate = 0.2m,
				Payments = new List<Payment>()
			};

			Assert.AreEqual(2m, invoice.TaxAmount);
		}
	}
}