using System.Collections.Generic;

namespace UTNotifications
{
	public class Button
	{
		public string title;

		public IDictionary<string, string> userData;

		public Button(string title, IDictionary<string, string> userData = null)
		{
			this.title = title;
			this.userData = userData;
		}
	}
}
