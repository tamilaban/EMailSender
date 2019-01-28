using EmailSenderProgram.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailSenderProgram.MailComposer
{
	public class WelcomeMailComposer : IMailComposer
	{
		public async Task<IEnumerable<IMailMessageInfo>> ComposeAsync()
		{
			return await Task.FromResult(this.Compose());
		}

		public IEnumerable<IMailMessageInfo> Compose()
		{
			List<IMailMessageInfo> mailMessageList = new List<IMailMessageInfo>();

			//List all customers 
			List<Customer> customers = DataLayer.ListCustomers();

			//List all new customers
			List<Customer> newCustomers = customers.Where(c => c.CreatedDateTime > DateTime.Now.AddDays(-1)).ToList();

			//loop through list of new customers
			foreach (var customer in newCustomers)
			{
				//Create a new MailMessage
				MailMessageInfo msgInfo = new MailMessageInfo();
				//Add customer to reciever list
				msgInfo.AddMailIdToReceiverList(customer.Email);
				//Add subject
				msgInfo.Subject = "Welcome as a new customer at EO!";
				//Send mail from info@EO.com
				msgInfo.From = "info@EO.com";
				//Add body to mail
				msgInfo.Body = "Hi " + customer.Email +
						 "<br>We would like to welcome you as customer on our site!<br><br>Best Regards,<br>EO Team";

				mailMessageList.Add(msgInfo);
			}

			return mailMessageList;
		}
	}
}
