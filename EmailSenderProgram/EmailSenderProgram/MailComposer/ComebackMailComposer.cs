using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using EmailSenderProgram.Model;

namespace EmailSenderProgram.MailComposer
{
	public class ComebackMailComposer : IMailComposer
	{
		private readonly string voucher;

		public ComebackMailComposer(string voucher)
		{
			this.voucher = voucher;
		}

		public string Voucher
		{
			get { return this.voucher; }
		}

		public async Task<IEnumerable<IMailMessageInfo>> ComposeAsync()
		{
			return await Task.FromResult(this.Compose());
		}

		public IEnumerable<IMailMessageInfo> Compose()
		{
			List<IMailMessageInfo> mailMessageList = new List<IMailMessageInfo>();

			//List all customers 
			List<Customer> customers = DataLayer.ListCustomers();
			//List all orders
			List<Order> orders = DataLayer.ListOrders();

			//Identify customer who hasn't put an order
			List<Customer> customerWitoutOrder = customers.Where(
				c => !orders.Any(o => o.CustomerEmail.Equals(c.Email, StringComparison.OrdinalIgnoreCase))).ToList();

			foreach (var customer in customerWitoutOrder)
			{
				//Create a new Mail Message Info
				MailMessageInfo msgInfo = new MailMessageInfo();
				//Add customer to reciever list
				msgInfo.AddMailIdToReceiverList(customer.Email);
				//Add subject
				msgInfo.Subject = "We miss you as a customer";
				//Send mail from info@EO.com
				msgInfo.From = "infor@EO.com";
				//Add body to mail
				msgInfo.Body = "Hi " + customer.Email +
						 "<br>We miss you as a customer. Our shop is filled with nice products. Here is a voucher that gives you 50 kr to shop for." +
						 "<br>Voucher: " + this.Voucher +
						 "<br><br>Best Regards,<br>EO Team";

				mailMessageList.Add(msgInfo);
			}

			return mailMessageList;
		}
	}
}
