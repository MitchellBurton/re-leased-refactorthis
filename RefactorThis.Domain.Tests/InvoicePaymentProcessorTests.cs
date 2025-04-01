using System;
using System.Collections.Generic;
using NUnit.Framework;
using RefactorThis.Persistence;

namespace RefactorThis.Domain.Tests
{
	[TestFixture]
	public class InvoicePaymentProcessorTests
	{
		[Test]
		public void ProcessPayment_Should_ThrowException_When_NoInoviceFoundForPaymentReference()
		{
			var repo = new TestInvoiceRepository();
			var paymentProcessor = new InvoiceService(repo);

			var payment = new Payment() { Amount = 10, Reference = "123" };
			var failureMessage = "";

			try
			{
				var result = paymentProcessor.ProcessPayment(payment);
			}
			catch (InvalidOperationException e)
			{
				failureMessage = e.Message;
			}

			Assert.AreEqual("There is no invoice matching this payment", failureMessage);
		}

		[Test]
		public void ProcessPayment_Should_ThrowException_When_PaymentDoesNotHaveAReference()
		{

			var repo = new TestInvoiceRepository();

			var invoice = new Invoice()
			{
				Reference = "123",
				Amount = 10,
				Payments = null
			};

			repo.Add(invoice);

			var paymentProcessor = new InvoiceService(repo);

			var payment = new Payment() { Amount = 1 };
			var failureMessage = "";
			try
			{
				var result = paymentProcessor.ProcessPayment(payment);
			}
			catch (InvalidOperationException e)
			{
				failureMessage = e.Message;
			}

			Assert.AreEqual("The payment reference must be provided", failureMessage);
		}

		[Test]
		public void ProcessPayment_Should_ReturnFailureMessage_When_NoPaymentNeeded()
		{
			var repo = new TestInvoiceRepository();

			var invoice = new Invoice()
			{
				Reference = "123",
				Amount = 0,
				Payments = null
			};

			repo.Add(invoice);

			var paymentProcessor = new InvoiceService(repo);

			var payment = new Payment() { Reference = "123", Amount = 1 };

			var result = paymentProcessor.ProcessPayment(payment);

			Assert.AreEqual("no payment needed", result);
			Assert.AreEqual(0, invoice.GetAmountPaid()); // Check that the payment was not applied to the invoice.
		}

		[Test]
		public void ProcessPayment_Should_ReturnFailureMessage_When_PaymentInvalid()
		{
			var repo = new TestInvoiceRepository();

			var invoice = new Invoice()
			{
				Reference = "123",
				Amount = 10,
				Payments = null
			};

			repo.Add(invoice);

			var paymentProcessor = new InvoiceService(repo);

			var payment = new Payment();

			var failureMessage = "";
			try
			{
				var result = paymentProcessor.ProcessPayment(payment);
			}
			catch (InvalidOperationException e)
			{
				failureMessage = e.Message;
			}

			Assert.AreEqual("The payment amount must be greater than 0", failureMessage);
			Assert.AreEqual(0, invoice.GetAmountPaid()); // Check that the payment was not applied to the invoice.
		}

		[Test]
		public void ProcessPayment_Should_ReturnFailureMessage_When_InvoiceAlreadyFullyPaid()
		{
			var repo = new TestInvoiceRepository();

			var invoice = new Invoice()
			{
				Reference = "123",
				Amount = 10,
				Payments = new List<Payment>
					{
						new Payment
						{
							Amount = 10
						}
					}
			};
			repo.Add(invoice);

			var paymentProcessor = new InvoiceService(repo);

			var payment = new Payment() { Reference = "123", Amount = 1 };

			var result = paymentProcessor.ProcessPayment(payment);

			Assert.AreEqual("invoice was already fully paid", result);
			Assert.AreEqual(10, invoice.GetAmountPaid()); // Check that the payment was not applied to the invoice.
		}

