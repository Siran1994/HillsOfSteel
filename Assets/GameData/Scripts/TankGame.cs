using AI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#pragma warning disable 0618
public class TankGame : MonoBehaviour
{
	public delegate void PlayerShotDelegate(BulletDefinition bulletDef);

	public delegate void BoosterUsedDelegate();

	public delegate void ScoreAddedDelegate(int score);

	public delegate void SetBossHealthDelegate(float v);

	public delegate void SetStageProgressDelegate(float p);

	public delegate IEnumerator BossIntroDelegate(TankGame game, Enemy bossInfo);

	public delegate void PlayerHealthDelegate();

	public delegate void SetHighscoreDelegate(int score);

	public delegate IEnumerator ArenaIntroDelegate(TankGame game);

	public delegate IEnumerator GameBeginDelegate(TankGame game, GameMode mode);

	public delegate IEnumerator GameEndDelegate(TankGame game, GameMode mode);

	public delegate IEnumerator GameTimeDelegate(TankGame game, GameMode mode, float time);

	public delegate IEnumerator SuddenDeathDelegate(TankGame game, GameMode mode);

	public static float FlippedOverAngleMin = 120f;

	public static float FlippedOverAngleMax = 240f;

	public static TankGame instance;

	public Transform cameraContainer;

	public Transform backBlockerContainer;

	public Transform frontBlockerContainer;

	public Variables variables;

	public FlagData flagData;

	public AudioMap audioMap;

	public GameObject coinPrefab;

	public GameObject particleEffectBlackSmoke;

	public GameObject bossHealthPack;

	public GameObject damageRangePrefab;

	public GameObject sparkleParticlePrefab;

	public GameObject airstrikeMissilePrefab;

	public GameObject shockWavePrefab;

	public GameObject minePrefab;

	public GameObject forceFieldPrefab;

	public Theme theme;

	public float themeOffset;

	public List<Camera> ingameCameras;

	public Camera backgroundCamera;

	[Header("PvP AI settings")]
	public AIController aiController;

	public bool arenaTanksShouldCollide;

	public static PlayerShotDelegate OnPlayerShot;

	public static BoosterUsedDelegate OnBoosterUsed;

	public static ScoreAddedDelegate OnScoreAdded;

	public static SetBossHealthDelegate OnSetBossHealth;

	public static SetStageProgressDelegate OnSetStageProgress;

	public static BossIntroDelegate OnBossIntro;

	public static PlayerHealthDelegate OnPlayerHealth;

	public static SetHighscoreDelegate OnSetHighscore;

	public static ArenaIntroDelegate OnArenaIntro;

	public static GameBeginDelegate OnGameBegin;

	public static GameEndDelegate OnGameEnd;

	public static GameTimeDelegate OnGameTime;

	public static SuddenDeathDelegate OnSuddenDeath;

	private List<BulletObject>[] bulletCache;

	private Queue<Vehicle> destroyedVehicles;

	private List<BaseController> playerControllers;

	public Transform cameraFollowTransform;

	public TankContainer playerTankContainer;

	public List<TankContainer> arenaAllies;

	public List<TankContainer> arenaEnemies;

	public int arenaAlliesAliveCount;

	public int arenaEnemiesAliveCount;

	private int[] enemyCountByType;

	private int score;

	private int enemiesKilled;

	private int coinsGotten;

	private int lastBoss = -1;

	public bool playerInvincible;

	private float flippedOverTime;

	private float playStartTime;

	private float playEndTime;

	private float frameCountLast;

	private float frameTimefromLast;

	private Queue<float> frameTimeHistory = new Queue<float>(5);

	public Vector2[] aiSamplePoints;

	public float[] aiSampleDerivatives;

	public List<Vector2> aiSamplePointMaxima;

	public List<Vector2> aiSamplePointMinima;

	public List<Vector3> aiSamplePointMaximaDerivatives;

	public List<Vector3> aiSamplePointMinimaDerivatives;

	public Tank playerTank;

	public Vector2 cameraShake;

	private Vector2 cameraOffset;

	private float previousGroundMeshBuiltAtOffset;

	private float nextSpawnPosition;

	private int spawnCount;

	private object scrapCountLock = new object();

	private int currentScrapCount;

	private Enemy boss;

	private List<Enemy> enemies = new List<Enemy>();

	private int enemiesSpawned;

	private int stagesCleared;

	private float stageDifficultyMultiplier = 1f;

	private int nextBossKillCount;

	private int bossRushCounter;

	private bool bossRushStarted;

	private List<BulletDefinition> bulletDefs = new List<BulletDefinition>();

	public List<Bullet> bullets = new List<Bullet>();

	private GameObject bulletCacheParent;

	private GameObject coinParent;

	private int[] groundIndices;

	private Vector2[] groundColliderVertices;

	private Vector3[] groundVertices;

	private Vector3[] middleGroundMaskVertices;

	private Vector2[] groundUvs;

	private Vector2[] middleGroundUvs;

	private GameObject[][] backgrounds;

	private int[] currentBackgroundIndex;

	private Vector3 lastCameraPos;

	private Variables.GroundGenerationSettings groundGenerationSettings;

	private bool levelLoading;

	private object deathLock = new object();

	private bool arenaIntroDone;

	private const int CacheShiftInterval = 20;

	private float?[] groundHeightCache = new float?[1000];

	private int groundHeightCacheOffset = -100;

	private int nextGroundHeightCacheShiftX = 20;

	public static int SelectedLevel
	{
		get;
		set;
	}

	public static bool Running
	{
		get;
		set;
	}

	public bool BossKilled
	{
		get;
		set;
	}

	public bool ReviveTried
	{
		get;
		set;
	}

	public bool ReviveUsed
	{
		get;
		set;
	}

	public bool AllLivesUsed
	{
		get;
		set;
	}

	public bool BossIsOn => instance.boss != null;

	public Enemy Boss => boss;

	public bool JustSpawnedCoins
	{
		get;
		private set;
	}

	public bool PlayerSurrendered
	{
		get;
		set;
	}

	public bool IsLowEndDevice
	{
		get;
		private set;
	}

	public int ArenaAlliesAliveCount
	{
		get
		{
			int num = 0;
			foreach (TankContainer arenaAlly in arenaAllies)
			{
				if (arenaAlly.CurrentHealth > 0f)
				{
					num++;
				}
			}
			return num;
		}
	}

	public int ArenaEnemiesAliveCount
	{
		get
		{
			int num = 0;
			foreach (TankContainer arenaEnemy in arenaEnemies)
			{
				if (arenaEnemy.CurrentHealth > 0f)
				{
					num++;
				}
			}
			return num;
		}
	}

	private void Awake()
	{
		instance = this;
	}

	public BulletObject GetBullet(int bulletTypeIndex)
	{
		int index = 0;
		if (bulletCache != null && bulletTypeIndex < bulletCache.Length)
		{
			if (bulletCache[bulletTypeIndex].Count == 0)
			{
				for (int i = 0; i < 10; i++)
				{
					bulletCache[bulletTypeIndex].Add(NewBullet(bulletDefs[bulletTypeIndex], bulletCacheParent));
				}
			}
			BulletObject bulletObject = bulletCache[bulletTypeIndex][index];
			if (bulletObject.bullet != null)
			{
				bulletCache[bulletTypeIndex].RemoveAt(index);
				return bulletObject;
			}
		}
		return null;
	}

	public BulletObject GetBullet(Vehicle vehicle)
	{
		return GetBullet(vehicle.BulletTypeIndex);
	}

	public void DestroyBullet(Bullet bullet, float time)
	{
		bullet.transform.GetComponentInChildren<SpriteRenderer>(includeInactive: true).gameObject.SetActive(value: false);
		bullet.transform.GetComponent<Rigidbody2D>().simulated = false;
		Delay(time, delegate
		{
			if (bullet != null)
			{
				if ((bool)bullet.transform && (bool)bullet.transform.gameObject)
				{
					bullet.transform.gameObject.SetActive(value: false);
				}
				if (bullet.cacheIndex == -1)
				{
					bullets.Remove(bullet);
					if ((bool)bullet.transform && (bool)bullet.transform.gameObject)
					{
						UnityEngine.Object.Destroy(bullet.transform.gameObject);
					}
				}
				else if (bullet.cacheIndex > 0 && bullet.bulletObject != null)
				{
					bulletCache[bullet.cacheIndex].Add(bullet.bulletObject);
				}
			}
		});
	}

	public static BulletObject NewBullet(BulletDefinition bulletDef, GameObject parent)
	{
		if (bulletDef != null && (bulletDef.type == BulletType.Grenade || bulletDef.type == BulletType.Missile || bulletDef.type == BulletType.Laser || bulletDef.type == BulletType.Cannon))
		{
			BulletObject bulletObject = new BulletObject();
			bulletObject.bullet = UnityEngine.Object.Instantiate(bulletDef.prefab, parent.transform);
			bulletObject.bullet.SetActive(value: false);
			GameObject original = (bulletDef.particleEffectShoot != null) ? bulletDef.particleEffectShoot : Variables.instance.particleEffectShoot;
			bulletObject.shootParticles = UnityEngine.Object.Instantiate(original, parent.transform);
			bulletObject.shootParticles.SetActive(value: false);
			GameObject original2 = (bulletDef.particleEffectBulletHitGround != null) ? bulletDef.particleEffectBulletHitGround : Variables.instance.particleEffectBulletHitGround;
			bulletObject.groundHitExplosion = UnityEngine.Object.Instantiate(original2, parent.transform);
			bulletObject.groundHitExplosion.SetActive(value: false);
			GameObject original3 = (bulletDef.particleEffectBulletHitTank != null) ? bulletDef.particleEffectBulletHitTank : Variables.instance.particleEffectBulletHitTank;
			bulletObject.tankHitExplosion = UnityEngine.Object.Instantiate(original3, parent.transform);
			bulletObject.tankHitExplosion.SetActive(value: false);
			return bulletObject;
		}
		return null;
	}

	public static void SetTheme(Theme theme)
	{
		instance.theme = theme;
	}

	public void SetLevel(int level, bool loadingScreen, Action callback = null)
	{
		if (level != SelectedLevel)
		{
			StartCoroutine(SetLevelRoutine(level, loadingScreen, callback));
		}
		else
		{
			callback?.Invoke();
		}
	}

	private IEnumerator SetLevelRoutine(int level, bool loadingScreen, Action callback)
	{
		if (levelLoading)
		{
			yield break;
		}
		levelLoading = true;
		int oldLevel = SelectedLevel;
		SelectedLevel = level;
		themeOffset = 0f;
		if (PlayerDataManager.IsSelectedGameModePvP)
		{
			themeOffset = variables.levelOffsets.Random();
		}
		this.theme = null;
		if (loadingScreen)
		{
			yield return LoadingScreen.FadeIn();
		}
		AsyncOperation async = null;
		try
		{
			Scene sceneByName = SceneManager.GetSceneByName("Level " + (oldLevel + 1));
			if (sceneByName.IsValid() && sceneByName.isLoaded)
			{
				async = SceneManager.UnloadSceneAsync(sceneByName);
			}
		}
		catch (Exception)
		{
		}
		while (async != null && !async.isDone)
		{
			yield return null;
		}
		SceneManager.LoadScene("Level " + (level + 1), LoadSceneMode.Additive);
		yield return null;
		Theme theme = UnityEngine.Object.FindObjectOfType<Theme>();
		groundGenerationSettings = variables.GetGroundGenerationSettings(PlayerDataManager.SelectedGameMode);
		groundIndices = new int[(groundGenerationSettings.groundPrecision - 1) * 6];
		groundColliderVertices = new Vector2[groundGenerationSettings.groundPrecision + 2];
		groundVertices = new Vector3[groundGenerationSettings.groundPrecision * 2];
		middleGroundMaskVertices = new Vector3[groundGenerationSettings.groundPrecision * 2];
		groundUvs = new Vector2[groundGenerationSettings.groundPrecision * 2];
		middleGroundUvs = new Vector2[groundGenerationSettings.groundPrecision * 2];
		if (backgrounds != null)
		{
			GameObject[][] array = backgrounds;
			foreach (GameObject[] array2 in array)
			{
				for (int j = 0; j < array2.Length; j++)
				{
					UnityEngine.Object.Destroy(array2[j]);
				}
			}
		}
		backgrounds = new GameObject[theme.layers.Length][];
		currentBackgroundIndex = new int[theme.layers.Length];
		for (int k = 0; k < theme.layers.Length; k++)
		{
			currentBackgroundIndex[k] = 1;
			backgrounds[k] = new GameObject[3];
			backgrounds[k][0] = theme.layers[k].gameObject;
			backgrounds[k][0].transform.parent = backgroundCamera.transform;
			ParallaxLayer component = theme.layers[k].GetComponent<ParallaxLayer>();
			for (int l = 1; l < backgrounds[k].Length; l++)
			{
				Vector3 position = new Vector3(component.bounds.size.x * (float)l, component.transform.position.y, component.transform.position.z);
				backgrounds[k][l] = UnityEngine.Object.Instantiate(theme.layers[k].gameObject, position, Quaternion.identity, backgroundCamera.transform);
			}
		}
		this.theme = theme;
		UpdateGroundMesh(ingameCameras[0].transform.position.x);
		if (coinParent != null)
		{
			UnityEngine.Object.Destroy(coinParent);
		}
		if (boss != null && boss.vehicleContainer != null)
		{
			UnityEngine.Object.Destroy(boss.vehicleContainer.gameObject);
			boss = null;
		}
		for (int m = 0; m < enemies.Count; m++)
		{
			if (enemies[m] != null && enemies[m].vehicleContainer != null)
			{
				UnityEngine.Object.Destroy(enemies[m].vehicleContainer.gameObject);
			}
		}
		enemies.Clear();
		while (destroyedVehicles.Count > 0)
		{
			Vehicle vehicle = destroyedVehicles.Dequeue();
			if (vehicle != null)
			{
				if (vehicle.driverRagdoll != null)
				{
					UnityEngine.Object.Destroy(vehicle.driverRagdoll.gameObject);
				}
				UnityEngine.Object.Destroy(vehicle.gameObject);
			}
		}
		if (playerTankContainer != null)
		{
			if (playerTankContainer.driverRagdoll != null)
			{
				UnityEngine.Object.Destroy(playerTankContainer.driverRagdoll.gameObject);
			}
			UnityEngine.Object.Destroy(playerTankContainer.gameObject);
		}
		callback?.Invoke();
		if (loadingScreen)
		{
			yield return new WaitForSecondsRealtime(0.25f);
			yield return LoadingScreen.FadeOut();
		}
		levelLoading = false;
	}

