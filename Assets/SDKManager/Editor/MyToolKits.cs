using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
/*****************************************
文件:   MyToolKits.cs
作者:   漠白
日期:   2020/10/12 15:12:57
功能:   编辑器拓展工具合集
*****************************************/
public class MyToolKits
{
    [InitializeOnLoadMethod]
    static void InitializeOnLoadMethod()//Unity防关闭
    {
        EditorApplication.wantsToQuit -= Quit;
        EditorApplication.wantsToQuit += Quit;
    }
    static bool Quit()
    {
        var res = EditorUtility.DisplayDialog("正在关闭Unity...", "你确定要关闭Unity吗?", "确定", "取消");
        if (res)
            EditorPrefs.DeleteKey("CanLoad");//EditorPrefs中保存Unity的重要数据,比如SDK,NDK,JDK的路径
        return res; //return true表示可以关闭unity编辑器
    }
    [MenuItem("项目助手/一键清理数据", false, 1)]
    static void ClearPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
        EditorPrefs.DeleteKey("CanLoad");
        Debug.Log("数据清理成功!");
    }
    [MenuItem("项目助手/删除无效脚本", false, 101)]
    static void CleanupMissingScript()
    {
        GameObject[] pAllObjects = (GameObject[])Resources.FindObjectsOfTypeAll(typeof(GameObject));

        int r;
        int j;
        for (int i = 0; i < pAllObjects.Length; i++)
        {
            if (pAllObjects[i].hideFlags == HideFlags.None)//HideFlags.None 获取Hierarchy面板所有Object
            {
                var components = pAllObjects[i].GetComponents<Component>();
                var serializedObject = new SerializedObject(pAllObjects[i]);
                var prop = serializedObject.FindProperty("m_Component");
                r = 0;

                for (j = 0; j < components.Length; j++)
                {
                    if (components[j] == null)
                    {
                        prop.DeleteArrayElementAtIndex(j - r);
                        r++;
                    }
                }

                serializedObject.ApplyModifiedProperties();
            }
        }
        Debug.Log("无效脚本清理成功!");
    }
    [MenuItem("项目助手/创建默认文件夹", false, 201)]
    static void CreateFolders()
    {
        string[] folderNames = { "Animations", "Audio", "Fonts", "Plugins", "Textures", "Materials", "Resources", "Scenes", "Scripts", "Shaders", "Prefabs" };
        string path = Application.dataPath + "/";
        for (int i = 0; i < folderNames.Length; i++)
        {
            if (!Directory.Exists(path + folderNames[i]))
            {
                Directory.CreateDirectory(path + folderNames[i]);
            }
        }
        AssetDatabase.Refresh();
    }
    [MenuItem("项目助手/查找空引用/当前场景", false, 301)]
    public static void FindMissingReferencesInCurrentScene()
    {
        var sceneObjects = GetSceneObjects();
        FindMissingReferences(EditorSceneManager.GetActiveScene().name, sceneObjects);
    }
    [MenuItem("项目助手/查找空引用/所有场景", false, 301)]
    public static void MissingSpritesInAllScenes()
    {
        foreach (var scene in EditorBuildSettings.scenes.Where(s => s.enabled))
        {
            EditorSceneManager.OpenScene(scene.path);
            FindMissingReferencesInCurrentScene();
        }
    }
    [MenuItem("项目助手/查找空引用/Assets", false, 301)]
    public static void MissingSpritesInAssets()
    {
        var allAssets = AssetDatabase.GetAllAssetPaths().Where(path => path.StartsWith("Assets/")).ToArray();
        var objs = allAssets.Select(a => AssetDatabase.LoadAssetAtPath(a, typeof(GameObject)) as GameObject).Where(a => a != null).ToArray();

        FindMissingReferences("Project", objs);
    }
    #region 空引用查找
    private static void FindMissingReferences(string context, GameObject[] objects)
    {
        foreach (var go in objects)
        {
            var components = go.GetComponents<Component>();

            foreach (var c in components)
            {
                if (!c)
                {
                    Debug.LogError("场景: " + EditorSceneManager.GetActiveScene().name + " " + "物体: " + GetFullPath(go) + " 有组件缺失", go);
                    continue;
                }

                SerializedObject so = new SerializedObject(c);
                var sp = so.GetIterator();

                while (sp.NextVisible(true))
                {
                    if (sp.propertyType == SerializedPropertyType.ObjectReference)
                    {
                        if (sp.objectReferenceValue == null
                            && sp.objectReferenceInstanceIDValue != 0)
                        {
                            ShowError(context, go, c.GetType().Name, ObjectNames.NicifyVariableName(sp.name));
                        }
                    }
                }
            }
        }
    }
    private static GameObject[] GetSceneObjects()
    {
        return Resources.FindObjectsOfTypeAll<GameObject>()
            .Where(go => string.IsNullOrEmpty(AssetDatabase.GetAssetPath(go))
                   && go.hideFlags == HideFlags.None).ToArray();
    }
    private static void ShowError(string context, GameObject go, string componentName, string propertyName)
    {
        var ERROR_TEMPLATE = "Missing Ref in: [{3}]{0}. Component: {1}, Property: {2}";

        Debug.LogError(string.Format(ERROR_TEMPLATE, GetFullPath(go), componentName, propertyName, context), go);
    }
    private static string GetFullPath(GameObject go)
    {
        return go.transform.parent == null
            ? go.name
                : GetFullPath(go.transform.parent.gameObject) + "/" + go.name;
    }
    #endregion
    [MenuItem("项目助手/文件创建/xLua File", false, 401)]
    static void CreateXLuaFile()
    {
        CreateFile("xLua", "newxLua");
    }
    [MenuItem("项目助手/文件创建/Lua File", false, 401)]
    static void CreateLuaFile()
    {
        CreateFile("lua", "newlua");
    }
    [MenuItem("项目助手/文件创建/Text File", false, 401)]
    static void CreateTextFile()
    {
        CreateFile("txt", "newtxt");
    }
    [MenuItem("项目助手/文件创建/Ini Config File", false, 401)]
    static void CreateIniFile()
    {
        CreateFile("ini", "newconfig");
    }
    [MenuItem("项目助手/文件创建/Xml File", false, 401)]
    static void CreateXmlFile()
    {
        CreateFile("xml", "newxml", "<xml></xml>");
    }
    #region 特殊文件创建
    static void CreateFile(string fileEx, string fileName = "newfile", string fileContain = "-- test")
    {
        var path = Application.dataPath + "/";
        var newFileName = fileName + "." + fileEx;
        var fullPath = path + newFileName;

        //如果是空白文件，编码并没有设成UTF-8
        File.WriteAllText(fullPath, fileContain, Encoding.UTF8);

        AssetDatabase.Refresh();
        //选中新创建的文件
        var asset = AssetDatabase.LoadAssetAtPath(fullPath, typeof(UnityEngine.Object));
        Selection.activeObject = asset;
    }
    #endregion
    #region 打AB包
    [MenuItem("项目助手/打AB包/无损打包", false, 701)] //编辑器目录
    static void BuildAllAssetBundles()
    {
        string dir = "Assets/StreamingAssets";

        if (Directory.Exists(dir) == false) //创建存放资源的AssetBundles文件夹
        {
            Directory.CreateDirectory(dir);
        }
        //输出文件夹(需要自行创建文件夹)         //打包选项                 //包资源使用平台(只能使用)
        BuildPipeline.BuildAssetBundles(Application.streamingAssetsPath,
            BuildAssetBundleOptions.None,
            BuildTarget.StandaloneWindows64);
        AssetDatabase.Refresh();
    }
    [MenuItem("项目助手/打AB包/启用TypeTree", false, 701)] //编辑器目录
    static void BuildBundlesWithTypeTree()  //兼容性之树(主要用于跨版本之间做兼容的)
    {
        string dir = "Assets/StreamingAssets";

        if (Directory.Exists(dir) == false) //创建存放资源的AssetBundles文件夹
        {
            Directory.CreateDirectory(dir);
        }
        //输出文件夹(需要自行创建文件夹)                //压缩格式                                                           //包资源使用平台(只能使用)
        BuildPipeline.BuildAssetBundles(Application.streamingAssetsPath,
            BuildAssetBundleOptions.ChunkBasedCompression,
            BuildTarget.StandaloneWindows64);
        AssetDatabase.Refresh();
    }
    [MenuItem("项目助手/打AB包/禁用TypeTree", false, 701)] //编辑器目录
    static void BuildBundlesWithOutTypeTree()
    {
        string dir = "Assets/StreamingAssets";

        if (Directory.Exists(dir) == false) //创建存放资源的AssetBundles文件夹
        {
            Directory.CreateDirectory(dir);
        }
        //输出文件夹(需要自行创建文件夹)                //压缩格式                                                        //包资源使用平台(只能使用)
        BuildPipeline.BuildAssetBundles(Application.streamingAssetsPath,
            BuildAssetBundleOptions.ChunkBasedCompression | BuildAssetBundleOptions.DisableWriteTypeTree, //节约内存，减小加载时间
            BuildTarget.StandaloneWindows64);//目标平台
    }
    [MenuItem("项目助手/打AB包/禁用文件名拓展", false, 701)] //编辑器目录
    static void BuildBundlesWithOutExtraName()
    {
        string dir = "Assets/StreamingAssets";

        if (Directory.Exists(dir) == false) //创建存放资源的AssetBundles文件夹
        {
            Directory.CreateDirectory(dir);
        }
        //输出文件夹(需要自行创建文件夹)                //压缩格式                                                        //包资源使用平台(只能使用)
        BuildPipeline.BuildAssetBundles(Application.streamingAssetsPath,
            BuildAssetBundleOptions.ChunkBasedCompression | BuildAssetBundleOptions.DisableLoadAssetByFileName | BuildAssetBundleOptions.DisableLoadAssetByFileNameWithExtension | BuildAssetBundleOptions.DisableWriteTypeTree, //节约内存，减小加载时间
            BuildTarget.StandaloneWindows64);//目标平台
        AssetDatabase.Refresh();
    }
    #endregion
    #region 打AB包
    [MenuItem("项目助手/屏幕截图", false, 801)]
    static void ScreenShot()
    {
        string resolution = "" + Screen.width + "X" + Screen.height;
        string Path = Application.dataPath + "/";
        ScreenCapture.CaptureScreenshot(Path + resolution + "-" + PlayerPrefs.GetInt("number", 0) + ".png", 1);
        PlayerPrefs.SetInt("number", PlayerPrefs.GetInt("number", 0) + 1);
        AssetDatabase.Refresh();
    }
    #endregion
    #region 一键生成预制体
    [MenuItem("GameObject/生成预制体", false, 0)]
    public static void Generate()
    {
        GameObject selectedGameObject = Selection.activeGameObject;
        string selectedAssetPath = "Assets/Resources/" + selectedGameObject.name + ".prefab";
        string dir = "Assets/Resources/";

        if (!Directory.Exists(dir)) //创建存放资源的AssetBundles文件夹
        {
            Directory.CreateDirectory(dir);
        }
        PrefabUtility.SaveAsPrefabAsset(selectedGameObject, selectedAssetPath);
    }
}
#endregion
#region 游戏对象高亮展示
[InitializeOnLoad]
public class ColorObjEditor
{
    static ColorObjEditor()
    {
        EditorApplication.hierarchyWindowItemOnGUI += EvaluateIcons;

    }
    private static void EvaluateIcons(int instanceId, Rect selectionRect)
    {
        GameObject go = EditorUtility.InstanceIDToObject(instanceId) as GameObject;
        if (go != null)
        {
            ColorObj decorator = go.GetComponent<ColorObj>();

            if (decorator != null)
            {
                DrawDecoration(go, decorator, selectionRect);
            }
        }
    }
    private static void DrawDecoration(GameObject obj, ColorObj decorator, Rect rect)
    {
        if (decorator.applyBackgroundColor)
        {
            Texture2D t = new Texture2D(1, 1);
            Color c = decorator.backgroundColor;
            t.SetPixel(1, 1, c);
            t.Apply();
            GUI.DrawTexture(rect, t, ScaleMode.StretchToFill);
        }
        if (decorator.applyCustomTextColor)
        {
            GUI.contentColor = decorator.gameObjectTextColor;

            Rect nameOfObject = new Rect(rect.x + 16, rect.y + 1, rect.width, rect.height);
            GUI.Label(nameOfObject, obj.name);

            GUI.contentColor = Color.white;
        }
        if (decorator.applyDescription)
        {
            GUI.contentColor = decorator.descriptionTextColor;

            Rect labelRect = new Rect(rect.x + 150, rect.y, rect.width - 150, rect.height);
            GUI.Label(labelRect, decorator.description);

            GUI.contentColor = Color.white;
        }
        if (decorator.icon != null)
        {
            Rect r = new Rect(rect.x + rect.width - 16, rect.y, 16, 16);
            GUI.DrawTexture(r, decorator.icon);
        }
        EditorApplication.RepaintHierarchyWindow();
    }
}
#endregion

