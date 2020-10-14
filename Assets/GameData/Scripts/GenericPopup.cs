using System;
using TMPro;

public class GenericPopup : MenuBase<GenericPopup>
{
	public TextMeshProUGUI popupText;

	public Action popupAfter;

	public void Init(string text, Action after)
	{
		popupText.text = text;
		popupAfter = after;
	}
}
