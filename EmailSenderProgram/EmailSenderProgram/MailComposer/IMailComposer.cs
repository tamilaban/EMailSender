using EmailSenderProgram.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace EmailSenderProgram.MailComposer
{
	public interface IMailComposer
	{
		Task<IEnumerable<IMailMessageInfo>> ComposeAsync();

		IEnumerable<IMailMessageInfo> Compose();
	}
}