	private void OnDisable()
	{
		try
		{
			Scene sceneByName = SceneManager.GetSceneByName("Level " + (SelectedLevel + 1));
			if (sceneByName.isLoaded)
			{
				SceneManager.UnloadSceneAsync(sceneByName);
			}
		}
		catch (Exception)
		{
		}
		RemovePlayerControllers();
	}

	public void AddCoins(int coins, bool sync = true, bool cloudSync = false)
	{
		coinsGotten += coins;
		PlayerDataManager.AddCoins(coins, sync, cloudSync);
	}

	private void SetPlayerBooster()
	{
		playerTankContainer.Booster = PlayerDataManager.GetSelectedBooster(playerTank);
		playerTankContainer.SetTankBooster(playerTankContainer.Booster);
	}

	private void FlipTank(TankContainer tank)
	{
		tank.Flip();
	}

	private TankContainer SpawnPlayer(Vector3 spawnPos, Tank tank, int skin, int level, Driver.Uniform uniform = null)
	{
        GameObject gameObject = UnityEngine.Object.Instantiate(tank.prefab, spawnPos, Quaternion.identity, base.transform);
        if (!gameObject.GetComponent<Rigidbody2D>().isKinematic)
		{
			gameObject.AddComponent<RigidbodyRotationConstraint>();
		}
		TankContainer component = gameObject.GetComponent<TankContainer>();
		component.SetHat(variables.levels[SelectedLevel].hat);
		int num = component.BulletTypeIndex = variables.GetTankIndex(tank.id);
		component.BulletDef = tank.bullet;
		component.SetSkin(tank.tankSkins[skin]);
		component.SetDustColors(variables.dustColors);
		component.BlinkHealth = true;
		TankProgression.Stats progression = tank.GetProgression(level);
		if (PlayerDataManager.IsSelectedGameModePvP)
		{
			progression.health *= 0.25f;
		}
		component.Stats = progression;
		if (uniform != null)
		{
			component.GetComponentInChildren<Driver>().SetUniform(uniform);
		}
		return component;
	}
    /// <summary>
    /// //////////////////////////////////////////////////////////////////////////////////////////////
    /// </summary>
    /// <param name="spawnPos"></param>
    /// <returns></returns>
	private TankContainer SpawnLocalPlayer(Vector3 spawnPos)
	{
		Driver.Uniform uniform = PlayerDataManager.IsSubscribed() ? Variables.instance.vipUniform : Variables.instance.allyUniform;
        TankContainer tankContainer = SpawnPlayer(spawnPos, playerTank, PlayerDataManager.GetSelectedSkin(playerTank), PlayerDataManager.GetTankUpgradeLevel(playerTank), uniform);
        tankContainer.GetComponent<SortingGroup>().sortingOrder += 100;
		if (playerControllers != null)
		{
			foreach (BaseController playerController in playerControllers)
			{
				tankContainer.RegisterController(playerController);
			}
			return tankContainer;
		}
		return tankContainer;
	}

	public static void AddPlayerController(BaseController controller)
	{
		if (instance.playerControllers == null)
		{
			instance.playerControllers = new List<BaseController>();
		}
		if (!instance.playerControllers.Contains(controller))
		{
			instance.playerControllers.Add(controller);
		}
		if (instance.playerTankContainer != null)
		{
			instance.playerTankContainer.RegisterController(controller);
		}
	}

	private void RemovePlayerControllers()
	{
		if (playerControllers != null)
		{
			foreach (BaseController playerController in playerControllers)
			{
				if (playerController != null)
				{
					playerController.Unregister();
				}
			}
		}
		if (playerTankContainer != null)
		{
			playerTankContainer.Shoot(val: false);
			playerTankContainer.SetSpeed(0f, 0f);
		}
	}

	public void SetLayerRecursive(Transform t, int layer)
	{
		t.gameObject.layer = layer;
		foreach (Transform item in t)
		{
			item.gameObject.layer = layer;
			SetLayerRecursive(item, layer);
		}
	}

	private IEnumerator SetFacebookProfilePhoto(string facebookId, RawImage image)
	{
		if (!string.IsNullOrEmpty(facebookId))
		{
			WWW www = new WWW("http://graph.facebook.com/" + facebookId + "/picture?type=square");
			yield return www;
			if (string.IsNullOrEmpty(www.error))
			{
				Texture2D texture = new Texture2D(512, 512, TextureFormat.ARGB32, mipChain: false, linear: false);
				www.LoadImageIntoTexture(texture);
				image.texture = texture;
				image.gameObject.SetActive(value: true);
			}
		}
		else
		{
			image.gameObject.SetActive(value: false);
		}
	}

