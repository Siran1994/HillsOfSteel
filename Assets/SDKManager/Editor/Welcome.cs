using UnityEngine;
using UnityEditor;
/*****************************************
	 文件:   Welcome.cs
	 作者:   漠白
	 日期:   2020/9/10 17:38:42
	 功能:   欢迎界面
 *****************************************/
[InitializeOnLoad]
public class Startup
{
    static Startup()
    {
        if (EditorPrefs.GetInt("CanLoad", 0) == 0)
        {
            WelcomeScreen.ShowWindow();
        }
    }
}
public class WelcomeScreen : EditorWindow
{
    private Texture mSamplesImage;
    private Rect imageRect = new Rect(100f, 100f, 200f, 200f);
    private Rect textRect = new Rect(120f, 40f, 300f, 100f);
    void Awake()
    {
        mSamplesImage = (Texture)AssetDatabase.LoadAssetAtPath("Assets/SDKManager/Texs/WX.png", typeof(Texture));
    }
    public void OnGUI()
    {
        GUIStyle style = new GUIStyle();
        style.fontSize = 14;
        style.normal.textColor = Color.white;
        GUI.Label(textRect, "欢迎扫一扫，关注微信号\n", style);
        GUI.DrawTexture(imageRect, mSamplesImage);
    }
    public static void ShowWindow()
    {
        WelcomeScreen window = GetWindow<WelcomeScreen>(true, "Hello!", false);
        window.minSize = window.maxSize = new Vector2(400, 400);
        window.titleContent = new GUIContent("欢迎使用本工具", null, "欢迎使用本工具!,开发者:漠白,QQ:342093031,支付宝:boskbu@gmail.com");
        DontDestroyOnLoad(window);
    }
    private void OnDisable()
    {
        EditorPrefs.SetInt("CanLoad", 1);
    }
}