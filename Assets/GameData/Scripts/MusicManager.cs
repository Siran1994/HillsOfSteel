using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicManager : Manager<MusicManager>
{
	public class MusicSource
	{
		public AudioSource source;

		public float volume;

		public Coroutine fadeRoutine;
	}

	public static MusicSource menuSource;

	public static MusicSource gameSource;

	public AudioClip gameMusic;

	public AudioClip bossMusic;

	public AudioClip bossEndMusic;

	private MusicSource targetSource;

	private float targetFromVolume;

	private float targetToVolume;

	public static void Play(MusicSource source)
	{
		source.source.Play();
	}

	public static void CrossFadeToGame(float time = 3f, bool stop = true)
	{
		gameSource.source.clip = Manager<MusicManager>.instance.gameMusic;
		FadeOut(menuSource, time, stop);
		FadeIn(gameSource, time);
	}

	public static void CrossFadeToMenu(float time = 3f, bool stop = true)
	{
		FadeOut(gameSource, time, stop);
		FadeIn(menuSource, time);
	}

	public static void FadeOut(MusicSource source, float time, bool stop)
	{
		if (source.fadeRoutine != null)
		{
			Manager<MusicManager>.instance.targetSource.source.volume = Manager<MusicManager>.instance.targetFromVolume;
			Manager<MusicManager>.instance.StopCoroutine(source.fadeRoutine);
		}
		source.fadeRoutine = Manager<MusicManager>.instance.StartCoroutine(FadeRoutine(source, source.volume, 0f, time, stop));
	}

	public static void FadeIn(MusicSource source, float time)
	{
		if (source.fadeRoutine != null)
		{
			Manager<MusicManager>.instance.targetSource.source.volume = Manager<MusicManager>.instance.targetToVolume;
			Manager<MusicManager>.instance.StopCoroutine(source.fadeRoutine);
		}
		source.source.Play();
		source.fadeRoutine = Manager<MusicManager>.instance.StartCoroutine(FadeRoutine(source, 0f, source.volume, time, stop: false));
	}

	private static IEnumerator FadeRoutine(MusicSource source, float fromVolume, float toVolume, float totalTime, bool stop)
	{
		Manager<MusicManager>.instance.targetSource = source;
		Manager<MusicManager>.instance.targetFromVolume = fromVolume;
		Manager<MusicManager>.instance.targetToVolume = toVolume;
		for (float time = 0f; time < totalTime; time += Time.unscaledDeltaTime)
		{
			source.source.volume = Mathf.Lerp(fromVolume, toVolume, time / totalTime);
			yield return new WaitForEndOfFrame();
		}
		source.source.volume = toVolume;
		if (stop)
		{
			Stop(source);
		}
		source.fadeRoutine = null;
	}

	public static void SetToBossMusic()
	{
		gameSource.source.clip = Manager<MusicManager>.instance.bossMusic;
		gameSource.source.loop = true;
		gameSource.source.Play();
	}

	public static void SetToBossEndMusic()
	{
		Manager<MusicManager>.instance.StartCoroutine(Manager<MusicManager>.instance.BossMusicEnd());
	}

	public static Coroutine StartBossEndMusicRoutine()
	{
		return Manager<MusicManager>.instance.StartCoroutine(Manager<MusicManager>.instance.BossMusicEnd());
	}

	private IEnumerator BossMusicEnd()
	{
		gameSource.source.clip = bossEndMusic;
		gameSource.source.loop = false;
		gameSource.source.Play();
		yield return new WaitForSecondsRealtime(gameSource.source.clip.length);
		gameSource.source.clip = gameMusic;
		gameSource.source.loop = true;
		gameSource.source.Play();
	}

	public static void Stop(MusicSource source)
	{
		source.source.Stop();
	}

	private void OnEnable()
	{
		if (Manager<MusicManager>.instance == null)
		{
			Manager<MusicManager>.instance = this;
			AudioSource[] components = GetComponents<AudioSource>();
			menuSource = new MusicSource
			{
				source = components[0],
				volume = components[0].volume
			};
			gameSource = new MusicSource
			{
				source = components[1],
				volume = components[1].volume
			};
			menuSource.source.volume = 0f;
			menuSource.source.Stop();
			gameSource.source.volume = 0f;
			gameSource.source.Stop();
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}
}
