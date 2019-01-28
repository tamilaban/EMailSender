using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EmailSenderProgram.Model;
using System.Net.Mail;
using System.Threading;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace EmailSenderProgram.Services
{
	public class MailService : IMailService, IDisposable
	{
		//Create maximum of 15 SMTP connections to the server.
		//This finite number of connection will elliminate the need for creating
		//a new SMTP connection for every email being sent and will not create
		//some sort of DoS (Denial-of-service) attack 
		private const int clientCount = 16;

		private SmtpClient[] smtpClients = new SmtpClient[clientCount + 1];
		private CancellationTokenSource cancellationToken;

		// Flag: Has Dispose already been called?
		bool disposed = false;
		// Instantiate a SafeHandle instance.
		SafeHandle handle = new SafeFileHandle(IntPtr.Zero, true);

		public MailService(string host, int port)
		{
			this.Host = host;
			this.Port = port;

#if !DEBUG
			// No need to connect with SMTP server in debug mode
			SetupSMTPClients();
#endif
		}

		public MailService(string host, int port, string userName, string password, string domain)
		{
			this.Host = host;
			this.Port = port;
			this.UserName = userName;
			this.Password = password;
			this.Domain = domain;

			SetupSMTPClients();
		}

		public string Host { get; private set; }

		public int Port { get; private set; }

		public string UserName { get; private set; }

		public string Password { get; private set; }

		public string Domain { get; private set; }

		private void SetupSMTPClients()
		{
			for (int i = 0; i <= clientCount; i++)
			{
				try
				{
					SmtpClient client = new SmtpClient(this.Host, this.Port);
					if (string.IsNullOrEmpty(this.UserName))
					{
						client.UseDefaultCredentials = false;
					}
					else
					{
						client.Credentials = new System.Net.NetworkCredential(this.UserName, this.Password, this.Domain);
					}

					this.smtpClients[i] = client;
				}
				catch (Exception ex)
				{
					//Log exception in log handler
					throw (ex);
				}
			}
		}

		public async Task<bool> SendBulkEmailAsync(IEnumerable<IMailMessageInfo> mailMessageInfos)
		{
			return await Task.FromResult(this.SendBulkEmail(mailMessageInfos));
		}

		public void CancelBulkEmailAsync()
		{
			this.cancellationToken.Cancel();
		}

		private bool SendBulkEmail(IEnumerable<IMailMessageInfo> mailMessageInfos)
		{
			bool result = true;
			ParallelOptions po = new ParallelOptions();
			//Cancellation token to can cancel the task.
			this.cancellationToken = new CancellationTokenSource();
			po.CancellationToken = this.cancellationToken.Token;
			//Manually Manage the MaxDegreeOfParallelism instead of .NET Managing this. 
			//We dont need to create separate thread for each mail sending.
			po.MaxDegreeOfParallelism = System.Environment.ProcessorCount;
			try
			{
				Parallel.ForEach(mailMessageInfos, po, (IMailMessageInfo mailInfo) =>
				{
					try
					{
						MailMessage msg = new MailMessage();
						msg.From = new MailAddress(mailInfo.From);
						foreach (var toAddress in mailInfo.To)
						{
							msg.To.Add(new MailAddress(toAddress));
						}
						msg.Subject = mailInfo.Subject;
						msg.Body = mailInfo.Body;
						msg.Priority = MailPriority.Normal;
						SendEmail(msg);
					}
					catch (Exception ex)
					{
						// Log exception
						result = false;
					}
				});
			}
			catch (OperationCanceledException e)
			{
				//User has cancelled this request.
				result = false;
			}

			return result;
		}

		private void SendEmail(MailMessage msg)
		{
#if !DEBUG
			try
			{
				bool locked = false;
				while (!locked)
				{
					//Keep looping through all smtp client connections until one becomes available
					for (int i = 0; i <= clientCount; i++)
					{
						if (Monitor.TryEnter(this.smtpClients[i]))
						{
							try
							{
								this.smtpClients[i].Send(msg);
							}
							finally
							{
								Monitor.Exit(this.smtpClients[i]);
							}
							locked = true;
							break;
						}
					}

					//Do this to make sure CPU usage doesn't ramp up to 100%
					Thread.Sleep(5);
				}
			}
			finally
			{
				msg.Dispose();
			}
#else
			string receiverMailAddresses = string.Empty;
			foreach(var item in msg.To)
			{
				receiverMailAddresses += item.Address + ";";
			}
			Console.WriteLine("Send mail to:" + receiverMailAddresses);
#endif
		}

		// Public implementation of Dispose pattern callable by consumers.
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		// Protected implementation of Dispose pattern.
		protected virtual void Dispose(bool disposing)
		{
			if (disposed)
				return;

			if (disposing)
			{
				handle.Dispose();
#if !DEBUG
				for (int i = 0; i <= clientCount; i++)
				{
					try
					{
						smtpClients[i].Dispose();
					}
					catch (Exception ex)
					{
						//Log exception in log handler
					}
				}
#endif
			}

			disposed = true;
		}
	}
}