		[Test]
		public void ProcessPayment_Should_ReturnFailureMessage_When_PartialPaymentExistsAndAmountPaidExceedsAmountDue()
		{
			var repo = new TestInvoiceRepository();
			var invoice = new Invoice()
			{
				Reference = "123",
				Amount = 10,
				Payments = new List<Payment>
					{
						new Payment
						{
							Amount = 5
						}
					}
			};
			repo.Add(invoice);

			var paymentProcessor = new InvoiceService(repo);

			var payment = new Payment()
			{
				Reference = "123",
				Amount = 6
			};

			var result = paymentProcessor.ProcessPayment(payment);

			Assert.AreEqual("the payment is greater than the partial amount remaining", result);
			Assert.AreEqual(5, invoice.GetAmountPaid()); // Check that the payment was not applied to the invoice.
		}

		[Test]
		public void ProcessPayment_Should_ReturnFailureMessage_When_NoPartialPaymentExistsAndAmountPaidExceedsInvoiceAmount()
		{
			var repo = new TestInvoiceRepository();
			var invoice = new Invoice()
			{
				Reference = "123",
				Amount = 5,
				Payments = new List<Payment>()
			};
			repo.Add(invoice);

			var paymentProcessor = new InvoiceService(repo);

			var payment = new Payment()
			{
				Reference = "123",
				Amount = 6
			};

			var result = paymentProcessor.ProcessPayment(payment);

			Assert.AreEqual("the payment is greater than the invoice amount", result);
			Assert.AreEqual(0, invoice.GetAmountPaid()); // Check that the payment was not applied to the invoice.
		}

		[Test]
		public void ProcessPayment_Should_ReturnFullyPaidMessage_When_PartialPaymentExistsAndAmountPaidEqualsAmountDue()
		{
			var repo = new TestInvoiceRepository();
			var invoice = new Invoice()
			{
				Reference = "123",
				Amount = 10,
				Payments = new List<Payment>
					{
						new Payment
						{
							Reference = "123",
							Amount = 5
						}
					}
			};
			repo.Add(invoice);

			var paymentProcessor = new InvoiceService(repo);

			var payment = new Payment()
			{
				Reference = "123",
				Amount = 5
			};

			var result = paymentProcessor.ProcessPayment(payment);

			Assert.AreEqual("final partial payment received, invoice is now fully paid", result);
			Assert.AreEqual(10, invoice.GetAmountPaid()); // Check that the payment was applied to the invoice.
		}

		[Test]
		public void ProcessPayment_Should_ReturnFullyPaidMessage_When_NoPartialPaymentExistsAndAmountPaidEqualsInvoiceAmount()
		{
			var repo = new TestInvoiceRepository();
			var invoice = new Invoice()
			{
				Reference = "123",
				Amount = 10,
				Payments = new List<Payment>() { new Payment() { Amount = 10 } }
			};
			repo.Add(invoice);

			var paymentProcessor = new InvoiceService(repo);

			var payment = new Payment()
			{
				Reference = "123",
				Amount = 10
			};

			var result = paymentProcessor.ProcessPayment(payment);

			Assert.AreEqual("invoice was already fully paid", result);
			Assert.AreEqual(10, invoice.GetAmountPaid()); // Check that the payment wasn't applied to the invoice.
		}

		[Test]
		public void ProcessPayment_Should_ReturnPartiallyPaidMessage_When_PartialPaymentExistsAndAmountPaidIsLessThanAmountDue()
		{
			var repo = new TestInvoiceRepository();
			var invoice = new Invoice()
			{
				Reference = "123",
				Amount = 10,
				Payments = new List<Payment>
					{
						new Payment
						{
							Amount = 5
						}
					}
			};
			repo.Add(invoice);

			var paymentProcessor = new InvoiceService(repo);

			var payment = new Payment()
			{
				Reference = "123",
				Amount = 1
			};

			var result = paymentProcessor.ProcessPayment(payment);

			Assert.AreEqual("another partial payment received, still not fully paid", result);
		}

		[Test]
		public void ProcessPayment_Should_ReturnPartiallyPaidMessage_When_NoPartialPaymentExistsAndAmountPaidIsLessThanInvoiceAmount()
		{
			var repo = new TestInvoiceRepository();
			var invoice = new Invoice()
			{
				Reference = "123",
				Amount = 10,
				Payments = new List<Payment>()
			};
			repo.Add(invoice);

			var paymentProcessor = new InvoiceService(repo);

			var payment = new Payment()
			{
				Reference = "123",
				Amount = 1
			};

			var result = paymentProcessor.ProcessPayment(payment);

			Assert.AreEqual("invoice is now partially paid", result);
		}
	}
}