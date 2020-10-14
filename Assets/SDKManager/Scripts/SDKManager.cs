using System;
using System.Collections;
using System.Threading;
using UnityEngine;
#pragma warning disable 0618,0649,0414
/*****************************************
	 文件:   SDKManager.cs
	 作者:   漠白
	 日期:   2020/10/9 11:15:54
	 功能:   广告接入管理类 (New)
 *****************************************/
public enum PackageType  //出包类型
{
    PingCe,   //评测包(无广告)
    BaiBao,   //白包(单机包: OPPO,VIVO) 
    AD,       //广告包
}
public enum ShowAdType  //展示广告类型插屏
{
    Banner = 1,   //条幅广告
    InfoStream,   //信息流广告
    ChaPing,      //插屏广告
    VideoAD,      //视频广告
    Reward,       //激励视频
    Splash        //开屏广告
}
[HelpURL("https://github.com/Siran1994/MyTools")]
[DisallowMultipleComponent]
[RequireComponent(typeof(ColorObj))]
public class SDKManager : MonoBehaviour
{
    public static SDKManager Instance; //单例类

    #region 全局设置
    public static string AppName = "测试"; //项目名称
    public static string ChannelName = "瞬玩"; //渠道名称
    public static string VersionNum = "1.0"; //版本号
    public static string KeyStoreName = "欢鱼"; //签名主体

    [Header("APK类型")]
    public PackageType PT; //包类型

    [HideInInspector]
    public string Channel = ""; //渠道名称

    [Header("是否展示广告(非AD包:false/Auto)")]
    public bool IsShowAd = true; //是否展示广告

    [Header("是否检测程序退出")]
    public bool UpdateToQuit = true; //是否需要每帧检测程序退出

    #endregion

    #region Unity方法
    void Awake()
    {
        PlayerPrefs.SetInt("Coin", 0);
        this.gameObject.name = "SDKManager";
        Application.targetFrameRate = 60; //控制update帧率
        if (Instance)
        {
            DestroyImmediate(this);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        if (PT != PackageType.AD || Application.platform != RuntimePlatform.Android)  //非安卓和非广告包不展示广告
            IsShowAd = false;
        Channel = GetChanel(); //获取渠道SDK的类型
        if (Channel == null || Channel == "")
        {
            Channel = ChannelName;
        }
    }
    void Start() //广告初始化
    {
        if (IsShowAd == false)
            return;
        // StartCoroutine(SendPost(url));//数据上报       
        switch (Channel)
        {
            case "OPPO":
            case "oppo":
            case "美图":
            case "爱奇艺":
                BannerInit("1", "0000", 1, -1);
                ChaPingInit("2", "0000", 1, -1);
                VideoAdInit("3", "0000", 1, -1);
                RewardInit("4", "", 1, -1);
                Login();
                break;
            default:
                BannerInit("1", "0000", 1, -1);
                ChaPingInit("2", "0000", 1, -1);
                VideoAdInit("3", "0000", 1, -1);
                RewardInit("4", "", 1, -1);
                break;
        }
        RepeatShowBan(2, 30, "默认启动后2s开始弹,30s循环"); //重复调用Banner 2s后展示 30秒刷新
    }
    void Update()
    {
        if (UpdateToQuit == false)
            return;
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (PT == PackageType.PingCe)
            {
                Application.Quit();
            }
            if (PT == PackageType.BaiBao || PT == PackageType.AD)//特殊渠道白包和广告包退出时注销广告SDK
            {
                if (Channel == "oppo" || Channel == "vivo" || Channel == "美图" || Channel == "爱奇艺")
                {
                    QuitGame(); //退出面板
                }
                else
                {
                    if (Channel == "咪咕")
                    {
                        PlayerPrefs.SetInt("handClose", 0);
                    }
                    Application.Quit();
                }
            }
        }
    }
    #endregion