	private IEnumerator Start()
	{
		MenuController.ShowMenu<GameMenu>();
		groundGenerationSettings = variables.GetGroundGenerationSettings(PlayerDataManager.SelectedGameMode);
		destroyedVehicles = new Queue<Vehicle>(20);
		if (theme == null)
		{
			int level = (PlayerDataManager.SelectedGameMode == GameMode.Adventure || PlayerDataManager.SelectedGameMode == GameMode.Classic) ? PlayerDataManager.GetSelectedLevel(PlayerDataManager.SelectedGameMode) : PlayerDataManager.SelectedArenaLevel;
			yield return SetLevelRoutine(level, loadingScreen: false, null);
		}
		stagesCleared = ((PlayerDataManager.SelectedGameMode == GameMode.Adventure) ? PlayerDataManager.GetCurrentStage() : 0);
		stageDifficultyMultiplier = 1f + (float)(stagesCleared + 1) * Variables.instance.stageDifficultyAddition;
		SetNextBossKillCount();
		LoadingScreen.instance.canvasGroup.gameObject.SetActive(value: false);
		LoadingScreen.instance.canvasGroup.alpha = 0f;
		ingameCameras.ForEach(delegate(Camera v)
		{
			v.orthographicSize = variables.screenWidth * (float)Screen.height / (float)Screen.width;
		});
		lastCameraPos = ingameCameras[0].transform.position;

        #region 新增功能

        if (GarageMenu.IsConfirm)
        {
            int index = PlayerPrefs.GetInt("TanIndex");
            switch (index)
            {
                case 0:
                    playerTank = variables.GetTank(1);
                    break;
                case 1:
                    playerTank = variables.GetTank(2);
                    break;
                case 2:
                    playerTank = variables.GetTank(3);
                    break;
                case 3:
                    playerTank = variables.GetTank(4);
                    break;
                case 4:
                    playerTank = variables.GetTank(5);
                    break;
                case 5:
                    playerTank = variables.GetTank(6);
                    break;
                case 6:
                    playerTank = variables.GetTank(7);
                    break;
                case 7:
                    playerTank = variables.GetTank(8);
                    break;
            }
        }
        else
        {
            playerTank = variables.GetTank(PlayerDataManager.GetSelectedTank());
        }
        #endregion
        // playerTank = variables.GetTank(PlayerDataManager.GetSelectedTank());
        enemyCountByType = new int[variables.enemies.Count];
		frontBlockerContainer.gameObject.SetActive(PlayerDataManager.IsSelectedGameModePvP);
		coinParent = new GameObject("Coins");
		coinParent.transform.SetParent(base.transform, worldPositionStays: false);
		bulletCacheParent = new GameObject("Bullet Cache");
		bulletCacheParent.transform.SetParent(base.transform);
		int num = variables.tanks.Length + variables.enemies.Count;
		bulletCache = new List<BulletObject>[num];
		bulletDefs = new List<BulletDefinition>(num);
		for (int j = 0; j < variables.tanks.Length; j++)
		{
			Tank tank = variables.tanks[j];
			bulletDefs.Add((tank.bullet.prefab != null) ? tank.bullet : null);
		}
		for (int k = 0; k < variables.enemies.Count; k++)
		{
			bulletDefs.Add((variables.enemies[k].bullet != null) ? variables.enemies[k].bullet : null);
		}
		for (int l = 0; l < bulletCache.Length; l++)
		{
			bulletCache[l] = new List<BulletObject>(20);
			if (bulletDefs[l] == null)
			{
				continue;
			}
			for (int m = 0; m < bulletCache[l].Capacity; m++)
			{
				bulletCache[l].Add(NewBullet(bulletDefs[l], bulletCacheParent));
				if (bulletCache[l][m] != null)
				{
					bulletCache[l][m].bullet.SetActive(value: false);
				}
			}
		}
		while (!Running)
		{
			yield return null;
		}
		MusicManager.CrossFadeToGame();
		Vector3 spawnPos = new Vector3(0f, GetGroundHeight(0f) + theme.groundMesh.transform.position.y + 2f, -0.5f);
		playerTankContainer = SpawnLocalPlayer(spawnPos);
		if (PlayerDataManager.SelectedGameMode == GameMode.Arena)
		{
			playerTankContainer.healthBarBase.gameObject.SetActive(value: false);
			playerTankContainer.SetHealthBar(MenuController.GetMenu<GameMenu>().arenaPlayerHealthbar[0], MenuController.GetMenu<GameMenu>().arenaPlayerHealthbarBase[0], coloredHealth: false);
			if (OnPlayerHealth != null)
			{
				OnPlayerHealth();
			}
			playerTankContainer.BulletLayer = LayerMask.NameToLayer("Bullet");
		}
		else if (PlayerDataManager.SelectedGameMode == GameMode.Arena2v2)
		{
			playerTankContainer.BulletLayer = LayerMask.NameToLayer("AllyBullet");
		}
		SetPlayerBooster();
		cameraFollowTransform = (playerTankContainer.cameraFollowTransform ? playerTankContainer.cameraFollowTransform : playerTankContainer.transform);
		UpdateGroundMesh(playerTankContainer.transform.localPosition.x + 20f);
		nextSpawnPosition = variables.GetRandomEnemySpawnOffset(0f) + 10f;
		if (PlayerDataManager.SelectedGameMode == GameMode.Adventure)
		{
			OnSetHighscore(PlayerDataManager.GetLeaderboardScore(LeaderboardID.Adventure));
		}
		else if (PlayerDataManager.SelectedGameMode == GameMode.Classic)
		{
			OnSetHighscore(PlayerDataManager.GetLeaderboardScore((LeaderboardID)PlayerDataManager.GetSelectedLevel()));
		}
		else if (PlayerDataManager.SelectedGameMode == GameMode.BossRush)
		{
			OnSetHighscore(PlayerDataManager.GetLeaderboardScore(LeaderboardID.BossRush));
		}
		AudioManager.SetMusicTo(0f);
		playStartTime = Time.time;
		AudioMap.PlayClipAt(audioMap, "levelStart", playerTankContainer.transform.position, audioMap.effectsMixerGroup);
		if (PlayerDataManager.IsSelectedGameModePvP)
		{
			PlayerSurrendered = false;
			bool flag = PlayerDataManager.SelectedGameMode == GameMode.Arena;
			arenaAllies = new List<TankContainer>();
			arenaEnemies = new List<TankContainer>();
			arenaAlliesAliveCount = 1;
			arenaEnemiesAliveCount = 0;
			int[] array8 = new int[0];
			string[] array9 = new string[0];
			string[] array10 = new string[0];
			string[] array11 = new string[0];
			Challenge[] array = new Challenge[flag ? 1 : 3];
			int num2;
			int[] array2;
			string[] array3;
			string[] array4;
			string[] array5;
			if (flag)
			{
				Challenge challenge = array[0] = PlayerDataManager.ArenaMatchData.arenaPayload;
				num2 = 2;
				int num3 = (challenge.actualRating != 0) ? challenge.actualRating : challenge.rating;
				array2 = new int[2]
				{
					PlayerDataManager.GetRating(PlayerDataManager.SelectedGameMode),
					num3
				};
				array3 = new string[2]
				{
					TankPrefs.GetString("challengeName", Social.localUser.userName),
					challenge.name
				};
				array4 = new string[2]
				{
					PlayerPrefs.GetString("facebookId", ""),
					challenge.facebookId
				};
				array5 = new string[2]
				{
					PlayerDataManager.GetCountryCode(),
					challenge.countryCode
				};
			}
			else
			{
				array = PlayerDataManager.ArenaMultiMatchData.arenaPayload;
				num2 = array.Length + 1;
				array2 = new int[num2];
				array3 = new string[num2];
				array4 = new string[num2];
				array5 = new string[num2];
				array3[0] = TankPrefs.GetString("challengeName", Social.localUser.userName);
				array2[0] = PlayerDataManager.GetRating(GameMode.Arena2v2);
				array4[0] = PlayerPrefs.GetString("facebookId", "");
				array5[0] = PlayerDataManager.GetCountryCode();
				List<Challenge> list = array.ToList();
				list.Sort(delegate(Challenge first, Challenge other)
				{
					if (first.rating > other.rating)
					{
						return -1;
					}
					return (first.rating < other.rating) ? 1 : 0;
				});
				array = list.ToArray();
				PlayerDataManager.ArenaMultiMatchData.arenaPayload = array;
				for (int n = 0; n != array.Length; n++)
				{
					array3[n + 1] = array[n].name;
					array2[n + 1] = array[n].actualRating;
					array4[n + 1] = array[n].facebookId;
					array5[n + 1] = array[n].countryCode;
				}
			}
			for (int num4 = 0; num4 < num2; num4++)
			{
				ArenaPlayerHUD arenaPlayerHUD = flag ? MenuController.GetMenu<GameMenu>().arenaPlayerHUDs[num4] : MenuController.GetMenu<GameMenu>().arenaMultiplayerHUDs[num4];
				StartCoroutine(SetFacebookProfilePhoto(array4[num4], arenaPlayerHUD.profilePicture));
				if (string.IsNullOrEmpty(array5[num4]))
				{
					arenaPlayerHUD.flag.sprite = flagData.unknownFlag;
				}
				else
				{
					try
					{
						CountryCode countryCode = (CountryCode)Enum.Parse(typeof(CountryCode), array5[num4]);
						arenaPlayerHUD.flag.sprite = flagData.flags[(int)countryCode].sprite;
					}
					catch (Exception)
					{
						arenaPlayerHUD.flag.sprite = flagData.unknownFlag;
					}
				}
				RankStats rankStats = variables.GetRankStats(array2[num4]);
				arenaPlayerHUD.rank.sprite = rankStats.inGameSprite;
				arenaPlayerHUD.rank.SetNativeSize();
				if (array3[num4].Length > 11 && flag)
				{
					arenaPlayerHUD.nameText.text = array3[num4].Substring(0, 9) + "...";
				}
				else
				{
					arenaPlayerHUD.nameText.text = array3[num4];
				}
				if (!flag)
				{
					arenaPlayerHUD.ratingText.text = array2[num4].ToString();
				}
			}
			Challenge[] array6 = array;
			foreach (Challenge challenge2 in array6)
			{
				MinMaxInt minMaxInt = new MinMaxInt(0, variables.GetMaxTankUpgradeLevel(challenge2.tankId));
				challenge2.armorLevel = minMaxInt.ClampInside(challenge2.armorLevel);
				challenge2.engineLevel = minMaxInt.ClampInside(challenge2.engineLevel);
				challenge2.gunLevel = minMaxInt.ClampInside(challenge2.gunLevel);
				challenge2.skin = Mathf.Clamp(challenge2.skin, 0, 2);
				challenge2.boosterLevel = Mathf.Clamp(challenge2.boosterLevel, 0, 14);
				challenge2.rating = Mathf.Clamp(challenge2.rating, 0, variables.GetMaxTrophies());
				challenge2.boosterCount = Mathf.Clamp(challenge2.boosterCount, 0, variables.GetBoosterCardCountMax());
				if (!variables.IsValidTankId(challenge2.tankId))
				{
					challenge2.tankId = "tank0";
				}
			}
			float x = backBlockerContainer.transform.position.x;
			float x2 = frontBlockerContainer.transform.position.x;
			float num6 = 0.25f;
			int num7 = Mathf.RoundToInt(Mathf.Abs(x - x2) / num6);
			aiSamplePoints = new Vector2[num7];
			for (int num8 = 0; num8 < num7; num8++)
			{
				float num9 = Mathf.Lerp(x2, x, (float)num8 / (float)num7);
				aiSamplePoints[num8] = new Vector2(num9, GetGroundHeight(num9));
			}
			aiSampleDerivatives = new float[num7 - 1];
			Vector2[] array7 = new Vector2[num7 - 1];
			for (int num10 = 1; num10 < num7; num10++)
			{
				Vector2 vector = aiSamplePoints[num10] - aiSamplePoints[num10 - 1];
				float num11 = vector.y / vector.x;
				aiSampleDerivatives[num10 - 1] = num11;
				array7[num10 - 1] = aiSamplePoints[num10 - 1];
			}
			aiSamplePointMaxima = new List<Vector2>();
			aiSamplePointMinima = new List<Vector2>();
			aiSamplePointMaximaDerivatives = new List<Vector3>();
			aiSamplePointMinimaDerivatives = new List<Vector3>();
			for (int num12 = 1; num12 < num7 - 1; num12++)
			{
				if (aiSamplePoints[num12].y > aiSamplePoints[num12 - 1].y && aiSamplePoints[num12].y > aiSamplePoints[num12 + 1].y)
				{
					aiSamplePointMaxima.Add(aiSamplePoints[num12]);
				}
			}
			for (int num13 = 1; num13 < num7 - 1; num13++)
			{
				if (aiSamplePoints[num13].y < aiSamplePoints[num13 - 1].y && aiSamplePoints[num13].y < aiSamplePoints[num13 + 1].y)
				{
					aiSamplePointMinima.Add(aiSamplePoints[num13]);
				}
			}
			for (int num14 = 1; num14 < aiSampleDerivatives.Length - 1; num14++)
			{
				if (aiSampleDerivatives[num14] > aiSampleDerivatives[num14 - 1] && aiSampleDerivatives[num14] > aiSampleDerivatives[num14 + 1])
				{
					Vector3 item = array7[num14];
					item.z = aiSampleDerivatives[num14];
					aiSamplePointMaximaDerivatives.Add(item);
				}
			}
			for (int num15 = 1; num15 < aiSampleDerivatives.Length - 1; num15++)
			{
				if (aiSampleDerivatives[num15] < aiSampleDerivatives[num15 - 1] && aiSampleDerivatives[num15] < aiSampleDerivatives[num15 + 1])
				{
					Vector3 item2 = array7[num15];
					item2.z = aiSampleDerivatives[num15];
					aiSamplePointMinimaDerivatives.Add(item2);
				}
			}
			aiController.Init();
			List<AIController.TankData> list2 = new List<AIController.TankData>();
			for (int num16 = 0; num16 != array.Length; num16++)
			{
				bool flag2 = flag || num16 > 0;
				Team team = flag2 ? Team.Right : Team.Left;
				Challenge challenge3 = array[num16];
				float num17 = flag2 ? (frontBlockerContainer.position.x - 10f - (float)(3 * num16)) : (backBlockerContainer.position.x + 7f + (float)(3 * num16));
				Vector3 spawnPos2 = new Vector3(num17, GetGroundHeight(num17) + theme.groundMesh.transform.position.y + 2f, -0.5f);
				int level2 = (challenge3.tankLevel <= 0) ? Mathf.Max(1, challenge3.gunLevel, challenge3.armorLevel, challenge3.engineLevel) : challenge3.tankLevel;
				Driver.Uniform uniform = challenge3.isVip ? Variables.instance.vipUniform : (flag2 ? Variables.instance.enemyUniform : Variables.instance.allyUniform);
				TankContainer tankContainer = SpawnPlayer(spawnPos2, variables.GetTank(challenge3.tankId), challenge3.skin, level2, uniform);
				tankContainer.Booster = PlayerDataManager.GetBooster(challenge3.boosterId, challenge3.boosterLevel, challenge3.boosterCount);
				tankContainer.SetTankBooster(tankContainer.Booster);
				if (flag2)
				{
					if (arenaTanksShouldCollide)
					{
						SetLayerRecursive(tankContainer.transform, LayerMask.NameToLayer("PlayerTankRight"));
						SetLayerRecursive(tankContainer.driverRagdoll, LayerMask.NameToLayer("TankDriver"));
					}
					if (flag)
					{
						tankContainer.healthBarBase.gameObject.SetActive(value: false);
						tankContainer.SetHealthBar(MenuController.GetMenu<GameMenu>().arenaPlayerHealthbar[1], MenuController.GetMenu<GameMenu>().arenaPlayerHealthbarBase[1], coloredHealth: false);
						OnPlayerHealth();
					}
					tankContainer.UpdateHitMask();
					FlipTank(tankContainer);
					arenaEnemies.Add(tankContainer);
					arenaEnemiesAliveCount++;
				}
				else
				{
					arenaAllies.Add(tankContainer);
					arenaAlliesAliveCount++;
				}
				list2.Add(new AIController.TankData(team, tankContainer, challenge3.rating));
			}
			AIController.TankData playerTankData = new AIController.TankData(Team.Left, playerTankContainer, PlayerDataManager.GetRating(PlayerDataManager.SelectedGameMode));
			playerTankContainer.AddHealthLossTrigger(new Vehicle.TriggerAtHealth(delegate
			{
				ArenaPlayerDied(playerTankData);
				GameMenu menu2 = MenuController.GetMenu<GameMenu>();
				menu2.otherThanBossContainer.gameObject.SetActive(value: false);
				if (arenaAlliesAliveCount > 0)
				{
					menu2.arenaSurrenderButton.gameObject.SetActive(value: true);
					Delay(1f, delegate
					{
						cameraFollowTransform = aiController.GetClosestAlly(playerTankData).vehicle.transform;
					});
				}
				else if (arenaEnemiesAliveCount > 0)
				{
					Delay(1f, delegate
					{
						cameraFollowTransform = aiController.GetEnemyLeader(playerTankData).vehicle.transform;
					});
				}
			}, 0f));
			aiController.AddPlayers(playerTankData, list2);
			yield return OnArenaIntro(this);
			arenaIntroDone = true;
		}
		yield return OnGameBegin(this, PlayerDataManager.SelectedGameMode);
		if (!PlayerDataManager.IsSelectedGameModePvP)
		{
			if (PlayerDataManager.SelectedGameMode == GameMode.Adventure)
			{
				while (!BossKilled && Running)
				{
					if (playerTankContainer.CurrentHealth <= 0f)
					{
						Running = false;
					}
					if (boss != null && boss.vehicleContainer != null && boss.vehicleContainer.CurrentHealth <= 0f)
					{
						Running = false;
					}
					if (!Running)
					{
						yield return EndLevel();
					}
					else
					{
						yield return null;
					}
				}
			}
			else if (PlayerDataManager.SelectedGameMode == GameMode.Classic)
			{
				while (Running)
				{
					if (playerTankContainer.CurrentHealth <= 0f)
					{
						Running = false;
					}
					if (boss != null && boss.vehicleContainer != null && boss.vehicleContainer.CurrentHealth <= 0f)
					{
						Running = false;
					}
					if (!Running)
					{
						yield return EndLevel();
					}
					else
					{
						yield return null;
					}
				}
			}
			else
			{
				while (Running)
				{
					if (playerTankContainer.CurrentHealth <= 0f || (boss != null && boss.vehicleContainer != null && boss.vehicleContainer.CurrentHealth <= 0f))
					{
						Running = false;
					}
					if (!Running)
					{
						yield return EndLevel();
					}
					else
					{
						yield return null;
					}
				}
			}
			TankPrefs.Save();
		}
		else
		{
			if (!PlayerDataManager.IsSelectedGameModePvP)
			{
				yield break;
			}
			int playersTotal = 1 + arenaAllies.Count + arenaEnemies.Count;
			GameMode mode = PlayerDataManager.SelectedGameMode;
			float time = 0f;
			float arenaTime = 60f;
			Color timeTextColor = Color.white;
			int num5;
			for (int i = 0; i < 2; i = num5)
			{
				while (time < arenaTime && arenaAlliesAliveCount > 0 && arenaEnemiesAliveCount > 0 && !PlayerSurrendered)
				{
					MenuController.GetMenu<GameMenu>().arenaTimeText.color = timeTextColor;
					MenuController.GetMenu<GameMenu>().arenaTimeText.text = Mathf.RoundToInt(arenaTime - time).ToString();
					time += Time.fixedDeltaTime;
					yield return new WaitForFixedUpdate();
				}
				if (i == 0 && arenaAlliesAliveCount > 0 && arenaEnemiesAliveCount > 0 && !PlayerSurrendered)
				{
					time = arenaTime * 0.5f;
					TankProgression.Stats stats;
					foreach (TankContainer arenaAlly in arenaAllies)
					{
						stats = arenaAlly.Stats;
						stats.health = 10f;
						arenaAlly.Stats = stats;
						arenaAlly.CurrentHealth = 0.5f;
					}
					foreach (TankContainer arenaEnemy in arenaEnemies)
					{
						stats = arenaEnemy.Stats;
						stats.health = 10f;
						arenaEnemy.Stats = stats;
						arenaEnemy.CurrentHealth = 0.5f;
					}
					stats = playerTankContainer.Stats;
					stats.health = 10f;
					playerTankContainer.Stats = stats;
					playerTankContainer.CurrentHealth = 0.5f;
					MusicManager.SetToBossMusic();
					Time.timeScale = 0f;
					Transform suddenDeathText = MenuController.GetMenu<GameMenu>().suddenDeathText.transform;
					Vector3 origScale = suddenDeathText.localScale;
					Vector3 startScale = origScale * 10f;
					suddenDeathText.transform.localScale = startScale;
					suddenDeathText.gameObject.SetActive(value: true);
					float animTime = 0.7f;
					for (float t2 = 0f; t2 < animTime; t2 += Time.unscaledDeltaTime)
					{
						float t3 = LeanTween.easeOutExpo(0f, 1f, t2 / animTime);
						suddenDeathText.localScale = Vector3.Lerp(startScale, origScale, t3);
						yield return null;
					}
					for (float t2 = 0f; t2 < animTime; t2 += Time.unscaledDeltaTime)
					{
						float t4 = LeanTween.easeInExpo(0f, 1f, t2 / animTime);
						suddenDeathText.localScale = Vector3.Lerp(origScale, Vector3.zero, t4);
						yield return null;
					}
					suddenDeathText.transform.localScale = origScale;
					MenuController.GetMenu<GameMenu>().suddenDeathText.gameObject.SetActive(value: false);
					Time.timeScale = 1f;
					timeTextColor = Color.red;
				}
				num5 = i + 1;
			}
			Running = false;
			int num18 = 0;
			if (arenaEnemiesAliveCount == 0 && arenaAlliesAliveCount > 0)
			{
				num18 = 1;
			}
			if ((arenaAlliesAliveCount == 0 && arenaEnemiesAliveCount > 0) || PlayerSurrendered)
			{
				num18 = -1;
			}
			TankAnalytics.PvpMatchEnded(mode, num18);
			PlayerDataManager.PvpGamePlayed();
			int rating = PlayerDataManager.GetRating(mode);
			bool flag3 = false;
			switch (num18)
			{
			case 1:
			{
				int rating2 = PlayerDataManager.GetRating(mode);
				PlayerDataManager.AddWinRating(mode);
				MenuController.GetMenu<MainMenu>().AddStarsFromGameMode(PlayerDataManager.SelectedGameMode, 1);
				PlayerDataManager.AddToChestProgress(ChestProgressionType.Pvp, 1);
				PlayerDataManager.AdvanceWinStreak(mode);
				PlayerDataManager.AddWinStreakRating(mode);
				int rating3 = PlayerDataManager.GetRating(mode);
				if (rating2 < 100 && rating3 >= 100)
				{
					TankAnalytics.NewAttributionEvent(TankAnalytics.AttributionEvent.RatingReached, 100, (mode == GameMode.Arena) ? "1v1" : "2v2");
				}
				else if (rating2 < 1000 && rating3 >= 1000)
				{
					TankAnalytics.NewAttributionEvent(TankAnalytics.AttributionEvent.RatingReached, 1000, (mode == GameMode.Arena) ? "1v1" : "2v2");
				}
				break;
			}
			case -1:
				flag3 = (PlayerDataManager.GetWinStreak(mode) > 0);
				PlayerDataManager.AddLoseRating(mode);
				PlayerDataManager.ResetWinStreak(mode);
				break;
			default:
				flag3 = (PlayerDataManager.GetWinStreak(mode) > 0);
				PlayerDataManager.ResetWinStreak(mode);
				break;
			}
			int ratingDifference = PlayerDataManager.GetRating(mode) - rating;
			BackendManager.SendChallenge(new Challenge
			{
				name = TankPrefs.GetString("challengeName", Social.localUser.userName),
				tankId = playerTank.id,
				skin = PlayerDataManager.GetSelectedSkin(playerTank),
				tankLevel = PlayerDataManager.GetTankUpgradeLevel(playerTank),
				boosterId = playerTankContainer.Booster.id,
				boosterCount = playerTankContainer.Booster.Count,
				boosterLevel = playerTankContainer.Booster.Level,
				rating = PlayerDataManager.GetRating(mode),
				actualRating = PlayerDataManager.GetRating(mode),
				countryCode = PlayerDataManager.GetCountryCode(),
				isVip = PlayerDataManager.IsSubscribed()
			}, mode);
			switch (mode)
			{
			case GameMode.Arena:
			{
				GameMenu gameMenu = MenuController.ShowMenu<GameMenu>();
				List<GameEndMenu.ArenaGameOverStats> list4 = new List<GameEndMenu.ArenaGameOverStats>();
				for (int num21 = 0; num21 != playersTotal; num21++)
				{
					list4.Add(new GameEndMenu.ArenaGameOverStats
					{
						profileTexture = gameMenu.arenaPlayerHUDs[num21].profilePicture.texture,
						flagSprite = gameMenu.arenaPlayerHUDs[num21].flag.sprite,
						rankSprite = gameMenu.arenaPlayerHUDs[num21].rank.sprite,
						name = gameMenu.arenaPlayerHUDs[num21].nameText.text
					});
				}
				int winStreak2 = PlayerDataManager.GetWinStreak(mode);
				foreach (WinStreakAchievement winStreakAchievement in Variables.instance.GetWinStreakAchievements(mode, winStreak2))
				{
					PlatformManager.ReportAchievement(winStreakAchievement.achievement);
				}
				MenuController.HideMenu<GameMenu>();
				GameEndMenu gameEndMenu2 = MenuController.ShowMenu<GameEndMenu>();
				gameEndMenu2.Init(rating, ratingDifference, list4, winStreak2, Variables.instance.GetWinStreakBonus(winStreak2), flag3 || winStreak2 > 1);
				for (int num22 = 0; num22 < gameEndMenu2.arenaGameOverTexts.Length; num22++)
				{
					gameEndMenu2.arenaGameOverTexts[num22].gameObject.SetActive(num18 + 1 == num22);
				}
				break;
			}
			case GameMode.Arena2v2:
			{
				GameMenu menu = MenuController.GetMenu<GameMenu>();
				List<GameEndMenu.ArenaGameOverStats> list3 = new List<GameEndMenu.ArenaGameOverStats>();
				for (int num19 = 0; num19 != playersTotal; num19++)
				{
					list3.Add(new GameEndMenu.ArenaGameOverStats
					{
						profileTexture = menu.arenaMultiplayerHUDs[num19].profilePicture.texture,
						flagSprite = menu.arenaMultiplayerHUDs[num19].flag.sprite,
						rankSprite = menu.arenaMultiplayerHUDs[num19].rank.sprite,
						name = menu.arenaMultiplayerHUDs[num19].nameText.text,
						rating = menu.arenaMultiplayerHUDs[num19].ratingText.text
					});
				}
				int winStreak = PlayerDataManager.GetWinStreak(mode);
				foreach (WinStreakAchievement winStreakAchievement2 in Variables.instance.GetWinStreakAchievements(mode, winStreak))
				{
					PlatformManager.ReportAchievement(winStreakAchievement2.achievement);
				}
				MenuController.HideMenu<GameMenu>();
				GameEndMenu gameEndMenu = MenuController.ShowMenu<GameEndMenu>();
				gameEndMenu.Init(rating, ratingDifference, list3, winStreak, Variables.instance.GetWinStreakBonus(winStreak), flag3 || winStreak > 1);
				for (int num20 = 0; num20 < gameEndMenu.arena2v2GameOverTexts.Length; num20++)
				{
					gameEndMenu.arena2v2GameOverTexts[num20].gameObject.SetActive(num18 + 1 == num20);
				}
				break;
			}
			}
			TankPrefs.Save();
			RemovePlayerControllers();
			aiController.Disable();
		}
	}