#region 获取游戏对象路径
public class GetHierarchyPath : Editor
{
    static Transform selectedItem;
    static List<string> pathElements = new List<string>();
    static Transform nextParent;
    static TextEditor path;
    static string tmpPath;

    [MenuItem("GameObject/复制当前对象路径", false, 0)]
    public static void NewMenuOptions()
    {
        selectedItem = Selection.activeTransform;
        if (selectedItem != null)
        {
            path = new TextEditor();
            tmpPath = "";
            pathElements.Clear();
            nextParent = selectedItem;
            while (true)
            {
                pathElements.Add(nextParent.name);
                if (nextParent.parent != null)
                    nextParent = nextParent.parent;
                else
                    break;
            }
            for (int i = pathElements.Count - 1; i >= 0; i--)
            {
                tmpPath += pathElements[i];
                tmpPath += "/";
            }
            tmpPath = tmpPath.Remove(tmpPath.Length - 1);
            path.text = tmpPath;
            path.SelectAll();
            path.Copy();
            Debug.Log("<color=#00ff00ff>当前游戏对象路径为:</color> " + "<color=#00ffffff><size=15>" + path.text + "</size></color>");
        }
        else
        {
            EditorUtility.DisplayDialog("错误!", "当前操作,需要选中一个游戏对象!", "确定");
        }
    }
}
#endregion

