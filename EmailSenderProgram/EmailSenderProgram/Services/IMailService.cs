using EmailSenderProgram.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailSenderProgram.Services
{
	public interface IMailService : IDisposable
	{
		Task<bool> SendBulkEmailAsync(IEnumerable<IMailMessageInfo> mailMessageInfos);

		void CancelBulkEmailAsync();
	}
}
