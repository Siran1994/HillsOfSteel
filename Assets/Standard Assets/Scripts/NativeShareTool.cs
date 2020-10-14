using System;
using System.Collections;
using System.IO;
using UnityEngine;

public class NativeShareTool : MonoBehaviour  //½ØÆÁ·ÖÏí¹¤¾ß
{
	public string ScreenshotName = "screenshot.png";

	private float width => Screen.width;

	private float height => Screen.height;

	public void ShareScreenshotWithText(string text)
	{
		string text2 = Application.persistentDataPath + "/" + ScreenshotName;
		if (File.Exists(text2))
		{
			File.Delete(text2);
		}
		ScreenCapture.CaptureScreenshot(ScreenshotName);
		StartCoroutine(delayedShare(text2, text));
	}

	private IEnumerator delayedShare(string screenShotPath, string text)
	{
		while (!File.Exists(screenShotPath))
		{
			yield return new WaitForSeconds(0.05f);
		}
		NativeShare.Share(text, screenShotPath, "", "", "image/png", chooser: true, "");
	}

	public void Screenshot()
	{
		StartCoroutine(GetScreenshot());
	}

	public IEnumerator GetScreenshot()
	{
		yield return new WaitForEndOfFrame();
		Texture2D texture2D = new Texture2D((int)width, (int)height, TextureFormat.ARGB32, mipChain: false);
		texture2D.ReadPixels(new Rect(0f, 0f, width, height), 0, 0, recalculateMipMaps: false);
		texture2D.Apply();
		Save_Screenshot(texture2D);
	}

	private void Save_Screenshot(Texture2D screenshot)
	{
		string text = Application.persistentDataPath + "/" + DateTime.Now.ToString("dd-MM-yyyy_HH:mm:ss") + "_" + ScreenshotName;
		File.WriteAllBytes(text, screenshot.EncodeToPNG());
		StartCoroutine(DelayedShare_Image(text));
	}

	public void Clear_SavedScreenShots()
	{
		FileInfo[] files = new DirectoryInfo(Application.persistentDataPath).GetFiles("*.png");
		for (int i = 0; i < files.Length; i++)
		{
			File.Delete(files[i].FullName);
		}
	}

	private IEnumerator DelayedShare_Image(string screenShotPath)
	{
		while (!File.Exists(screenShotPath))
		{
			yield return new WaitForSeconds(0.05f);
		}
		NativeShare_Image(screenShotPath);
	}

	private void NativeShare_Image(string screenShotPath)
	{
		string text = "";
		string url = "";
		string chooserText = "Select sharing app";
		text = "Test subject.";
		NativeShare.Share("Test text", screenShotPath, url, text, "image/png", chooser: true, chooserText);
	}
}
