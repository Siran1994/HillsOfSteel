using System.Collections.Generic;

namespace UTNotifications
{
	public sealed class JsonUtils
	{
		public static JSONArray ToJson(ICollection<Button> buttons)
		{
			if (buttons == null || buttons.Count == 0)
			{
				return null;
			}
			JSONArray jSONArray = new JSONArray();
			foreach (Button button in buttons)
			{
				JSONClass jSONClass = new JSONClass();
				jSONClass.Add("title", button.title);
				JSONNode jSONNode = ToJson(button.userData);
				if (jSONNode != null)
				{
					jSONClass.Add("userData", jSONNode);
				}
				jSONArray.Add(jSONClass);
			}
			return jSONArray;
		}

		public static JSONNode ToJson(IDictionary<string, string> userData)
		{
			if (userData == null || userData.Count == 0)
			{
				return null;
			}
			JSONClass jSONClass = new JSONClass();
			foreach (KeyValuePair<string, string> userDatum in userData)
			{
				jSONClass.Add(userDatum.Key, new JSONData(userDatum.Value));
			}
			return jSONClass;
		}
	}
}