	private IEnumerator PlayerInvincible(float seconds)
	{
		playerInvincible = true;
		yield return new WaitForSeconds(seconds);
		playerInvincible = false;
	}

	private IEnumerator GiveHealthToPlayer(TankContainer player, float health)
	{
		for (float time = 0f; time < 1f; time += Time.fixedDeltaTime)
		{
			player.CurrentHealth += health * Time.fixedDeltaTime;
			player.CurrentHealth = Mathf.Min(player.Stats.health, player.CurrentHealth);
			yield return new WaitForFixedUpdate();
		}
	}

	public static void DestroyTanks()
	{
		if (instance.playerTankContainer != null)
		{
			UnityEngine.Object.Destroy(instance.playerTankContainer.gameObject);
		}
		if (instance.arenaEnemies != null && instance.arenaEnemies[0] != null)
		{
			UnityEngine.Object.Destroy(instance.arenaEnemies[0].gameObject);
		}
		for (int i = 0; i < instance.enemies.Count; i++)
		{
			UnityEngine.Object.Destroy(instance.enemies[i].vehicleContainer.gameObject);
		}
		instance.enemies.Clear();
	}

	private IEnumerator FadeVehicleOut(Vehicle vehicle)
	{
		if (!(vehicle != null))
		{
			yield break;
		}
		if (!IsLowEndDevice)
		{
			yield return new WaitForSeconds(variables.enemyScrapDuration);
		}
		if (vehicle.blackSmoke != null)
		{
			vehicle.blackSmoke.Stop();
		}
		for (float t = 0f; t < variables.enemyScrapFadeOutTime; t += Time.deltaTime)
		{
			float t2 = t / variables.enemyScrapFadeOutTime;
			for (int i = 0; i < vehicle.spriteRenderers.Length; i++)
			{
				if (vehicle.spriteRenderers[i] != null && vehicle.spriteRenderers[i].GetComponent<Scrap>() == null)
				{
					vehicle.spriteRenderers[i].color = Color.Lerp(Color.white, new Color(1f, 1f, 1f, 0f), t2);
				}
			}
			for (int j = 0; j < vehicle.ragdollRenderers.Length; j++)
			{
				if (vehicle.ragdollRenderers[j] != null)
				{
					vehicle.ragdollRenderers[j].color = Color.Lerp(Color.white, new Color(1f, 1f, 1f, 0f), t2);
				}
			}
			yield return null;
		}
		if (vehicle != null)
		{
			ParticleSystem blackSmoke = vehicle.blackSmoke;
			if (blackSmoke != null)
			{
				blackSmoke.transform.SetParent(base.transform);
				blackSmoke.transform.localScale = Vector3.one;
				UnityEngine.Object.Destroy(blackSmoke.gameObject, blackSmoke.main.duration - variables.enemyScrapFadeOutTime);
			}
			if (vehicle.driverRagdoll != null)
			{
				UnityEngine.Object.Destroy(vehicle.driverRagdoll.gameObject);
			}
			UnityEngine.Object.Destroy(vehicle.gameObject);
		}
	}

	private void Update()
	{
		UpdateGroundHeightCache();
		for (int i = 0; i < enemies.Count; i++)
		{
			if (enemies[i].vehicleContainer == null)
			{
				enemyCountByType[enemies[i].typeIndex]--;
				enemies.Remove(enemies[i]);
				i--;
			}
			else
			{
				enemies[i].vehicleContainer.SetHealth();
			}
		}
		float add = (theme != null) ? (theme.groundMesh.transform.position.y - 0.33f) : 0f;
		Action<Vehicle> action = delegate(Vehicle v)
		{
			for (int n = 0; n < v.wheelColliders.Length; n++)
			{
				if (v.wheelColliders[n] != null)
				{
					CircleCollider2D circleCollider2D2 = v.wheelColliders[n];
					float offset = circleCollider2D2.transform.position.x - base.transform.position.x;
					float num4 = GetGroundHeight(offset) + add;
					if (circleCollider2D2.transform.position.y < num4)
					{
						Vector2 position3 = circleCollider2D2.attachedRigidbody.position;
						position3.y = num4 + circleCollider2D2.radius + 0.1f;
						circleCollider2D2.attachedRigidbody.position = position3;
					}
				}
			}
		};
		Action<Vehicle> action2 = delegate(Vehicle v)
		{
			for (int m = 0; m < v.wheelColliders.Length; m++)
			{
				if (v.wheelColliders[m] != null)
				{
					CircleCollider2D circleCollider2D = v.wheelColliders[m];
					float num3 = circleCollider2D.transform.position.x - base.transform.position.x;
					float x = backBlockerContainer.position.x;
					float x2 = frontBlockerContainer.position.x;
					if (num3 < x)
					{
						Vector2 position = circleCollider2D.attachedRigidbody.position;
						position.x = x + circleCollider2D.radius + 0.1f;
						circleCollider2D.attachedRigidbody.position = position;
					}
					else if (PlayerDataManager.IsSelectedGameModePvP && num3 > x2)
					{
						Vector2 position2 = circleCollider2D.attachedRigidbody.position;
						position2.x = x2 - (circleCollider2D.radius + 0.1f);
						circleCollider2D.attachedRigidbody.position = position2;
					}
				}
			}
		};
		if (PlayerDataManager.IsSelectedGameModePvP)
		{
			for (int j = 0; j != arenaAllies.Count; j++)
			{
				if (arenaAllies[j] != null)
				{
					action(arenaAllies[j]);
					action2(arenaAllies[j]);
				}
			}
			for (int k = 0; k != arenaEnemies.Count; k++)
			{
				if (arenaEnemies[k] != null)
				{
					action(arenaEnemies[k]);
					action2(arenaEnemies[k]);
				}
			}
		}
		if (playerTankContainer != null && playerTankContainer.CurrentHealth > 0f)
		{
			Physics2D.gravity = new Vector2(0f, variables.gravity);
			if (!playerTankContainer.Rigidbody.isKinematic)
			{
				playerTankContainer.Rigidbody.mass = playerTank.tankMass;
			}
			if (boss != null && boss.vehicleContainer != null)
			{
				boss.vehicleContainer.SetHealth();
			}
			UpdateMovement();
			playerTankContainer.SetHealth();
			action(playerTankContainer);
			action2(playerTankContainer);
		}
		else
		{
			RemovePlayerControllers();
		}
		while (destroyedVehicles != null && destroyedVehicles.Count > 0)
		{
			Vehicle vehicle = destroyedVehicles.Dequeue();
			if (vehicle != null)
			{
				StartCoroutine(FadeVehicleOut(vehicle));
			}
		}
		UpdateEnvironment();
		if (PlayerDataManager.IsSelectedGameModePvP)
		{
			UpdateArena();
		}
		else
		{
			UpdateEnemies();
		}
		if (!PlayerDataManager.IsSelectedGameModePvP)
		{
			if (boss != null && boss.vehicleContainer != null)
			{
				action(boss.vehicleContainer);
			}
			int bossKillCount = GetBossKillCount(stagesCleared);
			if (OnSetStageProgress != null)
			{
				OnSetStageProgress((float)enemiesKilled / (float)bossKillCount);
			}
		}
		if (playerTankContainer == null || playerTankContainer.CurrentHealth <= 0f)
		{
			return;
		}
		frameCountLast += 1f;
		frameTimefromLast += Time.unscaledDeltaTime;
		if (!(frameCountLast >= 30f))
		{
			return;
		}
		float item = frameCountLast / frameTimefromLast;
		frameTimefromLast = 0f;
		frameCountLast = 0f;
		frameTimeHistory.Enqueue(item);
		if (frameTimeHistory.Count <= 5)
		{
			return;
		}
		frameTimeHistory.Dequeue();
		float[] array = frameTimeHistory.ToArray();
		float num = 0f;
		float num2 = 100f;
		for (int l = 0; l != array.Length; l++)
		{
			num += array[l];
			if (array[l] < num2)
			{
				num2 = array[l];
			}
		}
		num /= (float)array.Length;
		IsLowEndDevice = (num < 27f);
	}

	private void FixedUpdate()
	{
		UpdateBullets();
		if ((bool)playerTankContainer && !playerTankContainer.Rigidbody.isKinematic)
		{
			UpdateCamera();
		}
	}

	private void LateUpdate()
	{
		if ((bool)playerTankContainer && playerTankContainer.Rigidbody.isKinematic)
		{
			UpdateCamera();
		}
	}

