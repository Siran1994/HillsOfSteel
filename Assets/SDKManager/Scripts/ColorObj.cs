using UnityEngine;


[DisallowMultipleComponent]
[AddComponentMenu("ColorObj")]
public class ColorObj : MonoBehaviour
{
    public static Texture Icon;

    [Header("������������ɫ")]
    public bool applyCustomTextColor = true;
    public Color gameObjectTextColor = Color.red;

    [Header("����ͼ��")]
    public Texture icon = Icon;

    [Header("����ɫ")]
    public bool applyBackgroundColor = true;
    public Color backgroundColor = Color.yellow;

    [Header("������ɫ")]
    public bool applyDescription = true;
    public Color descriptionTextColor = Color.green;
    public string description= "�����������(Singleton)";    
}



