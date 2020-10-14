#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Windows;
using UnityEditor.Build.Reporting;
using NPinyin;
#endif
#pragma warning disable 0618
#pragma warning disable 0414
/*****************************************
	 文件:   ApkBuildTool.cs
	 作者:   漠白
	 日期:   2020/10/9 11:12:57
	 功能:  Apk自动打包工具
 *****************************************/
public class ApkBuildTool
{
#if UNITY_EDITOR
    #region 核心变量
    private static bool Pass = true;//可以打包
    private static string AppName = GetAppName();//项目名
    private static string AppNameAbr = Pinyin.GetInitials(AppName).ToLower();//项目名简称
    private static string Channel = GetChannel();//渠道名称
    private static string Version = GetVersion();//版本号
    private static string KeyName = GetSignKeyType();//签名主体   
    #endregion
    [MenuItem("GameObject/SDKManager", false, 0)]
    static void Init()
    {
        ColorObj.Icon = (Texture)AssetDatabase.LoadAssetAtPath("Assets/SDKManager/Texs/Logo.jpg", typeof(Texture));
        var go = new GameObject("SDKManager");
        go.AddComponent<ColorObj>();
        go.AddComponent<SDKManager>();
        EditorApplication.SaveScene();
    }
    [MenuItem("一键打包/项目检查", false, 1)]
    static void Check()
    {
        Pass = true;
        Debug.Log("<color=#ff00ffff>当前项目:</color> " + "<color=#00ffffff><size=20>" + AppName + "</size></color>");
        Debug.Log("<color=yellow>当前渠道:</color> " + "<color=#00ffffff><size=20>" + Channel + "</size></color>");
        Debug.Log("<color=red>当前版本号:</color> " + "<color=#00ffffff><size=20>" + Version + "</size></color>");
        Debug.Log("<color=#00ff00ff>当前签名主体:</color> " + "<color=#00ffffff><size=20>" + KeyName + "</size></color>");
        UpdateAndroidSettings();
    }
    [MenuItem("一键打包/评测包", false, 102)]
    static void BuildPingCe()
    {
        BulidTarget(AppName, AppNameAbr, Channel, Version, 1, true, KeyName);
    }
    [MenuItem("一键打包/白包", false, 202)]
    static void BuildBaiBao()
    {
        BulidTarget(AppName, AppNameAbr, Channel, Version, 2, true, KeyName);
    }
    [MenuItem("一键打包/广告包", false, 302)]
    static void BuildAD()
    {
        BulidTarget(AppName, AppNameAbr, Channel, Version, 3, true, KeyName);
    }
    static void BulidTarget(string Appname, string AppNameAbr, string Platform = "", string Version = "1.0", int ApkType = 3, bool Is32 = true, string keystoreName = "欢鱼")
    {
        //------------------------------------------------------------项目设置-------------------------------------------------------
#if (UNITY_2019 || UNITY_2020) //设置签名
        PlayerSettings.Android.useCustomKeystore = true;
#endif
        var DesktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + "/";
        switch (keystoreName)
        {
            case "欢鱼":
                PlayerSettings.companyName = "hy";//公司名
                PlayerSettings.Android.keystoreName = DesktopPath + "欢鱼签名/huanyu.jks";
                PlayerSettings.Android.keyaliasPass = "123456";
                PlayerSettings.Android.keyaliasName = "huanyu";
                PlayerSettings.keystorePass = "123456";
                break;
            case "朋来":
                PlayerSettings.companyName = "pl";//公司名
                PlayerSettings.Android.keystoreName = DesktopPath + "朋来签名/penglai.jks";
                PlayerSettings.Android.keyaliasPass = "123456";
                PlayerSettings.Android.keyaliasName = "penglai";
                PlayerSettings.keystorePass = "123456";
                break;
            case "海南欢乐元素":
                PlayerSettings.companyName = "hlys";//公司名
                PlayerSettings.Android.keystoreName = DesktopPath + "海南欢乐元素签名/huanle.jks";
                PlayerSettings.Android.keyaliasPass = "123456";
                PlayerSettings.Android.keyaliasName = "huanle";
                PlayerSettings.keystorePass = "123456";
                break;
        }

        PlayerSettings.productName = Appname;//项目名
        SetIcons(@"Assets\Icon.png");//设置Icon
        SetScreenLogo("Assets/SDKManager/Texs/hy1.png");//设置开机图

        string Pt = "";
        switch (ApkType)
        {
            case 1:
                Pt = "评测包";
                break;
            case 2:
                Pt = "白包";
                break;
            case 3:
                Pt = "广告包";
                break;
        }

        PlayerSettings.Android.renderOutsideSafeArea = true;//适配刘海和水滴屏
        string app_name = Appname + Platform + Pt + Version + ".apk";//全名
        string outputPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + "/" + app_name;//输出位置
        PlayerSettings.bundleVersion = Version;//外部版本号
        PlayerSettings.Android.bundleVersionCode = (int)float.Parse(Version);//内部版本号
        if (Is32)
        {
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.Mono2x);//32位包
        }
        else
        {
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);//64位包
        }

        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel19;
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel28;
        PlayerSettings.scriptingRuntimeVersion = ScriptingRuntimeVersion.Latest;

        PlayerSettings.muteOtherAudioSources = true;

        var packageName = "com." + PlayerSettings.companyName + ".";
        switch (Platform)//设置包名
        {
            case "":
                PlayerSettings.applicationIdentifier = packageName + AppNameAbr + ".nearme";
                break;
            case "BM":
                PlayerSettings.applicationIdentifier = packageName + AppNameAbr + ".nearme.bm";
                break;
            case "233":
                PlayerSettings.applicationIdentifier = packageName + AppNameAbr + ".nearme.meta";
                break;
            case "糖果":
                PlayerSettings.applicationIdentifier = packageName + AppNameAbr + ".nearme.sugar";
                break;
            case "2345":
                PlayerSettings.applicationIdentifier = packageName + AppNameAbr + ".nearme.essw";
                break;
            case "oppo":
            case "OPPO":
                PlayerSettings.applicationIdentifier = packageName + AppNameAbr + ".nearme.gamecenter";
                break;
            case "vivo":
            case "VIVO":
                PlayerSettings.applicationIdentifier = packageName + AppNameAbr + ".nearme.vivo";
                break;
            case "小米":
            case "瓦力":
                PlayerSettings.applicationIdentifier = packageName + AppNameAbr + ".nearme.mi";
                break;
            case "联想":
                PlayerSettings.applicationIdentifier = packageName + AppNameAbr + ".nearme.lenovo";
                break;
            case "360":
                PlayerSettings.applicationIdentifier = packageName + AppNameAbr + ".nearme.qihoo";
                break;
            case "美图":
                PlayerSettings.applicationIdentifier = packageName + AppNameAbr + ".nearme.meitu";
                break;
            case "魅族":
                PlayerSettings.applicationIdentifier = packageName + AppNameAbr + ".nearme.mz";
                break;
            case "海启":
                PlayerSettings.applicationIdentifier = packageName + AppNameAbr + ".nearme.hq";
                break;
            case "爱奇艺":
                PlayerSettings.applicationIdentifier = packageName + AppNameAbr + ".nearme.iqiyi";
                break;
            case "海信":
                PlayerSettings.applicationIdentifier = packageName + AppNameAbr + ".nearme.hisense";
                break;
            case "芒果":
                PlayerSettings.applicationIdentifier = packageName + AppNameAbr + ".nearme.mgtv";
                break;
            default:
                PlayerSettings.applicationIdentifier = packageName + AppNameAbr + ".nearme";//默认母包名
                break;
        }
        //---------------------------------------------------------开始打包----------------------------------------------------
        if (Pass)
        {
            BuildReport report = BuildPipeline.BuildPlayer(GetBuildScenes(), outputPath, BuildTarget.Android, BuildOptions.None);

            BuildSummary summary = report.summary;//打包结果反馈

            if (summary.result == BuildResult.Succeeded)
            {
                var IsSuccess = EditorUtility.DisplayDialog("打包成功!", "项目大小为:" + summary.totalSize / (4 * 1024 * 1024) + "M" + "\n" + "耗时:" + summary.totalTime.TotalSeconds + "s", "确定");
                if (IsSuccess)
                {
                    Application.OpenURL(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory));
                }
            }
            if (summary.result == BuildResult.Failed)
            {
                EditorUtility.DisplayDialog("打包失败!", "问题数:" + summary.totalErrors + "个", "确定");
            }
        }
    }
    static string[] GetBuildScenes()//获取需要打包的场景
    {
        List<string> pathList = new List<string>();
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            if (scene.enabled)
            {
                pathList.Add(scene.path);
            }
        }
        return pathList.ToArray();
    }
    static void SetIcons(string Icon_Path)//设置Icon
    {
        if (!File.Exists(Icon_Path))
        {
            Icon_Path = @"Assets\Icon.jpg";
            if (!File.Exists(Icon_Path))
            {
                EditorUtility.DisplayDialog("错误!!!", "没有找到该项目Icon", "确定");
                Pass = false;
            }
        }
        string iconPrefixName = "Icon";
        //获取所有的Icon尺寸
        int[] iconSizes = PlayerSettings.GetIconSizesForTargetGroup(BuildTargetGroup.Android);
        Texture2D[] texArray = new Texture2D[iconSizes.Length];
        for (int i = 0; i < iconSizes.Length; ++i)
        {
            int iconSize = iconSizes[i];
            //获得对应目录下的Icon，并转换成Texture2D
            Texture2D tex2D = AssetDatabase.LoadAssetAtPath(string.Format(Icon_Path, iconPrefixName, iconSize),
                typeof(Texture2D)) as Texture2D;
            texArray[i] = tex2D;
        }
        //设置到PlayerSettings的各个Icon上
        PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Android, texArray);
        AssetDatabase.SaveAssets();
    }
    static void SetScreenLogo(string Logo_Path)//设置Logo
    {
        if (!File.Exists(Logo_Path))
        {
            EditorUtility.DisplayDialog("错误!!!", "没有找到健康忠告素材", "确定");
            Pass = false;
        }
        PlayerSettings.SplashScreen.showUnityLogo = true;// 屏蔽下方显示unity的logo (包含文字made with unity 和unity 的logo)
        PlayerSettings.SplashScreen.unityLogoStyle = PlayerSettings.SplashScreen.UnityLogoStyle.LightOnDark;
        PlayerSettings.SplashScreen.animationMode = PlayerSettings.SplashScreen.AnimationMode.Static;
        PlayerSettings.SplashScreen.drawMode = PlayerSettings.SplashScreen.DrawMode.AllSequential;
        PlayerSettings.SplashScreen.backgroundColor = Color.black;
        var logo = AssetDatabase.LoadAssetAtPath(Logo_Path, typeof(Sprite)) as Sprite;
        var Unitylogo = PlayerSettings.SplashScreenLogo.Create(2, PlayerSettings.SplashScreenLogo.unityLogo);
        var Mylogo = PlayerSettings.SplashScreenLogo.Create(2, logo);
        PlayerSettings.SplashScreen.logos = new PlayerSettings.SplashScreenLogo[2] { Unitylogo, Mylogo };
    }

    public static void UpdateAndroidSettings()//设置AndroidSDK,NDK,JDK路径
    {
#if (UNITY_2018 || UNITY_2017||UNITY5)
        UpdateSetting("AndroidSdkRoot", "F:/AndroidPlayer/SDK");
        UpdateSetting("AndroidNdkRoot", "F:/AndroidPlayer/NDK");
        UpdateSetting("JdkPath", "F:/AndroidPlayer/JDK");
#endif
    }
    private static void UpdateSetting(string key, string Path)
    {
        if (Directory.Exists(Path))
        {
            EditorPrefs.SetString(key, Path);
        }
        else
        {
            EditorUtility.DisplayDialog("提示!", "AndroidSDK,NDK,JDK路径错误!", "确定");
        }
    }
    static string GetAppName()//获取项目名称
    {
        if (SDKManager.AppName == "")
        {
            EditorUtility.DisplayDialog("提示!", "未设置项目名称!", "确定");
            return null;
        }
        else
        {
            return SDKManager.AppName;
        }
    }
    static string GetChannel()//获取渠道名称
    {
        if (SDKManager.ChannelName == "")
        {
            EditorUtility.DisplayDialog("提示!", "未设置渠道名称,默认渠道:'无'", "确定");
            return "";
        }
        else
        {
            return SDKManager.ChannelName;
        }
    }
    static string GetVersion()//获取版本号
    {
        if (SDKManager.VersionNum == "")
        {
            EditorUtility.DisplayDialog("提示!", "未设置版本号,默认版本号为:1.0", "确定");
            return "1.0";
        }
        else
        {
            return SDKManager.VersionNum;
        }
    }
    static string GetSignKeyType()//获取签名主体
    {
        string keystoreName = "";
        switch (SDKManager.KeyStoreName)
        {
            case "欢鱼":
                keystoreName = "欢鱼";
                break;
            case "朋来":
                keystoreName = "朋来";
                break;
            case "海南欢乐元素":
                keystoreName = "海南欢乐元素";
                break;
        }
        return keystoreName;
    }
#endif
}