	private void UpdateCamera()
	{
		if (PlayerDataManager.IsSelectedGameModePvP && !arenaIntroDone)
		{
			return;
		}
		Vector2 vector = default(Vector2);
		if (cameraFollowTransform != null)
		{
			float num = 0f;
			float num2 = 0f;
			if (playerTankContainer != null && (cameraFollowTransform == playerTankContainer.transform || cameraFollowTransform == playerTankContainer.cameraFollowTransform))
			{
				num = variables.cameraOffset;
				num2 = 2f;
			}
			vector.x = Mathf.Lerp(cameraContainer.position.x, cameraFollowTransform.position.x + num, variables.cameraSpring);
			vector.y = Mathf.Lerp(cameraContainer.position.y, cameraFollowTransform.position.y + num2, variables.cameraSpring);
		}
		vector += cameraOffset;
		vector.x += cameraShake.x * (Mathf.PerlinNoise(Time.time * variables.cameraShakeFrequency, 0f) - 0.5f);
		vector.y += cameraShake.y * (Mathf.PerlinNoise(0f, Time.time * variables.cameraShakeFrequency) - 0.5f);
		if ((bool)cameraFollowTransform)
		{
			cameraContainer.position = new Vector3(vector.x, vector.y, cameraContainer.position.z);
		}
		else
		{
			cameraContainer.position += new Vector3(vector.x, vector.y, 0f);
		}
		cameraShake = Vector2.Lerp(cameraShake, Vector2.zero, variables.cameraShakeDampening);
		cameraOffset = Vector2.Lerp(cameraOffset, Vector2.zero, variables.cameraShakeDampening);
	}

	private void AddScore(Vehicle vehicle)
	{
		if (vehicle.Enemy == null)
		{
			return;
		}
		if (PlayerDataManager.SelectedGameMode == GameMode.Adventure || PlayerDataManager.SelectedGameMode == GameMode.Classic)
		{
			int num = 1;
			if (PlayerDataManager.SelectedGameMode == GameMode.Classic)
			{
				num = Variables.instance.levels[PlayerDataManager.GetSelectedLevel()].coinsPerScore;
			}
			int num2 = vehicle.Enemy.settings.scoreReward * num;
			MenuController.GetMenu<GameMenu>().AnimateGameCoins(num2, vehicle.transform);
			AddCoins(num2, sync: false);
			int num3 = vehicle.Enemy.settings.scoreReward * ((PlayerDataManager.SelectedGameMode == GameMode.Classic) ? 1 : (stagesCleared + 1));
			score += num3;
			if (OnScoreAdded != null)
			{
				OnScoreAdded(num3);
			}
			PlayerDataManager.AddXP(num3);
		}
		else if (PlayerDataManager.SelectedGameMode == GameMode.BossRush)
		{
			int num4 = variables.bossRushStageReward * ++stagesCleared;
			int num5 = vehicle.Enemy.settings.scoreReward * stagesCleared;
			MenuController.GetMenu<GameMenu>().AnimateGameCoins(num4, vehicle.transform);
			AddCoins(num4, sync: false);
			score += num5;
			if (OnScoreAdded != null)
			{
				OnScoreAdded(num5);
			}
			PlayerDataManager.AddXP(num5);
		}
		TankPrefs.SaveAtEndOfFrame();
	}

	private void DestroyVehicle(Vehicle vehicle, float forceFactor, bool fadeAway = true)
	{
		AddScore(vehicle);
		vehicle.PreDestroy();
		if (vehicle.BulletRoutine != null)
		{
			vehicle.StopCoroutine(vehicle.BulletRoutine);
		}
		if (vehicle.FlameRoutine != null)
		{
			vehicle.StopCoroutine(vehicle.FlameRoutine);
		}
		if ((bool)vehicle.audioSourceAimSound)
		{
			vehicle.audioSourceAimSound.Stop();
		}
		vehicle.DisableAliveStuff();
		Transform torso = vehicle.torso;
		if ((bool)torso)
		{
			torso.gameObject.SetActive(value: false);
		}
		Transform driverRagdoll = vehicle.driverRagdoll;
		if ((bool)driverRagdoll)
		{
			if (vehicle.transform == playerTankContainer.transform)
			{
				cameraFollowTransform = driverRagdoll;
				driverRagdoll.gameObject.AddComponent<PlayOnCollision>().audioName = "bodyThud";
			}
			driverRagdoll.SetParent(base.transform);
			driverRagdoll.gameObject.SetActive(value: true);
			vehicle.ragdollRigidbody.AddForce(new Vector2(UnityEngine.Random.Range(-50, 50), 100f) * forceFactor, ForceMode2D.Impulse);
		}
		Array.ForEach(vehicle.topJoints, delegate(Joint2D j)
		{
			UnityEngine.Object.Destroy(j);
		});
		Array.ForEach(vehicle.wheelJoints, delegate(WheelJoint2D j)
		{
			UnityEngine.Object.Destroy(j);
		});
		Array.ForEach(vehicle.rigidbodies, delegate(Rigidbody2D v)
		{
			if ((bool)v && v.simulated)
			{
				Vector2 a = new Vector2((UnityEngine.Random.value - 0.7f) * variables.tankDeathForce.x, (UnityEngine.Random.value + 0.5f) / 2f * variables.tankDeathForce.y);
				float num = UnityEngine.Random.value * variables.tankDeathForceOffset;
				float f = UnityEngine.Random.value * (float)Math.PI * 2f;
				Vector2 b = new Vector2(num * Mathf.Cos(f), num * Mathf.Sin(f));
				v.AddForceAtPosition(a * forceFactor, new Vector2(v.transform.position.x, v.transform.position.y) + b);
			}
		});
		PlayOnCollision[] playOnCollisions = vehicle.playOnCollisions;
		foreach (PlayOnCollision playOnCollision in playOnCollisions)
		{
			if ((bool)playOnCollision)
			{
				playOnCollision.enabled = true;
			}
		}
		Collider2D[] collider2Ds = vehicle.collider2Ds;
		foreach (Collider2D collider2D in collider2Ds)
		{
			if ((bool)collider2D)
			{
				collider2D.gameObject.layer = LayerMask.NameToLayer("Scrap");
			}
		}
		SpriteRenderer[] spriteRenderers = vehicle.spriteRenderers;
		foreach (SpriteRenderer spriteRenderer in spriteRenderers)
		{
			if ((bool)spriteRenderer)
			{
				spriteRenderer.sortingLayerName = "Scrap";
			}
		}
		if (vehicle.healthBarBase != null)
		{
			vehicle.healthBarBase.gameObject.SetActive(value: false);
		}
		if (enemies.Count <= 5 && (bool)vehicle.blackSmoke)
		{
			vehicle.blackSmoke.Play();
		}
		Vector3 position = vehicle.transform.position;
		AudioMap.PlayClipAt(audioMap, "tankDestroyed", vehicle.transform.position, audioMap.effectsMixerGroup);
		AudioMap.PlayClipAt(audioMap, "tankExplodes", vehicle.transform.position, audioMap.effectsMixerGroup);
		if ((bool)vehicle.explosionParticles)
		{
			vehicle.explosionParticles.Play();
		}
		Delay(0.3f, delegate
		{
			if (vehicle != null)
			{
				AudioMap.PlayClipAt(audioMap, vehicle.deathSound, vehicle.transform.position, audioMap.effectsMixerGroup);
			}
		});
		AudioSource componentInChildren = vehicle.GetComponentInChildren<AudioSource>();
		if ((bool)componentInChildren)
		{
			AudioMap.PlayClip(audioMap.tankBurning[0], componentInChildren);
		}
		for (int k = 0; k < vehicle.chains.Length; k++)
		{
			if (vehicle.chains[k] != null)
			{
				UnityEngine.Object.Destroy(vehicle.chains[k].gameObject);
			}
		}
		ScrapMode scrapMode = (currentScrapCount >= 20) ? ((currentScrapCount < 40) ? ScrapMode.FadeOnCollision : ScrapMode.FadeImmediate) : ScrapMode.Normal;
		if (IsLowEndDevice)
		{
			scrapMode = ScrapMode.FadeImmediate;
		}
		vehicle.DetachScraps(vehicle.transform, forceFactor, scrapMode);
		CircleSaw componentInChildren2 = vehicle.GetComponentInChildren<CircleSaw>();
		if (componentInChildren2 != null)
		{
			UnityEngine.Object.Destroy(componentInChildren2.gameObject);
		}
		if (fadeAway)
		{
			destroyedVehicles.Enqueue(vehicle);
		}
	}

	public void AddScrap(int count)
	{
		lock (scrapCountLock)
		{
			currentScrapCount += count;
		}
	}

	public void Delay(float t, Action action)
	{
		StartCoroutine(DelayRoutine(t, action));
	}

	private IEnumerator DelayRoutine(float t, Action action)
	{
		yield return new WaitForSeconds(t);
		action();
	}

	public void SetArenaPlayerCollision(Vehicle tank, bool value)
	{
		if (!(tank == null))
		{
			SetLayerRecursive(tank.transform, value ? LayerMask.NameToLayer("PlayerTankRight") : LayerMask.NameToLayer("EnemyTankPassable"));
			if (tank.Booster.type == BoosterGameplayType.CircleSaw)
			{
				Transform circleSawContainer = tank.circleSawContainer;
				SetLayerRecursive(circleSawContainer, LayerMask.NameToLayer("Default"));
				circleSawContainer.gameObject.layer = LayerMask.NameToLayer("ShockwaveRight");
			}
		}
	}

	public void ArenaPlayerDied(AIController.TankData tank)
	{
		lock (deathLock)
		{
			if (tank.team == Team.Left)
			{
				arenaAlliesAliveCount--;
			}
			else
			{
				arenaEnemiesAliveCount--;
			}
			tank.Disable();
			aiController.ChargeAll();
			SetLayerRecursive(tank.vehicle.transform, LayerMask.NameToLayer("Scrap"));
			DestroyVehicle(tank.vehicle, variables.EvaluateDeathForceFactor(tank.vehicle.CurrentHealth), fadeAway: false);
		}
	}

	private void UpdateArena()
	{
		if (arenaIntroDone && Running && !(playerTankContainer == null))
		{
			aiController.Update(Time.deltaTime);
		}
	}

	private int GetBossKillCount(int stage)
	{
		if (stage < 0)
		{
			return 0;
		}
		if (PlayerDataManager.SelectedGameMode != GameMode.Classic)
		{
			return Mathf.Min(5 + 2 * stage, 35);
		}
		return 5 + 5 * ((stage + 1) * (stage + 1));
	}

	private void SetNextBossKillCount()
	{
		if (PlayerDataManager.SelectedGameMode == GameMode.BossRush)
		{
			nextBossKillCount = 0;
		}
		else
		{
			nextBossKillCount = GetBossKillCount(stagesCleared);
		}
	}

	public static void GivePlayerHealth(TankContainer player, float percentage)
	{
		if (!(player == null))
		{
			float health = percentage * player.Stats.health;
			instance.StartCoroutine(instance.GiveHealthToPlayer(player, health));
			AudioMap.PlayClipAt(instance.audioMap, "revive", player.transform.position, instance.audioMap.effectsMixerGroup);
			ParticleSystem component = UnityEngine.Object.Instantiate(instance.sparkleParticlePrefab, player.transform).GetComponent<ParticleSystem>();
			component.transform.localPosition = component.transform.localPosition + new Vector3(0f, -2f, 0f);
			component.Play();
			UnityEngine.Object.Destroy(component.gameObject, component.main.duration);
		}
	}

	private void UpdateEnemies()
	{
		for (int num = enemies.Count - 1; num >= 0; num--)
		{
			if (enemies[num].vehicleContainer.CurrentHealth <= 0f)
			{
				DestroyVehicle(enemies[num].vehicleContainer, variables.EvaluateDeathForceFactor(enemies[num].vehicleContainer.CurrentHealth));
				enemies[num].vehicleContainer.GetComponentInChildren<MonoBehaviour>().StopAllCoroutines();
				enemiesKilled++;
				enemyCountByType[enemies[num].typeIndex]--;
				enemies.RemoveAt(num);
			}
		}
		if (!Running || playerTankContainer == null)
		{
			return;
		}
		if (enemiesKilled >= nextBossKillCount && boss == null)
		{
			if (enemies.Count <= 0)
			{
				boss = new Enemy();
				MenuController.GetMenu<GameMenu>().otherThanBossContainer.SetActive(value: false);
				playerTankContainer.SetSpeed(0f, 0f);
				playerTankContainer.Shoot(val: false);
				Delay(1.5f, delegate
				{
					float num4 = playerTankContainer.transform.localPosition.x + 30f;
					int typeIndex = 0;
					if (PlayerDataManager.SelectedGameMode != GameMode.BossRush)
					{
						boss.settings = variables.GetRandomBoss(lastBoss, out typeIndex);
						MusicManager.SetToBossMusic();
					}
					else
					{
						if (bossRushCounter >= variables.bossSpawnOrder.Length)
						{
							bossRushCounter = 0;
						}
						if (!bossRushStarted)
						{
							AddPlayerController(MenuController.GetMenu<GameMenu>().touchScreenController);
							bossRushStarted = true;
							PlayerDataManager.SetNextBossRushFree(value: false);
							MusicManager.SetToBossMusic();
						}
						stageDifficultyMultiplier += variables.bossRushDifficultyIncrease;
						typeIndex = variables.bossSpawnOrder[bossRushCounter];
						boss.settings = variables.enemies[typeIndex];
						bossRushCounter++;
					}
					boss.typeIndex = typeIndex;
					lastBoss = typeIndex;
					enemyCountByType[boss.typeIndex]++;
					boss.vehicleContainer = UnityEngine.Object.Instantiate(boss.settings.prefab, base.transform).GetComponent<Vehicle>();
					boss.vehicleContainer.SetHat(variables.levels[SelectedLevel].hat);
					boss.vehicleContainer.BulletDef = boss.settings.bullet;
					boss.vehicleContainer.BulletTypeIndex = typeIndex + variables.tanks.Length;
					boss.vehicleContainer.Enemy = boss;
					boss.vehicleContainer.SetDustColors(variables.dustColors);
					boss.vehicleContainer.transform.localPosition = new Vector3(num4, GetGroundHeight(num4, approximate: false) + theme.groundMesh.transform.localPosition.y + 2f, 0f);
					TankProgression.Stats stats = boss.settings.stats;
					stats.health *= stageDifficultyMultiplier;
					stats.damage *= stageDifficultyMultiplier;
					boss.vehicleContainer.Stats = stats;
					boss.vehicleContainer.LogicRoutine = boss.vehicleContainer.StartCoroutine(EnemyLogic(boss));
					StartCoroutine(OnBossIntro(this, boss));
				});
			}
		}
		else
		{
			if (!(playerTankContainer.transform.localPosition.x > nextSpawnPosition - variables.spawnRadius))
			{
				return;
			}
			nextSpawnPosition = variables.GetRandomEnemySpawnOffset(nextSpawnPosition);
			float num2 = nextSpawnPosition;
			spawnCount++;
			int randomGroupSize = variables.GetRandomGroupSize(num2, nextBossKillCount - (enemiesKilled + enemies.Count));
			if (!JustSpawnedCoins && UnityEngine.Random.value < variables.coinsInsteadOfEnemiesChance)
			{
				JustSpawnedCoins = true;
				for (int i = 0; i < variables.coinGroupSize; i++)
				{
					GameObject gameObject = UnityEngine.Object.Instantiate(coinPrefab, coinParent.transform);
					float num3 = num2 + (float)i * variables.coinExtraSpacing;
					gameObject.transform.localPosition = new Vector3(num3, GetGroundHeight(num3) + theme.groundMesh.transform.localPosition.y + 0.5f, 0f);
				}
			}
			else
			{
				StartCoroutine(SpawnEnemies(randomGroupSize, num2));
			}
		}
	}

