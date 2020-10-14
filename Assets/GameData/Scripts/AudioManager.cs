using System.Collections;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
	private static AudioManager _instance;

	public static AudioManager instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = UnityEngine.Object.FindObjectOfType<AudioManager>();
			}
			return _instance;
		}
		private set
		{
			_instance = value;
		}
	}

	public static IEnumerator FadeMusicTo(float to, float length)
	{
		float time = 0f;
		float from = 0f;
		AudioMap.instance.uiMixerGroup.audioMixer.GetFloat("MusicVolume", out from);
		while (time < length)
		{
			AudioMap.instance.uiMixerGroup.audioMixer.SetFloat("MusicVolume", Mathf.Lerp(from, to, time / length));
			time += Time.unscaledDeltaTime;
			yield return new WaitForEndOfFrame();
		}
		AudioMap.instance.uiMixerGroup.audioMixer.SetFloat("MusicVolume", to);
	}

	public static void SetMusicTo(float to)
	{
		AudioMap.instance.uiMixerGroup.audioMixer.SetFloat("MusicVolume", to);
	}

	public static void SetGameAudioTo(float to)
	{
		AudioMap.instance.uiMixerGroup.audioMixer.SetFloat("EngineVolume", to);
		AudioMap.instance.uiMixerGroup.audioMixer.SetFloat("EffectsVolume", to);
	}

	public static void ToggleSound()
	{
		if (IsSoundOn())
		{
			SetSoundOff();
		}
		else
		{
			SetSoundOn();
		}
	}

	public static void SetSoundOn()
	{
		AudioListener.volume = 1f;
		PlayerPrefs.SetInt("audioOn", 1);
	}

	public static void SetSoundOff()
	{
		AudioListener.volume = 0f;
		PlayerPrefs.SetInt("audioOn", 0);
	}

	public static bool IsSoundOn()
	{
		return PlayerPrefs.GetInt("audioOn", 1) == 1;
	}

	public static IEnumerator AllUpgradedSound()
	{
		yield return FadeMusicForSound("tankLevelUp");
	}

	public static IEnumerator FadeMusicForSound(string clipName)
	{
		yield return FadeMusicTo(-20f, 0.5f);
		SetGameAudioTo(-20f);
		AudioSource audioSource = AudioMap.PlayClipAt(clipName, Vector3.zero, AudioMap.instance.uiMixerGroup);
		yield return new WaitForSeconds(audioSource.clip.length);
		SetGameAudioTo(0f);
		yield return FadeMusicTo(0f, 0.5f);
	}
}
