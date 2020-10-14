using UnityEngine;

public class ArrayElementTitleAttribute : PropertyAttribute
{
	public string name;

	public ArrayElementTitleAttribute(string elementName)
	{
		name = elementName;
	}
}