	private void ExitLevel(bool bossDefeated)
	{
		playEndTime = Time.time;
		TankAnalytics.ReportGameSessionLength(playEndTime - playStartTime);
		if (playerTankContainer.CurrentHealth <= 0f)
		{
			TankAnalytics.PlayerGotKilled(playerTankContainer.LastDamageDealer ? playerTankContainer.LastDamageDealer.gameObject.name : "", playerTank.name, variables.levels[SelectedLevel].name, stagesCleared, score, coinsGotten);
			AudioMap.PlayClipAt(audioMap, "ownTankDestroyed", ingameCameras[0].transform.position, audioMap.effectsMixerGroup);
		}
		int oldXp = PlayerDataManager.GetXP() - score;
		MenuController.HideMenu<GameMenu>();
		int lid = (PlayerDataManager.SelectedGameMode == GameMode.Adventure) ? 6 : ((PlayerDataManager.SelectedGameMode == GameMode.Classic) ? PlayerDataManager.GetSelectedLevel() : 5);
		int leaderboardScore = PlayerDataManager.GetLeaderboardScore((LeaderboardID)lid);
		PlayerDataManager.SetLeaderboardScore((LeaderboardID)lid, score);
		MenuController.ShowMenu<GameEndMenu>().Init(leaderboardScore, oldXp, score, coinsGotten, bossDefeated);
	}

	private IEnumerator EndLevel()
	{
		Running = false;
		if (playerTankContainer.CurrentHealth <= 0f && playerTankContainer != null)
		{
			RemovePlayerControllers();
			Vector3 newSpawnPos = playerTankContainer.transform.position + Vector3.right * 5f;
			DestroyVehicle(playerTankContainer, variables.EvaluateDeathForceFactor(playerTankContainer.CurrentHealth), fadeAway: false);
			if (OnGameEnd != null)
			{
				yield return OnGameEnd(this, PlayerDataManager.SelectedGameMode);
			}
			if (ReviveUsed && !AllLivesUsed)
			{
				Running = true;
				AllLivesUsed = true;
				if (boss != null)
				{
					boss.vehicleContainer.transform.position += new Vector3(40f, 15f);
				}
				for (int i = 0; i < enemies.Count; i++)
				{
					if ((bool)enemies[i].vehicleContainer)
					{
						UnityEngine.Object.Destroy(enemies[i].vehicleContainer.gameObject);
					}
				}
				enemies.Clear();
				UnityEngine.Object.Destroy(playerTankContainer.gameObject);
				UnityEngine.Object.Instantiate(sparkleParticlePrefab, new Vector3(0f, -2f, 0f), Quaternion.identity, playerTankContainer.transform);
				Vector3 spawnPos = newSpawnPos + new Vector3(0f, 2f, 0f) + playerTank.prefab.transform.position;
				playerTankContainer = SpawnLocalPlayer(spawnPos);
				SetPlayerBooster();
				playerTankContainer.BlinkHealth = true;
				playerTankContainer.CurrentHealth = 0.01f * playerTankContainer.Stats.health * variables.reviveHealthRestore;
				StartCoroutine(GiveHealthToPlayer(playerTankContainer, playerTankContainer.Stats.health * variables.reviveHealthRestore - playerTankContainer.CurrentHealth));
				StartCoroutine(PlayerInvincible(1f));
				AudioMap.PlayClipAt(audioMap, "revive", playerTankContainer.transform.position, audioMap.effectsMixerGroup);
				AudioMap.PlayClipAt(audioMap, "levelStart", playerTankContainer.transform.position, audioMap.effectsMixerGroup);
				ParticleSystem component = UnityEngine.Object.Instantiate(sparkleParticlePrefab, playerTankContainer.transform).GetComponent<ParticleSystem>();
				component.transform.localPosition = component.transform.localPosition + new Vector3(0f, -2f, 0f);
				component.Play();
				UnityEngine.Object.Destroy(component.gameObject, component.main.duration);
				cameraFollowTransform = (playerTankContainer.cameraFollowTransform ? playerTankContainer.cameraFollowTransform : playerTankContainer.transform);
			}
			else
			{
				TankAnalytics.ReviveDeclined();
				ExitLevel(bossDefeated: false);
			}
		}
		else
		{
			if (boss == null || !(boss.vehicleContainer.CurrentHealth <= 0f))
			{
				yield break;
			}
			if (PlayerDataManager.SelectedGameMode != GameMode.BossRush)
			{
				BossKilled = true;
				int num = boss.settings.scoreReward * (stagesCleared + 1);
				score += num;
				if (OnScoreAdded != null)
				{
					OnScoreAdded(num);
				}
				stagesCleared++;
			}
			if (PlayerDataManager.SelectedGameMode == GameMode.Classic)
			{
				StartCoroutine(MenuController.GetMenu<GameMenu>().ToggleStageClearedText());
			}
			cameraFollowTransform = boss.vehicleContainer.transform;
			if (boss.vehicleContainer.LogicRoutine != null)
			{
				boss.vehicleContainer.StopCoroutine(boss.vehicleContainer.LogicRoutine);
			}
			PlatformManager.ReportAchievement(boss.settings.achievement);
			boss.vehicleContainer.SetHealthBar(null, null, coloredHealth: false);
			DestroyVehicle(boss.vehicleContainer, variables.EvaluateDeathForceFactor(boss.vehicleContainer.CurrentHealth));
			boss.vehicleContainer.GetComponentInChildren<MonoBehaviour>().StopAllCoroutines();
			enemyCountByType[boss.typeIndex]--;
			if (PlayerDataManager.SelectedGameMode == GameMode.Classic)
			{
				UnityEngine.Object.Instantiate(bossHealthPack, boss.vehicleContainer.transform.position, Quaternion.identity);
			}
			boss = null;
			MenuController.GetMenu<GameMenu>().bossContainer.SetActive(value: false);
			if (PlayerDataManager.SelectedGameMode == GameMode.BossRush)
			{
				MenuController.GetMenu<GameMenu>().otherThanBossContainer.SetActive(value: false);
			}
			else
			{
				RemovePlayerControllers();
			}
			yield return new WaitForSeconds(1.5f);
			if (PlayerDataManager.SelectedGameMode == GameMode.Adventure)
			{
				if (OnGameEnd != null)
				{
					yield return OnGameEnd(this, PlayerDataManager.SelectedGameMode);
				}
				if (SelectedLevel + 1 >= Variables.instance.levels.Count)
				{
					PlayerDataManager.SetSelectedLevel(0, PlayerDataManager.SelectedGameMode);
					TankAnalytics.StageCleared(variables.levels[SelectedLevel].name, PlayerDataManager.GetCurrentStage(), score, playerTank.name);
					PlayerDataManager.SetCurrentStage(PlayerDataManager.GetCurrentStage() + 1);
				}
				else
				{
					PlayerDataManager.SetSelectedLevel(SelectedLevel + 1, PlayerDataManager.SelectedGameMode);
				}
				MenuController.GetMenu<MainMenu>().AddStarsFromGameMode(GameMode.Adventure, 1);
				PlayerDataManager.AddToChestProgress(ChestProgressionType.Adventure, 1);
				ExitLevel(bossDefeated: true);
			}
			else if (PlayerDataManager.SelectedGameMode == GameMode.Classic)
			{
				GameMenu gameMenu = MenuController.GetMenu<GameMenu>();
				gameMenu.otherThanBossContainer.SetActive(value: false);
				enemiesKilled = 0;
				SetNextBossKillCount();
				Running = true;
				cameraFollowTransform = (playerTankContainer.cameraFollowTransform ? playerTankContainer.cameraFollowTransform : playerTankContainer.transform);
				MusicManager.SetToBossEndMusic();
				yield return new WaitForSeconds(2f);
				AddPlayerController(gameMenu.touchScreenController);
				yield return OnGameBegin(this, PlayerDataManager.SelectedGameMode);
				StartCoroutine(gameMenu.ToggleStageClearedText());
			}
			else
			{
				MenuController.GetMenu<GameMenu>().otherThanBossContainer.SetActive(value: false);
				BossKilled = false;
				Running = true;
				Coroutine musicRoutine = MusicManager.StartBossEndMusicRoutine();
				yield return new WaitForSeconds(2f);
				StopCoroutine(musicRoutine);
				MusicManager.SetToBossMusic();
				MenuController.GetMenu<GameMenu>().otherThanBossContainer.SetActive(value: true);
			}
		}
	}

	private IEnumerator SpawnEnemies(int enemyCount, float pos)
	{
		int num2;
		for (int i = 0; i < enemyCount; i = num2)
		{
			JustSpawnedCoins = false;
			Enemy enemy = new Enemy();
			enemy.settings = variables.GetRandomEnemy(enemiesSpawned, ref enemyCountByType, out enemy.typeIndex);
			enemyCountByType[enemy.typeIndex]++;
			enemy.vehicleContainer = UnityEngine.Object.Instantiate(enemy.settings.prefab, base.transform).GetComponent<Vehicle>();
			enemy.vehicleContainer.SetHat(variables.levels[SelectedLevel].hat);
			enemy.vehicleContainer.BulletDef = enemy.settings.bullet;
			enemy.vehicleContainer.BulletTypeIndex = variables.tanks.Length + enemy.typeIndex;
			enemy.vehicleContainer.Enemy = enemy;
			enemy.vehicleContainer.SetDustColors(variables.dustColors);
			float num = pos + (float)i * variables.extraTankSpacing;
			enemy.vehicleContainer.transform.localPosition = new Vector3(num, GetGroundHeight(num) + theme.groundMesh.transform.localPosition.y + 2f, 0f);
			TankProgression.Stats stats = enemy.settings.stats;
			stats.damage *= stageDifficultyMultiplier;
			stats.health *= stageDifficultyMultiplier;
			enemy.vehicleContainer.Stats = stats;
			enemy.vehicleContainer.GetComponentInChildren<MonoBehaviour>().StartCoroutine(EnemyLogic(enemy, i % 4));
			enemies.Add(enemy);
			enemiesSpawned++;
			if (PlayerDataManager.SelectedGameMode == GameMode.Classic)
			{
				stageDifficultyMultiplier = 1f + (float)enemiesSpawned * Variables.instance.classicPerKillDifficultyAddition;
			}
			yield return null;
			num2 = i + 1;
		}
	}

	private IEnumerator EnemyLogic(Enemy enemy, int group = 0)
	{
		while (true)
		{
			yield return null;
			while (playerTankContainer == null || playerTankContainer.CurrentHealth <= 0f)
			{
				yield return null;
			}
			float acceleration = enemy.vehicleContainer.Stats.acceleration;
			float movement = 1f;
			if (enemy.settings.distanceTarget > 0f)
			{
				float num = enemy.vehicleContainer.transform.localPosition.x - playerTankContainer.transform.localPosition.x;
				if (playerTankContainer.transform.localPosition.x > enemy.vehicleContainer.transform.localPosition.x)
				{
					movement = ((!enemy.settings.getBackToPlayerAfterPassing) ? 1 : (-1));
				}
				else if (num < enemy.settings.distanceTarget - 1f)
				{
					movement = -1f;
				}
				else if (num < enemy.settings.distanceTarget + 1f)
				{
					movement = ((enemy.settings.speedWhileAttacking > 0f) ? (-1) : 0);
				}
			}
			enemy.vehicleContainer.SetSpeed(acceleration * movement, movement);
			if (IsLowEndDevice && Time.frameCount % 4 != group)
			{
				continue;
			}
			if (enemy.settings.hasToBeSeenBeforeAttacks && !enemy.hasBeenInView)
			{
				if (Mathf.Abs(playerTankContainer.transform.localPosition.x - enemy.vehicleContainer.transform.localPosition.x) < variables.screenWidth + variables.cameraOffset)
				{
					enemy.hasBeenInView = true;
				}
				yield return null;
				continue;
			}
			yield return new WaitForSeconds(enemy.GetMoveDuration());
			if (enemy.settings.bullet.type == BulletType.Cannon || enemy.settings.bullet.type == BulletType.Grenade)
			{
				if (!(Vector2.Distance(enemy.vehicleContainer.transform.position, playerTankContainer.transform.position) < enemy.settings.maxAimDistance))
				{
					continue;
				}
				enemy.vehicleContainer.SetSpeed(movement * enemy.settings.speedWhileAttacking, movement * Mathf.Min(1f, enemy.settings.speedWhileAttacking));
				if ((bool)enemy.vehicleContainer.audioSourceAimSound)
				{
					enemy.vehicleContainer.audioSourceAimSound.Play();
				}
				float closestDistance = float.PositiveInfinity;
				Transform turretPivot = enemy.vehicleContainer.turret;
				Quaternion startAngle = turretPivot.rotation;
				Quaternion a = enemy.vehicleContainer.transform.rotation * Quaternion.Euler(-Vector3.forward * enemy.settings.minAngle);
				Quaternion quaternion = enemy.vehicleContainer.transform.rotation * Quaternion.Euler(-Vector3.forward * enemy.settings.maxAngle);
				Quaternion targetAngle = quaternion;
				for (int i = 0; i < 4; i++)
				{
					Quaternion identity = Quaternion.identity;
					Quaternion quaternion2 = Quaternion.Slerp(a, quaternion, (float)i / 4f);
					Vector3 direction = quaternion2 * -Vector3.right;
					float num2 = PredictTrajectory(enemy.settings.bullet, turretPivot.position, direction);
					if (Mathf.Abs(num2) < Mathf.Abs(closestDistance))
					{
						closestDistance = num2;
						targetAngle = quaternion2;
					}
				}
				if (Mathf.Abs(enemy.settings.maxAngle - enemy.settings.minAngle) > 0f)
				{
					float aimStartTime = Time.time;
					while (Time.time < aimStartTime + enemy.GetAimDuration() / 2f)
					{
						float t = (Time.time - aimStartTime) / (enemy.GetAimDuration() / 2f);
						turretPivot.rotation = Quaternion.Lerp(startAngle, targetAngle, t);
						yield return null;
						if (!turretPivot)
						{
							break;
						}
					}
					yield return new WaitForSeconds(enemy.GetAimDuration() / 2f);
				}
				if ((bool)enemy.vehicleContainer.audioSourceAimSound)
				{
					enemy.vehicleContainer.audioSourceAimSound.Stop();
				}
				if (Mathf.Abs(closestDistance) < enemy.settings.maxAimDistance)
				{
					float shootDuration = enemy.GetShootDuration();
					enemy.vehicleContainer.Shoot();
					yield return new WaitForSeconds(shootDuration);
					enemy.NextShot();
				}
			}
			else
			{
				if (enemy.vehicleContainer.BulletRoutine != null || !(playerTankContainer.CurrentHealth > 0f))
				{
					continue;
				}
				float num3 = enemy.vehicleContainer.transform.position.x - playerTankContainer.transform.position.x;
				if (num3 < enemy.settings.distanceTarget + 5f && num3 > enemy.settings.distanceTarget - 5f)
				{
					movement = Mathf.Sign(num3);
					enemy.vehicleContainer.SetSpeed(enemy.settings.speedWhileAttacking * movement, (enemy.settings.speedWhileAttacking == 0f) ? 0f : movement);
					if (enemy.vehicleContainer.BulletDef.shootStartAudioName.Length > 0)
					{
						AudioMap.PlayClipAt(audioMap, enemy.vehicleContainer.BulletDef.shootStartAudioName, enemy.vehicleContainer.transform.position, audioMap.effectsMixerGroup);
					}
					yield return new WaitForSeconds(enemy.GetAimDuration());
					enemy.vehicleContainer.Shoot();
					while (enemy.vehicleContainer.BulletRoutine != null)
					{
						yield return new WaitForEndOfFrame();
					}
				}
				else
				{
					yield return new WaitForEndOfFrame();
				}
			}
		}
	}

