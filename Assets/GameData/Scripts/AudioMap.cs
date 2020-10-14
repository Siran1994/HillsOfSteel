using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Audio;

public class AudioMap : ScriptableObject
{
	[Serializable]
	public class AudioClipWithVolume
	{
		public AudioClip audioClip;

		[Range(0f, 1f)]
		public float volume;

		public bool loopingSound;
	}

	public static AudioMap instance;

	public List<AudioClipWithVolume> tankShellHitsGround;

	public List<AudioClipWithVolume> tankPartHitsAnything;

	public List<AudioClipWithVolume> tankHit;

	public List<AudioClipWithVolume> tankGunReloaded;

	public List<AudioClipWithVolume> tankBurning;

	public List<AudioClipWithVolume> tankDestroyed;

	public List<AudioClipWithVolume> tankExplodes;

	public List<AudioClipWithVolume> tankIgnition;

	public List<AudioClipWithVolume> playerTankFiring;

	public List<AudioClipWithVolume> playerTankMissileFiring;

	public List<AudioClipWithVolume> playerEngineIdle;

	public List<AudioClipWithVolume> smallTankFiring;

	public List<AudioClipWithVolume> tigerTankFiring;

	public List<AudioClipWithVolume> stuartTankFiring;

	public List<AudioClipWithVolume> mortarTankFiring;

	public List<AudioClipWithVolume> missileTankFiring;

	public List<AudioClipWithVolume> machineGunTankFiring;

	public List<AudioClipWithVolume> machineGunMiss;

	public List<AudioClipWithVolume> copterAiming;

	public List<AudioClipWithVolume> copterFiring;

	public List<AudioClipWithVolume> copterFiringEnd;

	public List<AudioClipWithVolume> minigunFiringStart;

	public List<AudioClipWithVolume> minigunFiring;

	public List<AudioClipWithVolume> minigunFiringEnd;

	public List<AudioClipWithVolume> flamerFiring;

	public List<AudioClipWithVolume> lightningFiring;

	public List<AudioClipWithVolume> projectileSwoosh;

	public List<AudioClipWithVolume> promotion;

	public List<AudioClipWithVolume> scoreroll;

	public List<AudioClipWithVolume> coinroll;

	public List<AudioClipWithVolume> buttonPress;

	public List<AudioClipWithVolume> purchase;

	public List<AudioClipWithVolume> upgradeTank;

	public List<AudioClipWithVolume> ownTankDestroyed;

	public List<AudioClipWithVolume> death;

	public List<AudioClipWithVolume> flyingDeath;

	public List<AudioClipWithVolume> bodyThud;

	public List<AudioClipWithVolume> logoHit;

	public List<AudioClipWithVolume> revive;

	public List<AudioClipWithVolume> allUpgraded;

	public List<AudioClipWithVolume> countdown;

	public List<AudioClipWithVolume> levelUnlock;

	public List<AudioClipWithVolume> levelStart;

	public List<AudioClipWithVolume> laserAim;

	public List<AudioClipWithVolume> laserShoot;

	public List<AudioClipWithVolume> upgradeBooster;

	public List<AudioClipWithVolume> mineSpawn;

	public List<AudioClipWithVolume> mineExplode;

	public List<AudioClipWithVolume> mineBeep;

	public List<AudioClipWithVolume> mammothFiring;

	public List<AudioClipWithVolume> forceFieldAppear;

	public List<AudioClipWithVolume> forceFieldAbsorbBig;

	public List<AudioClipWithVolume> gemCollect;

	public List<AudioClipWithVolume> chestShake;

	public List<AudioClipWithVolume> chestWindup;

	public List<AudioClipWithVolume> chestOpen;

	public List<AudioClipWithVolume> chestOpenChime;

	public List<AudioClipWithVolume> cardReveal;

	public List<AudioClipWithVolume> starCollect;

	public List<AudioClipWithVolume> starCollected;

	public List<AudioClipWithVolume> coinDeparture;

	public List<AudioClipWithVolume> tankLevelUp;

	public List<AudioClipWithVolume> progressBarTick;

	public List<AudioClipWithVolume> gemroll;

	public List<AudioClipWithVolume> newTankReveal;

	public List<AudioClipWithVolume> spiderShoot;

	public List<AudioClipWithVolume> spiderWalk;

	public List<AudioClipWithVolume> vipDoubleRoll;

	public List<AudioClipWithVolume> vipDoubleSplash;

	public AudioMixerGroup effectsMixerGroup;

	public AudioMixerGroup engineMixerGroup;

	public AudioMixerGroup uiMixerGroup;

	public AudioClipWithVolume this[string name]
	{
		get
		{
			List<AudioClipWithVolume> list = null;
			FieldInfo[] fields = GetType().GetFields(BindingFlags.Instance | BindingFlags.Public);
			foreach (FieldInfo fieldInfo in fields)
			{
				if (fieldInfo.Name == name)
				{
					list = (List<AudioClipWithVolume>)fieldInfo.GetValue(this);
				}
			}
			if (list != null)
			{
				return list[UnityEngine.Random.Range(0, list.Count)];
			}
			return null;
		}
		set
		{
		}
	}

	private void OnEnable()
	{
		instance = this;
	}

	public static AudioSource PlayClipAt(AudioMap audioMap, string name, Vector3 pos, AudioMixerGroup mixerGroup, float delay = 0f)
	{
		return PlayClipAt(audioMap[name], pos, mixerGroup, delay);
	}

	public static AudioSource PlayClipAt(string name, Vector3 pos, AudioMixerGroup mixerGroup, float delay = 0f)
	{
		return PlayClipAt(instance[name], pos, mixerGroup, delay);
	}

	private static AudioSource PlayClipAt(List<AudioClipWithVolume> clips, Vector3 pos, AudioMixerGroup mixerGroup, float delay = 0f)
	{
		return PlayClipAt(clips[UnityEngine.Random.Range(0, clips.Count)], pos, mixerGroup, delay);
	}

	public static void PlayClip(AudioClipWithVolume clip, AudioSource audioSource)
	{
		audioSource.clip = clip.audioClip;
		audioSource.volume = clip.volume;
		audioSource.loop = clip.loopingSound;
		audioSource.Play();
	}

	public static AudioSource PlayClipAt(AudioClipWithVolume clip, Vector3 pos, AudioMixerGroup mixerGroup, float delay = 0f)
	{
		AudioSource audioSource = null;
		if (clip != null)
		{
			GameObject gameObject = new GameObject(clip.audioClip.name);
			if ((bool)TankGame.instance)
			{
				gameObject.transform.parent = TankGame.instance.transform;
			}
			gameObject.transform.position = pos;
			audioSource = gameObject.AddComponent<AudioSource>();
			audioSource.clip = clip.audioClip;
			audioSource.loop = clip.loopingSound;
			audioSource.rolloffMode = AudioRolloffMode.Linear;
			audioSource.volume = clip.volume;
			audioSource.outputAudioMixerGroup = mixerGroup;
			audioSource.PlayDelayed(delay);
			if (!audioSource.loop)
			{
				UnityEngine.Object.Destroy(gameObject, clip.audioClip.length);
			}
		}
		return audioSource;
	}
}
