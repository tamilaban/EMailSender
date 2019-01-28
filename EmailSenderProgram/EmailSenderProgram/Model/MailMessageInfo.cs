using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailSenderProgram.Model
{
	public class MailMessageInfo : IMailMessageInfo
	{
		private List<string> toList;

		public MailMessageInfo()
		{
			toList = new List<string>();
		}

		public string Body { get; set; }

		public string From { get; set; }

		public string Subject { get; set; }

		public IEnumerable<string> To
		{
			get { return this.toList; }
		}

		public void AddMailIdToReceiverList(string mailId)
		{
			if (!this.toList.Contains(mailId))
				this.toList.Add(mailId);
		}		

		public void RemoveMailIdFromReceiverList(string mailId)
		{
			if (this.toList.Contains(mailId))
				this.toList.Remove(mailId);
		}

		public void ClearReceiverList()
		{
			this.toList.Clear();
		}
	}
}