    #region 公共方法
    public void ShowAd(ShowAdType ADType, int param, string Log = "Unity日志展示", Action<bool> action = null)
    {
        switch (ADType)
        {
            case ShowAdType.Banner:
                ShowAD(ShowAdType.Banner, param);
                Debug.Log(Log + "展示Banner");
                break;
            case ShowAdType.InfoStream:
                ShowAD(ShowAdType.InfoStream, param);
                Debug.Log(Log + "展示信息流");
                break;
            case ShowAdType.ChaPing:
                ShowAD(ShowAdType.ChaPing, param);
                Debug.Log(Log + "展示插屏");
                break;
            case ShowAdType.VideoAD:
                if (Channel == "233")
                {
                    if (isCanShow || index == 1)
                    {
                        index++;
                        isCanShow = false;
                        ShowAD(ShowAdType.VideoAD, param);
                        Invoke("CanShow", 180);
                        Invoke("Timer", 1.0f);
                    }
                    else
                    {
                        Debug.Log("广告请求过于频繁,请在" + (180 - time) + "秒后再试!");
                        if (180 - time == 0)
                        {
                            CancelInvoke("Timer");
                        }
                        MakeToast("广告请求过于频繁, 请在" + (180 - time) + "秒后再试!");
                    }
                }
                else
                {
                    ShowAD(ShowAdType.VideoAD, param);
                }
                Debug.Log(Log + "展示全屏视屏");
                break;
            case ShowAdType.Reward:
                if (PT == PackageType.PingCe || PT == PackageType.BaiBao)
                {
                    MakeToast();
                }
                else
                {
                    ShowAD(ShowAdType.Reward, param);
                    thread = new Thread(new ThreadStart(delegate
                    {
                        while (true)
                        {
                            while (IsComplete)
                            {
                                action.Invoke(IsComplete);//传递状态值
                                IsComplete = false;
                            }
                            System.Threading.Thread.Sleep(500);
                        }
                    }));
                    thread.IsBackground = true;
                    thread.Start();
                }
                Debug.Log(Log + "展示激励视屏");
                break;
            case ShowAdType.Splash:
                ShowAD(ShowAdType.Splash, param);
                Debug.Log(Log + "展示开屏");
                break;
        }
    }
    #region 数据上报
    private string url = "xxxxxxx";
    private IEnumerator SendGet(string url)//Get请求
    {
        if (!string.IsNullOrEmpty(url))
        {
            WWW result = new WWW(url);

            yield return result;

            if (result.error != null)
            {
                Debug.Log("访问失败：" + result.error);
            }
            else
            {
                if (string.IsNullOrEmpty(result.text))
                {
                    Debug.LogError("返回值为空");
                }
                else
                {
                    Debug.Log(result.text);

                }
            }
        }
        else
        {
            Debug.LogError("URL不能为空");
        }
    }
    private IEnumerator SendPost(string url, WWWForm wForm = null)//Post请求
    {
        if (!string.IsNullOrEmpty(url))
        {
            WWW result = new WWW(url, wForm);

            yield return result;

            if (result.error != null)
            {
                Debug.Log("访问失败：" + result.error);
            }
            else
            {
                if (string.IsNullOrEmpty(result.text))
                {
                    Debug.LogError("返回值为空");
                }
                else
                {
                    Debug.Log(result.text);
                }
            }
        }
        else
        {
            Debug.LogError("URL不能为空");
        }
    }
    #endregion

    #region 激励广告1分钟限制和提示范列
    bool isCanShow = false;
    int index = 1;
    int time = 0;
    public void CanShow()
    {
        isCanShow = true;
    }
    void Timer()
    {
        time++;
        if (time == 180)
        {
            time = 0;
        }
        else
        {
            Invoke("Timer", 1.0f);
        }
    }
    public void ADTOUNLOCK()
    {
        if (isCanShow || index == 1)
        {
            index++;
            isCanShow = false;
            SDKManager.Instance.ShowAd(ShowAdType.Reward, 1, "点击xxxxxx", (bool IsComplete) =>
            {
                //TODO(give reward)
            });
            Invoke("CanShow", 60);
            Invoke("Timer", 1.0f);
        }
        else
        {
            Debug.Log("广告请求过于频繁,请在" + (60 - time) + "秒后再试!");
            if (60 - time == 0)
            {
                CancelInvoke("Timer");
            }
            SDKManager.Instance.MakeToast("广告请求过于频繁, 请在" + (60 - time) + "秒后再试!");
        }
    }

