using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AnimatedCurrencyController : MonoBehaviour
{
	private class AnimatedCurrency
	{
		public Action<int> onLeave;

		public Action<int> onArrive;

		public Image image;

		public float time;

		public LTSpline spline;

		public int value;

		public string audioName;
	}

	public static AnimatedCurrencyController instance;

	public Sprite coinSprite;

	public Sprite gemSprite;

	public Sprite silverStarSprite;

	public Sprite goldenStarSprite;

	public AudioSource[] sounds;

	public Image[] animatedCurrencyImages;

	private List<AnimatedCurrency> animatedCurrencies;

	private List<AnimatedCurrency> animatedCurrenciesInFlight;

	private Coroutine animateRoutine;

	private void OnEnable()
	{
		instance = this;
		StartAnimateRoutine();
	}

	private void TryPlayAudio(string audioName)
	{
		int num = 0;
		while (true)
		{
			if (num < sounds.Length)
			{
				if (!sounds[num].isPlaying)
				{
					break;
				}
				num++;
				continue;
			}
			return;
		}
		AudioMap.AudioClipWithVolume audioClipWithVolume = AudioMap.instance[audioName];
		sounds[num].clip = audioClipWithVolume.audioClip;
		sounds[num].volume = audioClipWithVolume.volume;
		sounds[num].Play();
	}

	private void StartAnimateRoutine()
	{
		if (animateRoutine == null)
		{
			animatedCurrencies = new List<AnimatedCurrency>(animatedCurrencyImages.Length);
			animatedCurrenciesInFlight = new List<AnimatedCurrency>(animatedCurrencyImages.Length);
			for (int i = 0; i < animatedCurrencyImages.Length; i++)
			{
				animatedCurrencies.Add(new AnimatedCurrency
				{
					image = animatedCurrencyImages[i]
				});
			}
			animateRoutine = StartCoroutine(AnimateRoutine());
		}
	}

	public void StopAnimateRoutine()
	{
		if (animateRoutine != null)
		{
			foreach (AnimatedCurrency item in animatedCurrenciesInFlight)
			{
				item.image.enabled = false;
			}
			StopCoroutine(animateRoutine);
			animateRoutine = null;
		}
	}

	private IEnumerator AnimateRoutine()
	{
		float totalTime = 1.25f;
		while (true)
		{
			for (int i = 0; i < animatedCurrenciesInFlight.Count; i++)
			{
				AnimatedCurrency animatedCurrency = animatedCurrenciesInFlight[i];
				if (animatedCurrency.time > 0f)
				{
					if (animatedCurrency.onLeave != null)
					{
						animatedCurrency.onLeave(animatedCurrency.value);
					}
					animatedCurrency.image.enabled = true;
					if (animatedCurrency.time > totalTime)
					{
						animatedCurrenciesInFlight.RemoveAt(i);
						i--;
						animatedCurrencies.Add(animatedCurrency);
						animatedCurrency.image.enabled = false;
						if (animatedCurrency.onArrive != null)
						{
							animatedCurrency.onArrive(animatedCurrency.value);
						}
						TryPlayAudio(animatedCurrency.audioName);
					}
					else
					{
						animatedCurrency.image.transform.position = animatedCurrency.spline.interp(animatedCurrency.time / totalTime);
					}
				}
				else if (animatedCurrency.time + Time.unscaledDeltaTime > 0f)
				{
					TryPlayAudio(animatedCurrency.audioName);
				}
				animatedCurrency.time += Time.unscaledDeltaTime;
			}
			yield return null;
		}
	}

	public static void CancelAllAnimations()
	{
		foreach (AnimatedCurrency item in instance.animatedCurrenciesInFlight)
		{
			item.image.enabled = false;
		}
		instance.animatedCurrencies.AddRange(instance.animatedCurrenciesInFlight);
		instance.animatedCurrenciesInFlight.Clear();
	}

	public static void AnimateCoins(int count, Vector3 fromViewportSpace, Vector3 toViewportSpace, int countPerSprite = 1, Action<int> onLeaveTick = null, Action<int> onArriveTick = null, bool departureSound = true)
	{
		if (departureSound)
		{
			AudioMap.PlayClipAt("coinDeparture", Vector3.zero, AudioMap.instance.uiMixerGroup);
		}
		AnimatedCurrencySettings animatedCurrencySettings = default(AnimatedCurrencySettings);
		animatedCurrencySettings.onLeave = onLeaveTick;
		animatedCurrencySettings.onArrive = onArriveTick;
		animatedCurrencySettings.count = count;
		animatedCurrencySettings.countPerSprite = countPerSprite;
		animatedCurrencySettings.audioName = "coinroll";
		animatedCurrencySettings.icon = instance.coinSprite;
		animatedCurrencySettings.fromViewportSpace = fromViewportSpace;
		animatedCurrencySettings.toViewportSpace = toViewportSpace;
		AnimatedCurrencySettings settings = animatedCurrencySettings;
		instance.AnimateCurrencies(settings);
	}

	public static void AnimateGems(int count, Vector3 fromViewportSpace, Vector3 toViewportSpace, int countPerSprite = 1, Action<int> onLeaveTick = null, Action<int> onArriveTick = null)
	{
		AudioMap.PlayClipAt("gemCollect", Vector3.zero, AudioMap.instance.uiMixerGroup);
		AnimatedCurrencySettings animatedCurrencySettings = default(AnimatedCurrencySettings);
		animatedCurrencySettings.onLeave = onLeaveTick;
		animatedCurrencySettings.onArrive = onArriveTick;
		animatedCurrencySettings.count = count*50;
		animatedCurrencySettings.countPerSprite = countPerSprite;
		animatedCurrencySettings.audioName = "gemroll";
		animatedCurrencySettings.icon = instance.gemSprite;
		animatedCurrencySettings.fromViewportSpace = fromViewportSpace;
		animatedCurrencySettings.toViewportSpace = toViewportSpace;
		AnimatedCurrencySettings settings = animatedCurrencySettings;
		instance.AnimateCurrencies(settings);
	}

	public static void AnimateSilverStars(int count, Vector3 fromViewportSpace, Vector3 toViewportSpace, int countPerSprite = 1, Action<int> onLeaveTick = null, Action<int> onArriveTick = null)
	{
		AudioMap.PlayClipAt(AudioMap.instance["starCollect"], Vector3.zero, AudioMap.instance.uiMixerGroup);
		AnimatedCurrencySettings animatedCurrencySettings = default(AnimatedCurrencySettings);
		animatedCurrencySettings.onLeave = onLeaveTick;
		animatedCurrencySettings.onArrive = onArriveTick;
		animatedCurrencySettings.count = count;
		animatedCurrencySettings.countPerSprite = countPerSprite;
		animatedCurrencySettings.audioName = "starCollected";
		animatedCurrencySettings.icon = instance.silverStarSprite;
		animatedCurrencySettings.fromViewportSpace = fromViewportSpace;
		animatedCurrencySettings.toViewportSpace = toViewportSpace;
		AnimatedCurrencySettings settings = animatedCurrencySettings;
		instance.AnimateCurrencies(settings);
	}

	public static void AnimateGoldenStars(int count, Vector3 fromViewportSpace, Vector3 toViewportSpace, int countPerSprite = 1, Action<int> onLeaveTick = null, Action<int> onArriveTick = null)
	{
		AudioMap.PlayClipAt(AudioMap.instance["starCollect"], Vector3.zero, AudioMap.instance.uiMixerGroup);
		AnimatedCurrencySettings animatedCurrencySettings = default(AnimatedCurrencySettings);
		animatedCurrencySettings.onLeave = onLeaveTick;
		animatedCurrencySettings.onArrive = onArriveTick;
		animatedCurrencySettings.count = count;
		animatedCurrencySettings.countPerSprite = countPerSprite;
		animatedCurrencySettings.audioName = "starCollected";
		animatedCurrencySettings.icon = instance.goldenStarSprite;
		animatedCurrencySettings.fromViewportSpace = fromViewportSpace;
		animatedCurrencySettings.toViewportSpace = toViewportSpace;
		AnimatedCurrencySettings settings = animatedCurrencySettings;
		instance.AnimateCurrencies(settings);
	}

	private void AnimateCurrencies(AnimatedCurrencySettings settings)
	{
		if (animatedCurrencies.Count == 0)
		{
			return;
		}
		Vector3 vector = MenuController.UICamera.ViewportToWorldPoint(settings.fromViewportSpace);
		Vector3 vector2 = MenuController.UICamera.ViewportToWorldPoint(settings.toViewportSpace);
		vector.z = base.transform.position.z;
		vector2.z = base.transform.position.z;
		Vector3 vector3 = vector - vector2;
		Vector3 normalized = vector3.normalized;
		int count = settings.count;
		int num = count / settings.countPerSprite;
		int num2 = 0;
		LTSpline spline;
		while (true)
		{
			if (num2 < num)
			{
				spline = new LTSpline(new Vector3[6]
				{
					vector,
					vector,
					vector + (Vector3)UnityEngine.Random.insideUnitCircle * 2.5f + normalized * vector3.magnitude * 0.2f,
					vector + (Vector3)UnityEngine.Random.insideUnitCircle * 2.5f + normalized * vector3.magnitude * 0.05f,
					vector2,
					vector2
				});
				if (animatedCurrencies.Count > 1)
				{
					AnimatedCurrency animatedCurrency = animatedCurrencies[animatedCurrencies.Count - 1];
					animatedCurrencies.RemoveAt(animatedCurrencies.Count - 1);
					animatedCurrency.onArrive = settings.onArrive;
					animatedCurrency.onLeave = settings.onLeave;
					animatedCurrency.image.sprite = settings.icon;
					animatedCurrency.image.SetNativeSize();
					animatedCurrency.time = (float)(-num2) * 0.01f;
					animatedCurrency.value = ((num2 == num - 1) ? (count - settings.countPerSprite * num2) : settings.countPerSprite);
					animatedCurrency.spline = spline;
					animatedCurrency.audioName = settings.audioName;
					animatedCurrenciesInFlight.Insert(0, animatedCurrency);
				}
				else if (animatedCurrencies.Count == 1)
				{
					break;
				}
				num2++;
				continue;
			}
			return;
		}
		AnimatedCurrency animatedCurrency2 = animatedCurrencies[animatedCurrencies.Count - 1];
		animatedCurrencies.RemoveAt(animatedCurrencies.Count - 1);
		animatedCurrency2.onArrive = settings.onArrive;
		animatedCurrency2.onLeave = settings.onLeave;
		animatedCurrency2.image.sprite = settings.icon;
		animatedCurrency2.image.SetNativeSize();
		animatedCurrency2.time = (float)(-num2) * 0.01f;
		animatedCurrency2.value = count - settings.countPerSprite * num2;
		animatedCurrency2.spline = spline;
		animatedCurrency2.audioName = settings.audioName;
		animatedCurrenciesInFlight.Insert(0, animatedCurrency2);
	}
}