	public float PredictTrajectory(BulletDefinition bulletDef, Vector3 start, Vector3 direction, int mask = -1, Vehicle target = null)
	{
		target = (target ?? playerTankContainer);
		if (bulletDef.type == BulletType.Flame)
		{
			RaycastHit2D raycastHit2D = Physics2D.Raycast(start, direction, bulletDef.damageRange, mask);
			if ((bool)raycastHit2D.collider)
			{
				return raycastHit2D.point.x - target.transform.position.x;
			}
		}
		else if (bulletDef.type == BulletType.Small || bulletDef.type == BulletType.Lightning)
		{
			Vector3 vector = target.transform.position - start;
			float f = Vector3.Angle(vector, direction);
			float x = vector.x;
			if (Mathf.Abs(x) <= 12f && Mathf.Abs(f) < bulletDef.angleRandomization)
			{
				return x;
			}
		}
		else if (bulletDef.type == BulletType.Laser)
		{
			RaycastHit2D raycastHit2D2 = Physics2D.Raycast(start, direction, 20f, mask);
			if ((bool)raycastHit2D2.collider)
			{
				return raycastHit2D2.point.x - target.transform.position.x;
			}
			Vector2 input = direction;
			raycastHit2D2 = Physics2D.Raycast(start, input.Rotate(-5f), 20f, mask);
			if ((bool)raycastHit2D2.collider)
			{
				return raycastHit2D2.point.x - target.transform.position.x;
			}
		}
		else if (PlayerDataManager.IsSelectedGameModePvP)
		{
			for (int i = -1; i <= 1; i++)
			{
				Vector3 vector2 = start + Vector3.right * i;
				float num = bulletDef.bulletGravityScale * variables.gravity;
				float fixedDeltaTime = Time.fixedDeltaTime;
				Vector3 zero = Vector3.zero;
				Vector3 a = direction * bulletDef.speed;
				for (int j = 0; j < 50; j++)
				{
					zero = vector2 + a * fixedDeltaTime + 0.5f * new Vector3(0f, num, 0f) * fixedDeltaTime * fixedDeltaTime;
					a.y += num * fixedDeltaTime;
					vector2 = zero;
					RaycastHit2D raycastHit2D3 = Physics2D.Linecast(vector2, zero);
					float num2 = Mathf.Sign(target.Speed);
					if ((bool)raycastHit2D3.collider && ((float)i == num2 || num2 == 0f) && ((1 << raycastHit2D3.collider.gameObject.layer) & mask) != 0)
					{
						return raycastHit2D3.point.x - target.transform.position.x;
					}
				}
			}
		}
		else
		{
			Vector3 vector3 = start;
			float num3 = bulletDef.bulletGravityScale * variables.gravity;
			float fixedDeltaTime2 = Time.fixedDeltaTime;
			Vector3 zero2 = Vector3.zero;
			Vector3 a2 = direction * bulletDef.speed;
			for (int k = 0; k < 100; k++)
			{
				zero2 = vector3 + a2 * fixedDeltaTime2 + 0.5f * new Vector3(0f, num3, 0f) * fixedDeltaTime2 * fixedDeltaTime2;
				a2.y += num3 * fixedDeltaTime2;
				vector3 = zero2;
				RaycastHit2D raycastHit2D4 = Physics2D.Linecast(vector3, zero2);
				if ((bool)raycastHit2D4.collider && ((1 << raycastHit2D4.collider.gameObject.layer) & mask) != 0)
				{
					return raycastHit2D4.point.x - target.transform.position.x;
				}
			}
		}
		return float.MaxValue;
	}

	private void UpdateMovement()
	{
		float num = backBlockerContainer.localPosition.x;
		if (!PlayerDataManager.IsSelectedGameModePvP)
		{
			num = Mathf.Max(playerTankContainer.transform.localPosition.x - 50f, num);
		}
		float num2 = (theme != null) ? theme.groundMesh.transform.localPosition.y : 0f;
		float y = GetGroundHeight(num) + num2;
		backBlockerContainer.localPosition = new Vector3(num, y, 0f);
		if (!PlayerDataManager.IsSelectedGameModePvP)
		{
			for (int i = 1; i < 10; i++)
			{
				float num3 = num + (float)i * 0.5f;
				if (!(num3 > playerTankContainer.transform.localPosition.x - 35f))
				{
					float y2 = GetGroundHeight(num3) + num2;
					Vector2 normalized = (new Vector2(num3, y2) - new Vector2(num, y)).normalized;
					if (Vector2.Angle(new Vector2(1f, 0f), normalized) > 20f)
					{
						backBlockerContainer.localPosition = new Vector3(num3, y2, 0f);
					}
					continue;
				}
				break;
			}
		}
		else
		{
			Vector3 localPosition = frontBlockerContainer.localPosition;
			localPosition.y = GetGroundHeight(localPosition.x) + num2;
			frontBlockerContainer.localPosition = localPosition;
		}
	}

	private void SetUvOffset(MeshRenderer quad, Vector2 offset)
	{
		quad.material.SetTextureOffset("_MainTex", offset);
	}

	private void UpdateEnvironment()
	{
		if (theme == null)
		{
			return;
		}
		Vector2 vector = cameraContainer.localPosition;
		if (playerTankContainer != null && playerTankContainer.CurrentHealth > 0f)
		{
			vector = playerTankContainer.transform.localPosition;
		}
		if (!PlayerDataManager.IsSelectedGameModePvP)
		{
			vector.x += 30f;
		}
		if (backgrounds != null)
		{
			Vector3 vector2 = ingameCameras[0].transform.position - lastCameraPos;
			lastCameraPos = ingameCameras[0].transform.position;
			for (int i = 0; i < backgrounds.Length; i++)
			{
				Vector3 a = vector2;
				a.Scale(theme.layers[i].moveSpeedRelativeToCamera);
				for (int j = 0; j < backgrounds[i].Length; j++)
				{
					backgrounds[i][j].transform.position -= a + theme.layers[i].movePerFrame * Time.deltaTime;
				}
			}
			for (int k = 0; k < backgrounds.Length; k++)
			{
				if (currentBackgroundIndex == null)
				{
					break;
				}
				int num = currentBackgroundIndex[k];
				int num2 = (num + 1) % backgrounds[k].Length;
				int num3 = (backgrounds[k].Length + num - 1) % backgrounds[k].Length;
				if (backgrounds[k][num2].transform.position.x < backgroundCamera.transform.position.x)
				{
					Transform transform = backgrounds[k][num3].transform;
					transform.transform.position += Vector3.right * transform.GetComponent<ParallaxLayer>().bounds.size.x * backgrounds[k].Length;
					currentBackgroundIndex[k] = num2;
				}
				else if (backgrounds[k][num3].transform.position.x > backgroundCamera.transform.position.x)
				{
					Transform transform2 = backgrounds[k][num2].transform;
					transform2.transform.position += Vector3.left * transform2.GetComponent<ParallaxLayer>().bounds.size.x * backgrounds[k].Length;
					currentBackgroundIndex[k] = num3;
				}
			}
		}
		if (Mathf.Abs(vector.x - previousGroundMeshBuiltAtOffset) > groundGenerationSettings.groundWidth / (float)groundGenerationSettings.groundPrecision * (float)groundGenerationSettings.groundRefreshInterval)
		{
			UpdateGroundMesh(vector.x);
		}
	}

	private void CheckHatRagdoll(Vehicle owner, float damage)
	{
		int index = PlayerDataManager.IsSelectedGameModePvP ? PlayerDataManager.SelectedArenaLevel : SelectedLevel;
		if ((bool)owner.hat && damage > 10f && variables.levels[index].hat != HatType.Space)
		{
			owner.hat.simulated = true;
			owner.hat.AddForce(new Vector2(-1f, 1f), ForceMode2D.Impulse);
			owner.hat.AddTorque(0.1f, ForceMode2D.Impulse);
			owner.hat.transform.SetParent(null);
			owner.hat.gameObject.layer = LayerMask.NameToLayer("Scrap");
			owner.ragdollHat.gameObject.SetActive(value: false);
		}
	}

	private IEnumerator DoDamageRangeAnimation(GameObject damageRange, BulletDefinition bulletDef)
	{
		SpriteRenderer sprite = damageRange.GetComponentInChildren<SpriteRenderer>();
		float time = 0f;
		Color targetColor = Color.white;
		for (; time < variables.damageRangeSpriteTime; time += Time.deltaTime)
		{
			float time2 = time / variables.damageRangeSpriteTime;
			float num = variables.damageRangeSpriteGrowCurve.Evaluate(time2) * bulletDef.damageRange;
			sprite.transform.localScale = new Vector3(num, num, 1f);
			targetColor.a = variables.damageRangeSpriteAlphaCurve.Evaluate(time2);
			sprite.color = targetColor;
			yield return new WaitForEndOfFrame();
		}
		UnityEngine.Object.Destroy(damageRange);
	}

	private float DoDamage(Bullet bullet, Transform tank, bool fullHit)
	{
		float num = fullHit ? 0f : Mathf.Max(0f, Vector3.Distance(bullet.transform.position, tank.transform.position) - 1f);
		float num2 = 0f;
		if (bullet.definition.damageRange != 0f)
		{
			float value = variables.damageRangeCurve.Evaluate(num / bullet.definition.damageRange);
			num2 = bullet.damage * Mathf.Clamp(value, 0f, 1f);
		}
		else
		{
			num2 = bullet.damage;
		}
		num2 = variables.GetDamageAdjustedWithPosition(bullet.startX, tank.transform.position.x, num2);
		Vehicle component = tank.GetComponent<Vehicle>();
		if (bullet.owner == tank && tank == playerTankContainer.transform)
		{
			num2 *= variables.selfDamageScale;
		}
		else if (bullet.owner == tank && component.Enemy != null)
		{
			num2 *= component.Enemy.settings.selfDamageScale;
		}
		if (component != null && bullet.owner != null)
		{
			component.LastDamageDealer = bullet.owner.GetComponent<Vehicle>();
		}
		return num2;
	}

