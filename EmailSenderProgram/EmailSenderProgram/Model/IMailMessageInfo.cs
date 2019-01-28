using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailSenderProgram.Model
{
	public interface IMailMessageInfo
	{
		string From { get; set; }

		IEnumerable<string> To { get; }

		string Subject { get; set; }

		string Body { get; set; }

		void AddMailIdToReceiverList(string mailId);

		void RemoveMailIdFromReceiverList(string mailId);

		void ClearReceiverList();

	}
}