#region 子对象重命名
public class RenameChildren : EditorWindow
{
    public string baseName = "";
    static Transform[] parents;
    bool addIndexAtEnd;
    static RenameChildren window;
    string newName;
    public static void ShowWindow()
    {
        window = (RenameChildren)EditorWindow.GetWindow(typeof(RenameChildren), false, "子对象重命名", true);
        window.minSize = new Vector2(400, 200);
        window.titleContent = new GUIContent("子对象重命名");
        window.Show();
    }
    [MenuItem("GameObject/子对象重命名", false, 0)]
    public static void NewMenuOptions()
    {
        parents = Selection.transforms;
        if (parents != null && parents.Length > 0)
        {
            ShowWindow();
        }
        else
        {
            EditorUtility.DisplayDialog("错误!", "当前操作,需要选中一个游戏对象!", "确定");
        }
    }
    void OnGUI()
    {
        EditorGUILayout.Space();
        addIndexAtEnd = EditorGUILayout.ToggleLeft(new GUIContent("添加索引", "子索引将被附加到名称"), addIndexAtEnd);

        EditorGUILayout.Space();
        baseName = EditorGUILayout.TextField(" 名字: ", baseName);

        if (GUILayout.Button("重命名") || (Event.current != null && Event.current.keyCode == KeyCode.Return))
        {
            if (parents != null && parents.Length > 0)
            {
                RenameAllChildren(parents);
            }
            else
            {
                Debug.Log("Double check failed");
            }
        }
    }
    void RenameAllChildren(Transform[] prnt)
    {
        for (int i = 0; i < prnt.Length; i++)
        {
            foreach (Transform item in prnt[i])
            {
                if (baseName == "")
                    baseName = "GameObject";

                if (addIndexAtEnd)
                {
                    newName = baseName + "_" + item.transform.GetSiblingIndex();
                }
                else
                {
                    newName = baseName;
                }
                item.name = newName;
                EditorUtility.SetDirty(item.gameObject);
            }
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }

        window.Close();
    }
    void OnLostFocus()
    {
        window.Close();
    }
    void OnDestroy()
    {
        parents = null;
    }
}
#endregion
