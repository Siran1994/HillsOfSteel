/*****************************************
	 文件:   ScriptsInfoRecorder.cs
	 作者:   #姓名
	 日期:   #CreateTime
	 功能:   #Nothing
 *****************************************/
using System;
using System.IO;
public class ScriptsInfoRecorder:UnityEditor.AssetModificationProcessor
{
    private static void OnWillCreateAsset(string path)
    {
        path = path.Replace(".meta", "");
        if (path.EndsWith(".cs"))
        {
            string str = File.ReadAllText(path);

            str = str.Replace("#姓名", "漠白").Replace
                (
                "#CreateTime",
                string.Concat
                    (
                    DateTime.Now.Year,
                    "/",
                    DateTime.Now.Month,
                    "/",
                    DateTime.Now.Day,
                    " ",
                    DateTime.Now.Hour,
                    ":",
                    DateTime.Now.Minute,
                    ":",
                    DateTime.Now.Second)).Replace
                (
                "#Nothing","Nothing"
                );

            File.WriteAllText(path,str);

        }
    }

}