	private void BulletExplosion(bool hitGround, bool spawnParticles, bool drawBlastWaveParticles, bool fullHit, Bullet bullet, Vehicle targetVehicle)
	{
		BulletObject bulletObject = bullet.bulletObject;
		AudioMap.PlayClipAt(audioMap, hitGround ? "tankShellHitsGround" : "tankHit", bullet.transform.position, audioMap.effectsMixerGroup);
		if (bulletObject != null && (spawnParticles | hitGround))
		{
			GameObject particles = hitGround ? bulletObject.groundHitExplosion : bulletObject.tankHitExplosion;
			if (particles != null)
			{
				particles.transform.position = bullet.transform.position;
				particles.SetActive(value: true);
				ParticleSystem component = particles.GetComponent<ParticleSystem>();
				component.Play();
				Delay(component.main.duration, delegate
				{
					if (particles != null)
					{
						particles.SetActive(value: false);
					}
				});
			}
		}
		if (playerTankContainer != null)
		{
			float num = Vector3.Distance(bullet.transform.position, playerTankContainer.transform.position);
			float d = variables.cameraShakeExplosionCurve.Evaluate(num / variables.cameraShakeExplosionRange);
			cameraOffset += (Vector2)((playerTankContainer.transform.position - bullet.transform.position).normalized * variables.cameraRecoil * d);
			cameraShake += Vector2.one * variables.cameraShakeExplosion * d;
		}
		int num2 = bullet.hitMask | LayerMask.GetMask("Scrap", "TankDriver");
		if (bullet != null && (bool)bullet.owner && (bool)bullet.owner.gameObject)
		{
			num2 |= 1 << bullet.owner.gameObject.layer;
		}
		if (playerTankContainer != null && (bool)bullet.owner && bullet.owner == playerTankContainer.transform)
		{
			num2 |= LayerMask.GetMask("EnemyTankMain");
		}
		HashSet<Vehicle> hashSet = new HashSet<Vehicle>();
		RaycastHit2D[] array = Physics2D.CircleCastAll(bullet.transform.position, bullet.definition.damageRange * 0.5f, Vector2.up, 0f, num2);
		for (int i = 0; i < array.Length; i++)
		{
			RaycastHit2D raycastHit2D = array[i];
			Vehicle componentInParent = raycastHit2D.transform.GetComponentInParent<Vehicle>();
			bool flag = false;
			if (componentInParent != null && componentInParent != targetVehicle && (bool)bullet.owner && (bool)bullet.owner.gameObject)
			{
				if (componentInParent.gameObject.layer != bullet.owner.gameObject.layer)
				{
					flag = hashSet.Add(componentInParent);
				}
				else if (componentInParent.transform == bullet.owner)
				{
					flag = hashSet.Add(componentInParent);
				}
			}
			if (!flag)
			{
				continue;
			}
			Vector2 vector = raycastHit2D.transform.position - bullet.transform.position;
			Vector2 a = vector.normalized * bullet.definition.hitForce;
			a *= variables.damageRangeCurve.Evaluate(vector.sqrMagnitude / (bullet.definition.damageRange * bullet.definition.damageRange));
			if (raycastHit2D.rigidbody != null)
			{
				if (PlayerDataManager.IsSelectedGameModePvP)
				{
					a *= 0.5f;
				}
				raycastHit2D.rigidbody.AddForce(a, ForceMode2D.Impulse);
			}
		}
		if (bullet.definition.damageRange != 0f)
		{
			Vector3 position = bullet.transform.position;
			position.z = damageRangePrefab.transform.position.z;
			GameObject gameObject = UnityEngine.Object.Instantiate(damageRangePrefab, position, Quaternion.identity, base.transform);
			StartCoroutine(DoDamageRangeAnimation(gameObject, bullet.definition));
			if (drawBlastWaveParticles)
			{
				gameObject.transform.GetChild(1).gameObject.SetActive(value: true);
			}
		}
		TrailRenderer component2 = bullet.transform.GetComponent<TrailRenderer>();
		if (component2 != null)
		{
			DestroyBullet(bullet, component2.time);
		}
		else
		{
			DestroyBullet(bullet, 0f);
		}
		if ((bool)targetVehicle)
		{
			float num3 = DoDamage(bullet, targetVehicle.transform, fullHit);
			CheckHatRagdoll(targetVehicle, num3);
			if (targetVehicle != playerTankContainer || !playerInvincible)
			{
				if (targetVehicle.ActiveForceField == null)
				{
					targetVehicle.CurrentHealth -= num3;
				}
				else
				{
					targetVehicle.ActiveForceField.Absorb(bullet.transform.position, num3);
				}
			}
		}
		else
		{
			foreach (Vehicle item in hashSet)
			{
				if (item != null)
				{
					float num4 = DoDamage(bullet, item.transform, fullHit: false);
					CheckHatRagdoll(item, num4);
					if (item != playerTankContainer || !playerInvincible)
					{
						item.CurrentHealth -= num4;
					}
				}
			}
		}
	}

	private IEnumerator BulletExplosionRoutine(bool hitGround, bool spawnParticles, bool drawBlastWaveParticles, bool fullHit, Bullet bullet, TankContainer targetTankContainer)
	{
		yield return new WaitForSeconds(bullet.definition.explosionDelay);
		if (bullet.explodeDelayed)
		{
			BulletExplosion(hitGround, spawnParticles, drawBlastWaveParticles, fullHit, bullet, targetTankContainer);
		}
	}

	private void BulletExplosionDelayed(bool hitGround, bool spawnParticles, bool drawBlastWaveParticles, Bullet bullet)
	{
		StartCoroutine(BulletExplosionRoutine(hitGround, spawnParticles, drawBlastWaveParticles, fullHit: false, bullet, null));
	}

	private void UpdateBullets()
	{
		List<Bullet> list = new List<Bullet>();
		foreach (Bullet bullet2 in bullets)
		{
			bool flag = false;
			bool spawnParticles = false;
			Vehicle vehicle = null;
			if (bullet2.transform == null)
			{
				list.Add(bullet2);
			}
			else if (playerTankContainer != null && bullet2.owner == playerTankContainer.transform && (playerTankContainer.transform.position - bullet2.transform.position).sqrMagnitude > 3600f)
			{
				bullet2.cacheIndex = -1;
				DestroyBullet(bullet2, 0f);
			}
			else
			{
				Vector2 velocity = bullet2.transform.GetComponent<Rigidbody2D>().velocity;
				if (!bullet2.definition.preventSpriteRotation && velocity.magnitude > 0f)
				{
					SpriteRenderer componentInChildren = bullet2.transform.GetComponentInChildren<SpriteRenderer>();
					if (componentInChildren != null)
					{
						componentInChildren.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, 57.29578f * Mathf.Atan2(velocity.y, velocity.x) - 90f));
					}
				}
				ContactFilter2D contactFilter = default(ContactFilter2D);
				int layerCollisionMask = Physics2D.GetLayerCollisionMask(bullet2.transform.gameObject.layer);
				layerCollisionMask &= ~LayerMask.GetMask("ForceField", "ForceFieldRight");
				contactFilter.SetLayerMask(layerCollisionMask);
				contactFilter.useTriggers = true;
				if (bullet2.transform.GetComponent<Collider2D>().IsTouching(contactFilter))
				{
					bool drawBlastWaveParticles = true;
					bool flag2 = true;
					Collider2D bulletCollider = bullet2.transform.GetComponent<Collider2D>();
					foreach (Enemy enemy in enemies)
					{
						if (!(enemy.vehicleContainer == bullet2.owner))
						{
							flag = Array.Find(enemy.vehicleContainer.collider2Ds, (Collider2D v) => (bool)v && v.enabled && bulletCollider.IsTouching(v));
							flag2 = (flag2 && !flag);
							if (flag && enemy.vehicleContainer.CurrentHealth > 0f)
							{
								vehicle = enemy.vehicleContainer;
								spawnParticles = true;
								drawBlastWaveParticles = false;
							}
						}
					}
					if (vehicle == null && playerTankContainer != null)
					{
						flag = Array.Find(playerTankContainer.collider2Ds, (Collider2D v) => (bool)v && v.enabled && bulletCollider.IsTouching(v));
						flag2 = (flag2 && !flag);
						if (flag && playerTankContainer.CurrentHealth > 0f)
						{
							vehicle = playerTankContainer;
							spawnParticles = true;
							drawBlastWaveParticles = false;
						}
					}
					Bullet bullet = bullets.Find((Bullet v) => v.transform != null && bulletCollider.IsTouching(v.transform.GetComponentInChildren<Collider2D>()));
					flag2 = (flag2 && bullet == null);
					if (bullet2.definition.type == BulletType.Cannon || bullet2.definition.type == BulletType.Laser)
					{
						BulletExplosion(flag2, spawnParticles, drawBlastWaveParticles, flag, bullet2, vehicle);
					}
					else
					{
						BulletExplosionDelayed(flag2, spawnParticles, drawBlastWaveParticles, bullet2);
					}
					list.Add(bullet2);
				}
			}
		}
		foreach (Bullet item in list)
		{
			bullets.Remove(item);
		}
	}

	public float GetGroundHeight(float offset, bool approximate = true)
	{
		float num = 0f;
		if (PlayerDataManager.IsSelectedGameModePvP)
		{
			float num2 = offset - frontBlockerContainer.transform.position.x;
			if (num2 > -20f && num2 < 50f)
			{
				return GetGroundHeight(frontBlockerContainer.transform.position.x - 20f, approximate);
			}
		}
		int num3 = Mathf.FloorToInt(offset * 10f) - groundHeightCacheOffset;
		if (approximate && (bool)playerTankContainer && num3 > 0 && num3 < groundHeightCache.Length && groundHeightCache[num3].HasValue)
		{
			if (num3 + 1 < groundHeightCache.Length && groundHeightCache[num3 + 1].HasValue)
			{
				float t = offset * 10f - (float)num3;
				return Mathf.Lerp(groundHeightCache[num3].Value, groundHeightCache[num3 + 1].Value, t);
			}
			return groundHeightCache[num3].Value;
		}
		for (int i = 0; i < variables.hills.Count; i++)
		{
			Variables.Hill hill = variables.hills[i];
			if (!hill.disabled)
			{
				float num4 = themeOffset + Mathf.Max(groundGenerationSettings.groundWidth / 2f * hill.frequency + 0.5f, (offset + groundGenerationSettings.groundWidth / 2f) * hill.frequency);
				float f = 0f;
				if (hill.mode == Variables.Hill.Mode.PerlinNoise)
				{
					f = variables.GetPerlinNoise(num4 + hill.offset, 0f);
				}
				else if (hill.mode == Variables.Hill.Mode.Sin)
				{
					f = Mathf.Sin((num4 + hill.offset) * (float)Math.PI);
				}
				num += Mathf.Pow(f, hill.pow) * hill.amplitude * (float)((!hill.subtractive) ? 1 : (-1));
			}
		}
		if (num3 > 0 && num3 < groundHeightCache.Length)
		{
			groundHeightCache[num3] = num;
		}
		return num;
	}

	private void UpdateGroundHeightCache()
	{
		if ((bool)playerTankContainer && !(playerTankContainer.transform.position.x < (float)nextGroundHeightCacheShiftX))
		{
			nextGroundHeightCacheShiftX += 20;
			groundHeightCacheOffset += 200;
			groundHeightCache = groundHeightCache.Shift(200);
		}
	}

	private void UpdateGroundMesh(float offset)
	{
		if (!(theme == null))
		{
			for (int i = 0; i < groundGenerationSettings.groundPrecision * 2; i += 2)
			{
				float num = offset - groundGenerationSettings.groundWidth * 0.5f + (float)i * 0.5f * groundGenerationSettings.groundWidth / (float)groundGenerationSettings.groundPrecision;
				float groundHeight = GetGroundHeight(num, approximate: false);
				groundVertices[i] = new Vector3(num, groundHeight, 0f);
				groundVertices[i + 1] = new Vector3(num, groundHeight - groundGenerationSettings.groundHeight, 0f);
				groundColliderVertices[i / 2 + 1] = new Vector2(groundVertices[i].x, groundVertices[i].y);
				groundUvs[i] = new Vector2(groundVertices[i].x, 1f);
				groundUvs[i + 1] = new Vector2(groundVertices[i].x, 0f);
				Vector3 vector = groundVertices[i + 1] - groundVertices[i];
				middleGroundMaskVertices[i] = groundVertices[i] + vector.normalized * 0.4f;
				middleGroundMaskVertices[i + 1] = groundVertices[i + 1];
				middleGroundUvs[i] = groundUvs[i];
				middleGroundUvs[i].y -= 0.4f / vector.magnitude;
				middleGroundUvs[i + 1] = groundUvs[i + 1];
			}
			for (int j = 1; j < groundGenerationSettings.groundPrecision; j++)
			{
				groundIndices[(j - 1) * 6] = j * 2 - 2;
				groundIndices[(j - 1) * 6 + 1] = j * 2;
				groundIndices[(j - 1) * 6 + 2] = j * 2 - 1;
				groundIndices[(j - 1) * 6 + 3] = j * 2;
				groundIndices[(j - 1) * 6 + 4] = j * 2 + 1;
				groundIndices[(j - 1) * 6 + 5] = j * 2 - 1;
			}
			previousGroundMeshBuiltAtOffset = offset;
			if (theme.groundMesh.sharedMesh == null)
			{
				theme.groundMesh.sharedMesh = new Mesh();
			}
			theme.groundMesh.sharedMesh.Clear();
			theme.groundMesh.sharedMesh.vertices = groundVertices;
			theme.groundMesh.sharedMesh.triangles = groundIndices;
			theme.groundMesh.sharedMesh.uv = groundUvs;
			if (theme.middleGroundMesh.sharedMesh == null)
			{
				theme.middleGroundMesh.sharedMesh = new Mesh();
			}
			theme.middleGroundMesh.sharedMesh.Clear();
			theme.middleGroundMesh.sharedMesh.vertices = middleGroundMaskVertices;
			theme.middleGroundMesh.sharedMesh.triangles = groundIndices;
			theme.middleGroundMesh.sharedMesh.uv = middleGroundUvs;
			Vector3 vector2 = groundVertices[1];
			Vector3 vector3 = groundVertices[groundGenerationSettings.groundPrecision * 2 - 1];
			groundColliderVertices[0] = new Vector2(vector2.x, vector2.y);
			groundColliderVertices[groundGenerationSettings.groundPrecision + 1] = new Vector2(vector3.x, vector3.y);
			theme.groundCollider.SetPath(0, groundColliderVertices);
		}
	}

	public static void DoBooster(Vehicle v = null)
	{
		if (v == null)
		{
			v = instance.playerTankContainer;
			if (PlayerDataManager.SelectedGameMode == GameMode.Arena2v2)
			{
				AIController.instance.RepairKitUsed(v.transform.position);
			}
		}
		if (v.Booster.type != 0)
		{
			instance.StartCoroutine(v.UseBooster());
		}
	}

	private void OnApplicationPause(bool pause)
	{
		if (!pause && Running && /*(DateTime.Now /*- AdsManager.ApplicationPausedAt*//*).TotalSeconds > 20.0 &&*/ PlayerDataManager.IsSelectedGameModePvP)
		{
			Running = false;
			MenuController.HideMenu<GameMenu>();
			PlayerDataManager.SelectedGameMode = GameMode.Adventure;
			LoadingScreen.ReloadGame(delegate
			{
				MenuController.ShowMenu<MainMenu>().UpdatePlayMenu();
			});
		}
	}
}
