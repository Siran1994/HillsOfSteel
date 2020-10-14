using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class CountText : MonoBehaviour
{
	public string format = "{0}";

	private TextMeshProUGUI text;

	private int currentCount;

	private int endCount;

	private int maxCount;

	public void Init(int count, int maxCount = 0)
	{
		text = GetComponent<TextMeshProUGUI>();
		currentCount = count;
		this.maxCount = maxCount;
		text.text = string.Format(format, currentCount, maxCount);
	}

	public void Tick(int tickSize)
	{
		currentCount += tickSize;
		text.text = string.Format(format, currentCount, maxCount);
	}

    public void Tex(int tickSize)
    {
        currentCount += tickSize;
        text.text = currentCount.ToString();
    }
}