    #endregion
    public void MakeToast(string str = "暂无广告!!!")//吐司提示
    {
        Debug.Log(str);
        if (Application.platform != RuntimePlatform.Android)
            return;
        AndroidJavaObject currentActivity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
        AndroidJavaClass Toast = new AndroidJavaClass("android.widget.Toast");
        currentActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
        {
            Toast.CallStatic<AndroidJavaObject>("makeText", currentActivity, str, Toast.GetStatic<int>("LENGTH_LONG")).Call("show");
        }));
    }
    public void RepeatShowBan(float time, int rate, string Log = "Unity日志展示") //重复调用Banner
    {
        Debug.Log(Log + "展示Banner");
        if (IsShowAd == false)
            return;
        if (Channel == "oppo" || Channel == "OPPO")
        {
            Invoke("ShowBanner", time);
        }
        else
        {
            InvokeRepeating("ShowBanner", time, rate);
        }
    }
    public void CloseBanner(int param = 1, string Log = "Unity日志展示") //关闭banner
    {
        Debug.Log(Log + "关闭Banner");
        if (IsShowAd == false)
            return;
        CancelInvoke("ShowBanner");
        using (AndroidJavaClass ac = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            AndroidJavaObject ao = ac.GetStatic<AndroidJavaObject>("currentActivity");
            if (ao != null)
            {
                ao.Call("closeBanner", param);
            }
        }
    }
    public void SuperRelaxation(int index = 3) //超休闲
    {
        using (AndroidJavaClass ac = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            AndroidJavaObject ao = ac.GetStatic<AndroidJavaObject>("currentActivity");
            if (ao != null)
            {
                ao.Call("jump", index);
            }
        }
    }
    public void EventRecorder(string eventID = "启动游戏", string eventDesc = "A:玩家启动游戏") //打点
    {
        if (Channel == "瓦力" || Channel == "小米")
        {
            using (AndroidJavaClass ac = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                AndroidJavaObject ao = ac.GetStatic<AndroidJavaObject>("currentActivity");
                if (ao != null)
                {
                    ao.Call("tdEvent", eventID, eventDesc);
                }
            }
        }
    }
    #endregion

    #region  私有方法
    private string GetChanel() //获取SDK渠道的名称 
    {
        if (IsShowAd == false)
            return "";
        using (AndroidJavaClass ac = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            AndroidJavaObject ao = ac.GetStatic<AndroidJavaObject>("currentActivity");
            return ao.Call<string>("getChannel");
        }
    }
    private void QuitGame()  //退出面板
    {
        using (AndroidJavaClass ac = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            AndroidJavaObject ao = ac.GetStatic<AndroidJavaObject>("currentActivity");
            if (ao != null)
            {
                ao.Call("onQuitGame");
            }
        }
    }
    private void ShowBanner()//Banner展示
    {
        if (Channel == "咪咕")
        {
            if (PlayerPrefs.GetInt("handClose", 0) == 0)
            {
                ShowAD(ShowAdType.Banner, 1);
            }
        }
        else
        {
            ShowAD(ShowAdType.Banner, 1);
        }
    }

    #region OPPO,爱奇艺,美图登陆
    private void Login() //登陆
    {
        using (AndroidJavaClass ac = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            AndroidJavaObject ao = ac.GetStatic<AndroidJavaObject>("currentActivity");

            if (ao != null)
            {
                ao.Call("login");
            }
        }
    }
    #endregion
    #endregion

    #region 广告初始化
    void BannerInit(string adId, string openId, int param, int limit) //Banner初始化
    {
        using (AndroidJavaClass ac = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            AndroidJavaObject ao = ac.GetStatic<AndroidJavaObject>("currentActivity");
            ao.Call("initBanner", adId, openId, param, limit);
        }
    }
    void InfoStreamInit(string adId, string openId, int param, int limit)  //信息流初始化
    {
        using (AndroidJavaClass ac = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            AndroidJavaObject ao = ac.GetStatic<AndroidJavaObject>("currentActivity");
            ao.Call("initNativeBanner", adId, openId, param, limit);
        }
    }
    void ChaPingInit(string adId, string openId, int param, int limit) //插屏广告初始化
    {
        using (AndroidJavaClass ac = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            AndroidJavaObject ao = ac.GetStatic<AndroidJavaObject>("currentActivity");
            ao.Call("initInterstitialAd", adId, openId, param, limit);
        }
    }
    void VideoAdInit(string adId, string openId, int param, int limit) //全屏视频初始化
    {
        using (AndroidJavaClass ac = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            AndroidJavaObject ao = ac.GetStatic<AndroidJavaObject>("currentActivity");
            ao.Call("initFullVideo", adId, openId, param, limit);
        }
    }
    void RewardInit(string adId, string openId, int param, int limit)  //激励视频初始化
    {
        using (AndroidJavaClass ac = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            AndroidJavaObject ao = ac.GetStatic<AndroidJavaObject>("currentActivity");
            ao.Call("initRewardVideo", adId, openId, param, limit);
        }
    }
    #endregion

    #region 广告展示
    private void ShowAD(ShowAdType ADType, int param) //四种基本类型的广告展示
    {
        if (IsShowAd == false)
            return;
        using (AndroidJavaClass ac = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            AndroidJavaObject ao = ac.GetStatic<AndroidJavaObject>("currentActivity");
            if (ao != null)
            {
                switch (ADType)
                {
                    case ShowAdType.Splash:
                        ao.Call("showSplashAd");  //展示开屏视频
                        break;
                    case ShowAdType.InfoStream:
                        ao.Call("showNativeBanner", param);  //展示信息流
                        EventRecorder("InfoStream总展示", "D:InfoStream展示次数");
                        break;
                    case ShowAdType.Banner:
                        ao.Call("showBanner", param);//展示Banner
                        EventRecorder("Banner总展示", "A:Banner展示次数");
                        break;
                    case ShowAdType.ChaPing:
                        ao.Call("showInterstitialAd", param);//展示插屏
                        break;
                    case ShowAdType.VideoAD:
                        ao.Call("showFullVideo", param);//展示全屏视频
                        EventRecorder("全屏视频总展示", "B:全屏展示次数");
                        break;
                    case ShowAdType.Reward:
                        ao.Call("showRewardVideo", param);  //展示激励视频
                        EventRecorder("激励视频总展示", "C:激励视频展示次数");
                        break;
                }
            }
        }
    }
    #endregion

    #region 安卓回调函数
    private void onAdReady(object obj)
    {
    }
    private void onAdClose(object obj)
    {
        if (Channel == "咪咕")
        {
            PlayerPrefs.SetInt("handClose", 1);
        }
        Invoke("StopThread", 3);
    }
    private void StopThread()
    {
        thread.Abort();
    }
    private void onAdShow(object obj)
    {
    }
    private void onAdFailed(object obj)
    {
    }
    private void onAdClick(object obj)
    {
    }
    static bool IsComplete = false;
    Thread thread;
    public void onAdComplete(object obj)
    {
        string info = obj.ToString();
        adCallBackInfo data = JsonUtility.FromJson<adCallBackInfo>(info);
        switch (data.param)
        {
            case "1":
                IsComplete = true;
                break;
            case "2":
                EventRecorder("全屏正常结束次数", "D:全屏视频完整播放");
                break;
        }
    }
    public void onLogin(object obj)
    {
        Debug.Log("WD_LOG_onLogin : " + (string)obj);
        string info = obj.ToString();
        loginOrRealNameCallBackInfo data = JsonUtility.FromJson<loginOrRealNameCallBackInfo>(info);
        Debug.Log("WD :" + data.code);
        Debug.Log("WD :" + data.msg);
        if (data.code == callBcakState.CODE_LOGIN_SUCCESS.ToString())
        {
        }
        else if (data.code == callBcakState.CODE_LOGIN_FAILED.ToString())
        {
        }
        else if (data.code == callBcakState.CODE_LOGIN_CANCEL.ToString())
        {
        }
    }
    public void onSendInfo(object obj)
    {
        Debug.Log("WD_LOG_onSendInfo : " + (string)obj);
        string info = obj.ToString();
        loginOrRealNameCallBackInfo data = JsonUtility.FromJson<loginOrRealNameCallBackInfo>(info);
        Debug.Log("WD :" + data.code);
        Debug.Log("WD :" + data.msg);
        if (data.code == callBcakState.CODE_SEND_INFO_SUCCESS.ToString())
        {
        }
        else if (data.code == callBcakState.CODE_SEND_INFO_FAILED.ToString())
        {
        }
    }
    public void onVerified(object obj)
    {
        Debug.Log("WD_LOG_onVerified : " + (string)obj);
        string info = obj.ToString();
        loginOrRealNameCallBackInfo data = JsonUtility.FromJson<loginOrRealNameCallBackInfo>(info);
        Debug.Log("WD :" + data.code);
        Debug.Log("WD :" + data.msg);
        if (data.code == callBcakState.CODE_VERIFIED_SUCCESS_18_TO_MAX.ToString())
        {
        }
        else if (data.code == callBcakState.CODE_VERIFIED_SUCCESS_16_TO_18.ToString())
        {
        }
        else if (data.code == callBcakState.CODE_VERIFIED_SUCCESS_0_TO_16.ToString())
        {
        }
        else if (data.code == callBcakState.CODE_VERIFIED_FAILED_RESUME_GAME.ToString())
        {
        }
        else if (data.code == callBcakState.CODE_VERIFIED_FAILED_STOP_GAME.ToString())
        {
        }
    }
    #endregion
}
#region JSON数据类
class SendInfo
{
    public string roleId;
    public string roleName;
    public int roleLevel;
    public string realmId;
    public string realmName;
    public string chapter;
    public int combatValue;
    public int pointValue;
}
public class adCallBackInfo
{
    public string type;
    public string param;
    public string msg;
}
public class payCallBackInfo
{
    public string param;
    public string msg;
}
public class loginOrRealNameCallBackInfo
{
    public string code;
    public string msg;
}
public class callBcakState
{
    public static int CODE_INIT_SDK_ACTIVITY_SUCCESS = 101;
    public static int CODE_INIT_SDK_ACTIVITY_FAILED = 110;
    public static int CODE_LOGIN_SUCCESS = 301;
    public static int CODE_LOGIN_FAILED = 310;
    public static int CODE_LOGIN_CANCEL = 320;

    //  已实名认证，且已满18岁
    public static int CODE_VERIFIED_SUCCESS_18_TO_MAX = 401;
    //  已实名认证，已满16但未满18岁
    public static int CODE_VERIFIED_SUCCESS_16_TO_18 = 402;
    //  已实名认证，未满16
    public static int CODE_VERIFIED_SUCCESS_0_TO_16 = 403;
    //  未实名认证，允许继续玩游戏
    public static int CODE_VERIFIED_FAILED_RESUME_GAME = 410;
    //  未实名认证，退出游戏
    public static int CODE_VERIFIED_FAILED_STOP_GAME = 411;
    //  信息上报成功
    public static int CODE_SEND_INFO_SUCCESS = 501;
    //  信息上报失败
    public static int CODE_SEND_INFO_FAILED = 510;
}
#endregion




