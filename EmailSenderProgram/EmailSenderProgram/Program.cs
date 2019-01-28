using EmailSenderProgram.MailComposer;
using EmailSenderProgram.Model;
using EmailSenderProgram.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailSenderProgram
{
	class Program
	{
		static void Main(string[] args)
		{
			SendMailAsync().GetAwaiter().GetResult();
			Console.ReadKey();
		}

		private static async Task SendMailAsync()
		{
			IMailComposer welcomeMailComposer = new WelcomeMailComposer();
			IMailComposer comebackMailComposer = new ComebackMailComposer("EOComebackToUs");

			List<Task<IEnumerable<IMailMessageInfo>>> tasks = new List<Task<IEnumerable<IMailMessageInfo>>>();
			tasks.Add(welcomeMailComposer.ComposeAsync());
#if DEBUG
			//Debug mode, always send Comeback mail
			tasks.Add(comebackMailComposer.ComposeAsync());
#else
			//Every Sunday run Comeback mail
			if (DateTime.Now.DayOfWeek.Equals(DayOfWeek.Monday))
			{
				tasks.Add(comebackMailComposer.ComposeAsync());
			}
#endif
			await Task.WhenAll(tasks.ToArray());

			using (IMailService mailService = new MailService("smtp.Org.com", 25))
			{
				List<IMailMessageInfo> messagesToSend = new List<IMailMessageInfo>();
				foreach (var task in tasks)
				{
					messagesToSend.AddRange(task.Result);
				}
				var mailSendingTask = mailService.SendBulkEmailAsync(messagesToSend);
				await mailSendingTask;

				//Check if the sending went OK
				if (mailSendingTask.Result)
				{
					Console.WriteLine("All mails are sent, I hope...");
				}
				else
				{
					Console.WriteLine("Oops, something went wrong when sending mail (I think...)");
				}
			}
		}
	}
}
