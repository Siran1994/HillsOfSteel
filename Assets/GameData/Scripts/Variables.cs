using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;

public class Variables : ScriptableObject
{
	[Serializable]
	public class Hill
	{
		public enum Mode
		{
			PerlinNoise,
			Sin
		}

		public Mode mode;

		public bool disabled;

		[Range(0f, 0.5f)]
		[Tooltip("Makes hills more frequent")]
		public float frequency;

		[Tooltip("Makes hills higher")]
		public float amplitude;

		[Tooltip("Highlights spikes")]
		[Range(1f, 10f)]
		public float pow;

		[Tooltip("Makes holes")]
		public bool subtractive;

		[Tooltip("Perlin noise offset")]
		public float offset;
	}

	[Serializable]
	public struct SpawnRegion
	{
		[Serializable]
		public struct EnemyProbabilityPair
		{
			public int enemyIndex;

			public float probability;
		}

		public int afterTanks;

		public EnemyProbabilityPair[] enemyProbabilities;
	}

	[Serializable]
	public class GroundGenerationSettings
	{
		[Tooltip("Needs to be wide enough so enemies don't spawn on empty ground. Too big and it will hurt performance.")]
		public float groundWidth;

		[Tooltip("Needs to be high enough so hill's won't stop before bottom of the screen")]
		public float groundHeight;

		[Tooltip("In how big chuncs the ground is generated. Too small and it will hurt performance. Too big and enemies might spawn on empty ground.")]
		public int groundRefreshInterval;

		[Tooltip("Ground smoothnes. Too high and it will hurt performance. Too low and the ground will look blocky.")]
		public int groundPrecision;
	}

	[Serializable]
	public class Level
	{
		public string name;

		[Header("Ground")]
		[Tooltip("Different types of hills")]
		public List<Hill> hills;

		[Header("Misc")]
		public float gravity = -6.93f;

		[Header("Spawn spacing: d + rnd(lerp(min, 0, eval(minc, d/cap)), min + lerp(max, 0, eval(maxc, d/cap)))")]
		[Tooltip("Minimum spawn distance (min)")]
		public float minSpawnDistance;

		[Tooltip("Minimum distance between spawn points as function of distance from origo. (minc)")]
		public AnimationCurve minSpawnDistanceCurve;

		[Tooltip("Maximum spawn distance (max)")]
		public float maxSpawnDistance;

		[Tooltip("Maximum distance between spawn points as function of distance from origo (maxc)")]
		public AnimationCurve maxSpawnDistanceCurve;

		[Tooltip("Distance from origo after which the spawn distance curve is capped (cap)")]
		public float spawnDistanceCurveCap;

		[Tooltip("How close the player needs to be for a tank to spawn")]
		public float spawnRadius;

		[Header("Spawn group size")]
		[Tooltip("Max group size")]
		public int maxGroupSize;

		[Tooltip("Probability of an extra tank as function of distance from origo")]
		public AnimationCurve extraTankProbabilityCurve;

		[Tooltip("Distance from origo after which extra tank probability doesn't increase")]
		public float extraTankProbabilityDistanceCap;

		[Tooltip("How far away tanks are from each other when spawned in a group")]
		public float extraTankSpacing;

		[Header("Spawn types")]
		public SpawnRegion[] spawnRegions;

		public int[] bosses;

		[Header("Cosmetics")]
		public HatType hat;

		public Color[] dustColors;

		[Header("Economy")]
		public int price;

		public int coinsPerScore;

		[Header("1vs1")]
		public float[] levelOffsets;

		public LeaderboardID leaderboard;
	}

	[Serializable]
	public class EnemyTank
	{
		[Serializable]
		public struct Shot
		{
			[Tooltip("How long the tank aims before shooting")]
			public float aimDuration;

			[Tooltip("How long the tank is idle after shooting")]
			public float shootDuration;

			[Tooltip("How long the tank moves before aiming")]
			public float moveDuration;
		}

		public string name;

		public Sprite nameSprite;

		public GameObject prefab;

		public int spawnLimit;

		public TankProgression.Stats stats;

		[Tooltip("Tank move speed")]
		public float speedWhileAttacking;

		public float distanceTarget;

		public bool hasToBeSeenBeforeAttacks = true;

		public bool getBackToPlayerAfterPassing;

		public BulletDefinition bullet;

		[Tooltip("How low can the tank aim")]
		public float minAngle;

		[Tooltip("How high can the tank aim")]
		public float maxAngle;

		public Shot[] shots;

		[Tooltip("How close the enemy has to be able to hit before it shoots")]
		public float maxAimDistance = 1.5f;

		[Range(0f, 1f)]
		public float selfDamageScale = 1f;

		public int scoreReward = 1;

		public AchievementID achievement;
	}

	[Serializable]
	public class ChestGenerationSettings
	{
		[Serializable]
		public class TankCardDrop
		{
			public Rarity rarity;

			[Range(0f, 1f)]
			public float probability;

			public MinMaxInt stackSize;
		}

		public Rarity rarity;

		[Header("Probabilities")]
		[Range(0f, 1f)]
		public float newTankCardProbability;

		public int newTankCardsMax;

		[Range(0f, 1f)]
		public float newBoosterCardProbability;

		public int newBoosterCardsMax;

		[Range(0f, 1f)]
		public float gemDropProbability;

		[Tooltip("Rarity probability weights")]
		[ArrayElementTitle("rarity")]
		public TankCardDrop[] tankCardPool;

		[Header("Cards")]
		[Tooltip("Number of different tank type cards (random from range)")]
		public MinMaxInt tankCardCount;

		[Tooltip("Number of booster cards (random from range)")]
		public MinMaxInt boosterCardCount;

		[Tooltip("Number of booster cards per stack (random from range)")]
		public MinMaxInt boosterStackSize;

		[Header("Currencies")]
		public MinMaxInt coinCount;

		public MinMaxInt gemCount;

		public Rarity GetTankRarity(float probability)
		{
			float num = 0f;
			for (int num2 = tankCardPool.Length - 1; num2 > 0; num2--)
			{
				num += tankCardPool[num2].probability;
				if (probability < num)
				{
					return tankCardPool[num2].rarity;
				}
			}
			return Rarity.Common;
		}

		public TankCardDrop GetDrop(Rarity rarity)
		{
			for (int i = 0; i != tankCardPool.Length; i++)
			{
				if (tankCardPool[i].rarity.Equals(rarity))
				{
					return tankCardPool[i];
				}
			}
			return null;
		}

		public override string ToString()
		{
			return rarity.ToString();
		}
	}

	[Serializable]
	public struct ChestStruct
	{
		public Rarity rarity;

		public int gemValue;

		[Range(0f, 1f)]
		public float dropProbability;

		public Sprite sprite;
	}

	[Serializable]
	public class Bundle
	{
		public string iapId;

		public int showAfterTimesTried;
	}

	[Serializable]
	public struct CoinPack
	{
		public string name;

		public int coinAmount;

		public int gemPrice;
	}

	private static Variables _instance;

	public List<Level> levels;

	public GroundGenerationSettings adventureGenerationSettings;

	public GroundGenerationSettings arenaGenerationSettings;

	[Header("Player Movement")]
	public float wheelSuspensionFrequency;

	[Range(0f, 1f)]
	public float wheelSuspensionDampening;

	public float defaultFumes;

	public float engineSoundAlteringSpeed;

	public float engineMaxPitch;

	public float engineMinVolume;

	public float engineMaxVolume;

	[Tooltip("Multiplied by speed and added to fume rate")]
	public float speedFumes;

	public Color nearDeathFumesColor;

	public int maxDustPartices = 50;

	public float maxDustParticleSpeed = 10f;

	public float flippedOverDamageTime = 1f;

	[Range(0f, 1f)]
	public float flippedOverDamagePerSecond = 1f;

	[Header("Camera")]
	[Tooltip("Screen width in pixels")]
	public float screenWidth;

	[Tooltip("How fast the camera follows the player")]
	public float cameraSpring;

	[Tooltip("How much the camera is offset from the player")]
	public float cameraOffset;

	[Tooltip("How big camera recoil shooting causes")]
	public float cameraRecoil;

	[Tooltip("How much the camera shakes when someone shoots (including enemies)")]
	public float cameraShootShake;

	[Tooltip("How fast the camera shakes")]
	public float cameraShakeFrequency;

	[Tooltip("How fast the camera shake amplitude is reduced")]
	public float cameraShakeDampening;

	[Tooltip("How much the camera shakes when something explodes (or bullet hits ground)")]
	public float cameraShakeExplosion;

	[Tooltip("Maximum range for explosion camera shake")]
	public float cameraShakeExplosionRange;

	[Tooltip("Explosion camera shake rolloff")]
	public AnimationCurve cameraShakeExplosionCurve;

	[Range(0f, 1f)]
	public float selfDamageScale;

	[Tooltip("Damage as function of distance to explosion")]
	public AnimationCurve damageRangeCurve;

	public Vector2 tankDeathForce;

	public float tankDeathForceOffset;

	[Tooltip("How forcefully scrap flies as function of negative hp")]
	public AnimationCurve tankDeathForceHpFactorCurve;

	[Tooltip("Hp cap after which scarp force doesn't increase")]
	public float tankDeathForceHpFactorHpCap;

	public AnimationCurve damageRangeSpriteGrowCurve;

	public AnimationCurve damageRangeSpriteAlphaCurve;

	public float damageRangeSpriteTime;

	public float damageScaleDownDistance = 18f;

	public float maxDamageDistance = 25f;

	[Range(0f, 1f)]
	public float damageScaleDownStart = 0.75f;

	[Range(0f, 1f)]
	public float damageScaleDownEnd;

	[Header("Enemies")]
	public List<EnemyTank> enemies;

	public float enemyScrapDuration = 3.5f;

	public float enemyScrapFadeOutTime = 1.75f;

	[Header("Tanks")]
	public Tank[] tanks;

	public int[] tankOrder;

	public GameObject particleEffectShoot;

	public GameObject particleEffectBulletHitGround;

	public GameObject particleEffectBulletHitTank;

	[Header("Coin groups")]
	[Range(0f, 1f)]
	public float coinsInsteadOfEnemiesChance = 0.25f;

	[Range(1f, 20f)]
	public int coinGroupSize = 5;

	public float coinExtraSpacing = 2f;

	[Header("Ranking system")]
	public Rank[] ranks;//¾üÏÎÏµÍ³

	[Header("Economy")]
	[Tooltip("How many Gems per Dollar")]
	public float dollarGemValue;

	[Tooltip("How many Coins per Gem")]
	public float gemCoinValue;

	[Tooltip("Gem multiplier for premium unlocks")]
	public float premiumOfferMultiplier;

	[Tooltip("Premium daily offer rarity probabilities")]
	[ArrayElementTitle("rarity")]
	public ChestGenerationSettings.TankCardDrop[] premiumOfferProbabilities;

	public float[] premiumDailyOfferDiscounts;

	public MinMaxFloat dailyOfferDiscount;

	[Tooltip("Percentage of cards needed for the next level")]
	public MinMaxFloat dailyOfferStackSize;

	public int reviveCost = 10;

	public float reviveTime = 5f;

	[Range(0f, 1f)]
	public float reviveHealthRestore = 0.5f;

	public MinMaxInt tankLevelMinMax;

	[ArrayElementTitle("rarity")]
	public TankRarityProgression[] tankRarityProgression;

	[ArrayElementTitle("rarity")]
	public ChestStruct[] chests;

	[ArrayElementTitle("rarity")]
	public ChestGenerationSettings[] chestRewards;

	[ArrayElementTitle("type")]
	public ChestProgression[] chestProgression;

	public Bundle[] bundles;

	public int[] iapCoinCounts;

	public int[] dailyBonusAmounts;

	public ScoreAchievement[] scoreAchievements;

	public int showAdsBeforeOffer = 3;

	[Header("Boosters")]
	public Booster[] boosters;

	public int[] boosterLevelUpCosts;

	public int[] boosterCardsPerLevelUp;

	public int maxCardsInReward;

	public GameObject circleSawPrefab;

	[Header("Shop items")]
	public CoinPack[] coinPacks;

	[Header("1vs1 & 2vs2")]
	public RankStats[] rankStats;

	public int[] winStreakBonuses;

	[ArrayElementTitle("achievement")]
	public WinStreakAchievement[] winStreakAchievements;

	public Driver.Uniform allyUniform;

	public Driver.Uniform enemyUniform;

	public Driver.Uniform vipUniform;

	[Header("Adventure")]
	public float stageDifficultyAddition = 0.15f;

	[Header("Classic")]
	public float classicPerKillDifficultyAddition = 0.05f;

	[Header("Boss Rush")]
	public TransactionCost bossRushCost;

	public int bossRushStageReward = 10;

	public int[] bossSpawnOrder;

	public float bossRushDifficultyIncrease = 0.2f;

	[Header("UI")]
	public ButtonSprites defaultButtonSprites;

	public ButtonSprites roundedButtonSprites;

	[Header("Easter")]
	public int easterEggCoins = 25;

	public static Variables instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = UnityEngine.Object.FindObjectOfType<Variables>();
			}
			return _instance;
		}
		private set
		{
			_instance = value;
		}
	}

	public List<Hill> hills => levels[TankGame.SelectedLevel].hills;

	public LeaderboardID leaderboard => levels[TankGame.SelectedLevel].leaderboard;

	public float minSpawnDistance => levels[TankGame.SelectedLevel].minSpawnDistance;

	public AnimationCurve minSpawnDistanceCurve => levels[TankGame.SelectedLevel].minSpawnDistanceCurve;

	public float maxSpawnDistance => levels[TankGame.SelectedLevel].maxSpawnDistance;

	public AnimationCurve maxSpawnDistanceCurve => levels[TankGame.SelectedLevel].maxSpawnDistanceCurve;

	public float spawnDistanceCurveCap => levels[TankGame.SelectedLevel].spawnDistanceCurveCap;

	public float spawnRadius => levels[TankGame.SelectedLevel].spawnRadius;

	public int maxGroupSize => levels[TankGame.SelectedLevel].maxGroupSize;

	public AnimationCurve extraTankProbabilityCurve => levels[TankGame.SelectedLevel].extraTankProbabilityCurve;

	public float extraTankProbabilityDistanceCap => levels[TankGame.SelectedLevel].extraTankProbabilityDistanceCap;

	public float extraTankSpacing => levels[TankGame.SelectedLevel].extraTankSpacing;

	public float gravity => levels[TankGame.SelectedLevel].gravity;

	public SpawnRegion[] spawnRegions => levels[TankGame.SelectedLevel].spawnRegions;

	public Color[] dustColors => levels[TankGame.SelectedLevel].dustColors;

	public int[] bosses => levels[TankGame.SelectedLevel].bosses;

	public float[] levelOffsets => levels[TankGame.SelectedLevel].levelOffsets;

	private void OnEnable()
	{
		instance = this;
	}

	public GroundGenerationSettings GetGroundGenerationSettings(GameMode mode)
	{
		if (mode == GameMode.Arena || mode == GameMode.Arena2v2)
		{
			return arenaGenerationSettings;
		}
		return adventureGenerationSettings;
	}

	public List<ScoreAchievement> GetScoreAchievements(int score)
	{
		List<ScoreAchievement> list = new List<ScoreAchievement>();
		for (int i = 0; i < scoreAchievements.Length; i++)
		{
			if (scoreAchievements[i].score < score)
			{
				list.Add(scoreAchievements[i]);
			}
		}
		return list;
	}

	public List<WinStreakAchievement> GetWinStreakAchievements(GameMode mode, int streak)
	{
		List<WinStreakAchievement> list = new List<WinStreakAchievement>();
		for (int i = 0; i != winStreakAchievements.Length; i++)
		{
			if (winStreakAchievements[i].mode.Equals(mode) && streak >= winStreakAchievements[i].streak)
			{
				list.Add(winStreakAchievements[i]);
			}
		}
		return list;
	}

	public float GetDamageAdjustedWithPosition(float startX, float endX, float damage)
	{
		float num = Mathf.Abs(endX - startX);
		if (num > damageScaleDownDistance)
		{
			float value = (num - damageScaleDownDistance) / (maxDamageDistance - damageScaleDownDistance);
			return Mathf.Lerp(damageScaleDownStart, damageScaleDownEnd, Mathf.Clamp01(value)) * damage;
		}
		return damage;
	}

	public Rank GetRank(int xp)
	{
		for (int i = 1; i < ranks.Length; i++)
		{
			if (ranks[i].xp > xp)
			{
				return ranks[i - 1];
			}
		}
		return ranks[0];
	}

	public List<Rank> GetRanksUntil(int xp)
	{
		List<Rank> list = new List<Rank>();
		for (int i = 0; i < ranks.Length && ranks[i].xp <= xp; i++)
		{
			list.Add(ranks[i]);
		}
		return list;
	}

	public Ranks GetRanks(int xp, int xpAdd)
	{
		Ranks ranks = default(Ranks);
		ranks.ranks = new List<Rank>();
		for (int i = 0; i < this.ranks.Length; i++)
		{
			if (this.ranks[i].xp <= xp)
			{
				ranks.ranks.Add(this.ranks[i]);
			}
			else if ((xp < this.ranks[0].xp || ranks.ranks.Count > 0) && this.ranks[i].xp <= xp + xpAdd)
			{
				ranks.DidLevelUp = true;
				ranks.ranks.Add(this.ranks[i]);
			}
			else if (this.ranks[i].xp > xp + xpAdd)
			{
				ranks.ranks.Add(this.ranks[i]);
				break;
			}
		}
		return ranks;
	}

	public float GetRandomEnemySpawnOffset(float previousSpawnPosition)
	{
		float num = Mathf.Lerp(0f, minSpawnDistance, minSpawnDistanceCurve.Evaluate(previousSpawnPosition / spawnDistanceCurveCap));
		float num2 = Mathf.Lerp(0f, maxSpawnDistance, maxSpawnDistanceCurve.Evaluate(previousSpawnPosition / spawnDistanceCurveCap));
		return previousSpawnPosition + UnityEngine.Random.Range(num, num + num2);
	}

	public int GetRandomGroupSize(float spawnPosition, int max)
	{
		int num = 1;
		for (int i = 0; i < maxGroupSize; i++)
		{
			if (UnityEngine.Random.value < extraTankProbabilityCurve.Evaluate(spawnPosition / extraTankProbabilityDistanceCap))
			{
				num++;
			}
		}
		return Mathf.Min(max, num);
	}

	public int GetTankIndex(string id)
	{
		for (int i = 0; i < tanks.Length; i++)
		{
			if (tanks[i].id.Equals(id))
			{
				return i;
			}
		}
		return -1;
	}

	public int GetMaxTankUpgradeLevel(string tankId)
	{
		return GetMaxTankUpgradeLevel(GetTank(tankId));
	}

	public int GetMaxTankUpgradeLevel(Tank tank)
	{
		return GetTankLevelUpRequirements(tank.rarity).levelUpCostCards.Length - 1;
	}

	public Tank FindTankFromProduct(Product product)
	{
		if (product != null)
		{
			foreach (PayoutDefinition payout in product.definition.payouts)
			{
				if (payout.subtype.Contains("tank"))
				{
					return GetTank(payout.subtype.Replace("Cards", ""));
				}
			}
		}
		return null;
	}

	public Tank GetTank(string id)
	{
		for (int i = 0; i < tanks.Length; i++)
		{
			if (tanks[i].id.Equals(id))
			{
				return tanks[i];
			}
		}
		return null;
	}

	public Tank GetTank(PlayerTankType type)
	{
		for (int i = 0; i < tanks.Length; i++)
		{
			if (tanks[i].type.Equals(type))
			{
				return tanks[i];
			}
		}
		return null;
	}

	public bool IsValidTankId(string id)
	{
		for (int i = 0; i < tanks.Length; i++)
		{
			if (tanks[i].id.Equals(id))
			{
				return true;
			}
		}
		return false;
	}

	public Tank GetTank(int index)
	{
		return tanks[tankOrder[index]];
	}

	public EnemyTank GetRandomBoss(int lastBoss, out int typeIndex)
	{
		int num = 0;
		do
		{
			typeIndex = bosses[UnityEngine.Random.Range(0, bosses.Length)];
			num++;
		}
		while (num < bosses.Length && typeIndex == lastBoss);
		return enemies[typeIndex];
	}

	public EnemyTank GetRandomEnemy(int tanks, ref int[] spawned, out int typeIndex)
	{
		int i;
		for (i = 0; i < spawnRegions.Length - 1 && tanks >= spawnRegions[i + 1].afterTanks; i++)
		{
		}
		float num = 0f;
		for (int j = 0; j < spawnRegions[i].enemyProbabilities.Length; j++)
		{
			num += spawnRegions[i].enemyProbabilities[j].probability;
		}
		float num2 = UnityEngine.Random.value;
		EnemyTank result = enemies[0];
		typeIndex = 0;
		for (int k = 0; k < spawnRegions[i].enemyProbabilities.Length; k++)
		{
			int enemyIndex = spawnRegions[i].enemyProbabilities[k].enemyIndex;
			float num3 = spawnRegions[i].enemyProbabilities[k].probability / num;
			if (num3 >= num2)
			{
				if (enemies[enemyIndex].spawnLimit <= 0 || spawned[enemyIndex] < enemies[enemyIndex].spawnLimit)
				{
					result = enemies[enemyIndex];
					typeIndex = enemyIndex;
					break;
				}
			}
			else
			{
				num2 -= num3;
			}
		}
		return result;
	}

	public float EvaluateDeathForceFactor(float hp)
	{
		return tankDeathForceHpFactorCurve.Evaluate(Mathf.Abs(hp) / tankDeathForceHpFactorHpCap);
	}

	public float GetPerlinNoise(float x, float y)
	{
		return Mathf.PerlinNoise(x, y);
	}

	public TankProgression GetMaxProgression()
	{
		TankProgression tankProgression = default(TankProgression);
		for (int i = 0; i < tanks.Length; i++)
		{
			tankProgression.maxStep.acceleration = Mathf.Max(tankProgression.maxStep.acceleration, tanks[i].progression.maxStep.acceleration);
			tankProgression.maxStep.damage = Mathf.Max(tankProgression.maxStep.damage, tanks[i].progression.maxStep.damage);
			tankProgression.maxStep.health = Mathf.Max(tankProgression.maxStep.health, tanks[i].progression.maxStep.health);
			tankProgression.maxStep.maxSpeed = Mathf.Max(tankProgression.maxStep.maxSpeed, tanks[i].progression.maxStep.maxSpeed);
			tankProgression.maxStep.reloadTime = Mathf.Max(tankProgression.maxStep.reloadTime, tanks[i].progression.maxStep.reloadTime);
			tankProgression.maxStep.dps = Mathf.Max(tankProgression.maxStep.dps, tanks[i].progression.maxStep.dps);
		}
		return tankProgression;
	}

	public int GetTankLevelUpCoins(Rarity rarity, int level)
	{
		TankRarityProgression tankLevelUpRequirements = GetTankLevelUpRequirements(rarity);
		if (tankLevelUpRequirements == null || !tankLevelMinMax.ContainsExclusive(level))
		{
			return -1;
		}
		return tankLevelUpRequirements.levelUpCostCoins[level];
	}

	public int GetTankLevelUpCards(Rarity rarity, int level)
	{
		TankRarityProgression tankLevelUpRequirements = GetTankLevelUpRequirements(rarity);
		if (tankLevelUpRequirements == null || !tankLevelMinMax.ContainsExclusive(level))
		{
			return -1;
		}
		return tankLevelUpRequirements.levelUpCostCards[level];
	}

	public int GetTankLevelUpCardsCumulative(Rarity rarity, int level)
	{
		TankRarityProgression tankLevelUpRequirements = GetTankLevelUpRequirements(rarity);
		if (level >= tankLevelMinMax.max)
		{
			level = tankLevelMinMax.max - 1;
		}
		int num = 0;
		for (int i = 0; i <= level; i++)
		{
			num += tankLevelUpRequirements.levelUpCostCards[i];
		}
		return num;
	}

	public bool CanUpgradeBooster(Booster booster)
	{
		return booster.Count >= GetBoosterCardCountNextLevel(booster.Level);
	}

	public Booster GetBooster(string id)
	{
		for (int i = 0; i < boosters.Length; i++)
		{
			if (boosters[i].id.Equals(id))
			{
				return boosters[i];
			}
		}
		return new Booster();
	}

	public int GetBoosterRewardTypeCount()
	{
		return UnityEngine.Random.Range(1, 4);
	}

	public int GetBoosterCardRewardCount()
	{
		return UnityEngine.Random.Range(1, maxCardsInReward + 1);
	}

	public Booster GetRandomBooster()
	{
		if (PlayerDataManager.GetTankBoosters(tanks[0])[0].Count == 0)
		{
			return boosters[0];
		}
		return boosters[UnityEngine.Random.Range(0, boosters.Length - 1)];
	}

	public int GetBoosterCardCountMax()
	{
		return boosterCardsPerLevelUp[boosterCardsPerLevelUp.Length - 1];
	}

	public int GetBoosterCardCountNextLevel(int currentLevel)
	{
		if (currentLevel + 1 >= boosterCardsPerLevelUp.Length)
		{
			return -1;
		}
		return boosterCardsPerLevelUp[currentLevel + 1];
	}

	public int GetBoosterLevelUpPriceForNextLevel(int currentLevel)
	{
		if (currentLevel + 1 >= boosterLevelUpCosts.Length)
		{
			return -1;
		}
		return boosterLevelUpCosts[currentLevel + 1];
	}

	public TankRarityProgression GetTankLevelUpRequirements(Rarity rarity)
	{
		for (int i = 0; i != tankRarityProgression.Length; i++)
		{
			if (tankRarityProgression[i].rarity.Equals(rarity))
			{
				return tankRarityProgression[i];
			}
		}
		return null;
	}

	public int GetTankPossibleLevel(Tank tank, int cards)
	{
		TankRarityProgression tankLevelUpRequirements = instance.GetTankLevelUpRequirements(tank.rarity);
		int num = 0;
		for (int i = 0; i != tankLevelUpRequirements.levelUpCostCards.Length; i++)
		{
			num += tankLevelUpRequirements.levelUpCostCards[i];
			if (num > cards)
			{
				return i;
			}
		}
		return 0;
	}

	public Tank GetRandomTank()
	{
		return tanks[UnityEngine.Random.Range(0, GetMaxTankCount())];
	}

	public Tank GetRandomTank(Rarity rarity)
	{
		List<Tank> list = new List<Tank>();
		for (int i = 0; i != tanks.Length; i++)
		{
			if (tanks[i].rarity.Equals(rarity))
			{
				list.Add(tanks[i]);
			}
		}
		return list.Random();
	}

	public int GetMaxTankCount()
	{
		return tanks.Length;
	}

	public int GetMaxTankCardCount(Tank tank)
	{
		return GetMaxTankCardCount(tank.rarity);
	}

	public int GetMaxTankCardCount(Rarity rarity)
	{
		int num = 0;
		TankRarityProgression tankLevelUpRequirements = GetTankLevelUpRequirements(rarity);
		for (int i = 0; i != tankLevelUpRequirements.levelUpCostCards.Length; i++)
		{
			num += tankLevelUpRequirements.levelUpCostCards[i];
		}
		return num;
	}

	public int GetTankCardCoinValue(Rarity tankRarity)
	{
		for (int i = 0; i != tanks.Length; i++)
		{
			if (tanks[i].rarity.Equals(tankRarity))
			{
				return tanks[i].coinValue;
			}
		}
		return 0;
	}

	public int GetTankGemValue(Tank tank)
	{
		return Mathf.RoundToInt(GetTankLevelUpRequirements(tank.rarity).cardGemValue * dollarGemValue * premiumOfferMultiplier);
	}

	public RankStats GetPrevRankStats(int currentRating)
	{
		for (int num = rankStats.Length - 1; num >= 1; num--)
		{
			if (currentRating > rankStats[num - 1].maxTrophies)
			{
				if (num - 1 < 0)
				{
					return default(RankStats);
				}
				return rankStats[num - 1];
			}
		}
		return default(RankStats);
	}

	public RankStats GetRankStats(int currentRating)
	{
		for (int num = rankStats.Length - 1; num >= 1; num--)
		{
			if (currentRating > rankStats[num - 1].maxTrophies)
			{
				return rankStats[num];
			}
		}
		return rankStats[0];
	}

	public RankStats GetNextRankStats(int currentRating)
	{
		for (int num = rankStats.Length - 1; num >= 1; num--)
		{
			if (currentRating > rankStats[num - 1].maxTrophies)
			{
				if (num + 1 >= rankStats.Length)
				{
					return default(RankStats);
				}
				return rankStats[num + 1];
			}
		}
		return rankStats[1];
	}

	public int GetMaxTrophies()
	{
		return rankStats[rankStats.Length - 1].maxTrophies;
	}

	public ChestGenerationSettings GetChestSettings(Rarity rarity)
	{
		for (int i = 0; i != chestRewards.Length; i++)
		{
			if (chestRewards[i].rarity.Equals(rarity))
			{
				return chestRewards[i];
			}
		}
		return null;
	}

	public ChestRewards GenerateChestRewards(Rarity rarity)
	{
		ChestRewards lhs = new ChestRewards();
		ChestGenerationSettings chestSettings = GetChestSettings(rarity);
		if (chestSettings == null)
		{
			UnityEngine.Debug.LogError("Chest reward pool not found.");
			return null;
		}
		lhs += GenerateTankRewards(chestSettings);
		List<Tank> list = new List<Tank>();
		foreach (Card card in lhs.cards)
		{
			Tank tank = GetTank(card.id);
			if (card.isNew)
			{
				list.Add(tank);
			}
		}
		lhs += GenerateBoosterRewards(chestSettings, list);
		lhs.cards.Sort(delegate(Card item, Card other)
		{
			if (item.type == CardType.BoosterCard)
			{
				return 1;
			}
			if (other.type == CardType.BoosterCard)
			{
				return -1;
			}
			if (item.rarity > other.rarity)
			{
				return 1;
			}
			return (item.rarity < other.rarity) ? (-1) : 0;
		});
		lhs.coins += chestSettings.coinCount.Random();
		if (UnityEngine.Random.value <= chestSettings.gemDropProbability)
		{
			lhs.gems += chestSettings.gemCount.Random();
		}
		if (PlayerDataManager.IsSubscribed())
		{
			for (int i = 0; i != lhs.cards.Count; i++)
			{
				lhs.cards[i].count *= 2;
			}
			lhs.coins *= 2;
			lhs.gems *= 2;
		}
		return lhs;
	}

	private ChestRewards GenerateTankRewards(ChestGenerationSettings chest)
	{
		ChestRewards chestRewards = new ChestRewards();
		Dictionary<Rarity, List<Tank>> tanksWithNoCards = PlayerDataManager.GetTanksWithNoCards();
		int num = 0;
		foreach (KeyValuePair<Rarity, List<Tank>> item in tanksWithNoCards)
		{
			num += item.Value.Count;
		}
		Dictionary<Rarity, List<Tank>> tanksWithOwnedCards = PlayerDataManager.GetTanksWithOwnedCards();
		Dictionary<Rarity, List<Tank>> nonMaxedTanks = PlayerDataManager.GetNonMaxedTanks();
		int num2 = chest.tankCardCount.Random();
		int num3 = 0;
		for (int i = 0; i != chest.newTankCardsMax; i++)
		{
			if (UnityEngine.Random.value < chest.newTankCardProbability)
			{
				num3++;
			}
		}
		for (int j = 0; j != num2; j++)
		{
			Rarity tankRarity = chest.GetTankRarity(UnityEngine.Random.value);
			Rarity[] raritiesOrderedPriorityLow = GetRaritiesOrderedPriorityLow(tankRarity);
			Tank tank = null;
			bool flag = false;
			if (nonMaxedTanks[tankRarity].Count == 0)
			{
				for (int k = 0; k != raritiesOrderedPriorityLow.Length; k++)
				{
					if (nonMaxedTanks[raritiesOrderedPriorityLow[k]].Count > 0)
					{
						flag = true;
						tank = nonMaxedTanks[raritiesOrderedPriorityLow[k]].Random();
					}
				}
			}
			else
			{
				tank = nonMaxedTanks[tankRarity].Random();
			}
			if (num3 > 0 && num > 0)
			{
				for (int l = 0; l != raritiesOrderedPriorityLow.Length; l++)
				{
					if (tanksWithNoCards[raritiesOrderedPriorityLow[l]].Count > 0)
					{
						tank = tanksWithNoCards[raritiesOrderedPriorityLow[l]].Random();
						tanksWithNoCards[raritiesOrderedPriorityLow[l]].Remove(tank);
						num3--;
						num--;
						break;
					}
				}
			}
			else
			{
				bool flag2 = false;
				for (int m = 0; m != raritiesOrderedPriorityLow.Length; m++)
				{
					if (tanksWithOwnedCards[raritiesOrderedPriorityLow[m]].Count > 0)
					{
						tank = tanksWithOwnedCards[raritiesOrderedPriorityLow[m]].Random();
						tanksWithOwnedCards[raritiesOrderedPriorityLow[m]].Remove(tank);
						flag2 = true;
						break;
					}
				}
				if (!flag2)
				{
					chestRewards.coins += GetTankCardCoinValue(tankRarity) * chest.GetDrop(tankRarity).stackSize.Random();
					continue;
				}
			}
			if (tank == null)
			{
				chestRewards.coins += GetTankCardCoinValue(tankRarity) * chest.GetDrop(tankRarity).stackSize.Random();
				continue;
			}
			nonMaxedTanks[tankRarity].Remove(tank);
			int tankCardCount = PlayerDataManager.GetTankCardCount(tank);
			int num4 = (tankCardCount == 0) ? 1 : Mathf.RoundToInt(chest.GetDrop(tankRarity).stackSize.Random());
			if (flag)
			{
				int num5 = 1;
				for (int n = 0; n != instance.tanks.Length; n++)
				{
					if (instance.tanks[n].rarity == tankRarity)
					{
						num5 = instance.tanks[n].coinValue;
					}
				}
				num4 = Mathf.Max(1, Mathf.FloorToInt((float)(num4 * num5) / (float)tank.coinValue));
			}
			Card card = tank.CreateCards(num4);
			card.isNew = (tankCardCount == 0);
			bool flag3 = false;
			for (int num6 = 0; num6 != chestRewards.cards.Count; num6++)
			{
				if (chestRewards.cards[num6].type == CardType.TankCard && chestRewards.cards[num6].id.Equals(card.id))
				{
					chestRewards.cards[num6].count += card.count;
					flag3 = true;
				}
			}
			if (!flag3)
			{
				chestRewards.cards.Add(card);
			}
		}
		return chestRewards;
	}

	private ChestRewards GenerateBoosterRewards(ChestGenerationSettings chest, List<Tank> newTanks = null)
	{
		ChestRewards chestRewards = new ChestRewards();
		int num = chest.boosterCardCount.Random();
		List<Booster> boostersForOwnedTanks = PlayerDataManager.GetBoostersForOwnedTanks();
		if (newTanks != null && newTanks.Count > 0)
		{
			foreach (Tank newTank in newTanks)
			{
				boostersForOwnedTanks.AddRange(PlayerDataManager.GetTankBoosters(newTank));
			}
		}
		boostersForOwnedTanks.Shuffle();
		for (int i = 0; i != num; i++)
		{
			if (i > boostersForOwnedTanks.Count || boostersForOwnedTanks.Count == 0)
			{
				chestRewards.coins += GetRandomBooster().coinValue;
				continue;
			}
			Booster booster = boostersForOwnedTanks.Random();
			if (UnityEngine.Random.value < chest.newBoosterCardProbability)
			{
				if (boostersForOwnedTanks.Count > 0)
				{
					foreach (Booster item in boostersForOwnedTanks)
					{
						if (item.Count <= 0)
						{
							booster = item;
							boostersForOwnedTanks.Remove(item);
							break;
						}
					}
				}
			}
			else
			{
				List<Booster> list = new List<Booster>();
				foreach (Booster item2 in boostersForOwnedTanks)
				{
					if (item2.Count > 0)
					{
						list.Add(item2);
					}
				}
				if (list.Count > 0)
				{
					booster = list.Random();
				}
			}
			boostersForOwnedTanks.Remove(booster);
			int stackSize = chest.boosterStackSize.Random();
			Card card = booster.CreateCards(stackSize);
			card.isNew = (booster.Count == 0);
			bool flag = false;
			for (int j = 0; j != chestRewards.cards.Count; j++)
			{
				if (chestRewards.cards[j].id.Equals(card.id))
				{
					chestRewards.cards[j].count += card.count;
					flag = true;
				}
			}
			if (!flag)
			{
				chestRewards.cards.Add(card);
			}
		}
		return chestRewards;
	}

	public int GetChestPointsNeeded(ChestProgressionType type)
	{
		ChestProgression[] array = this.chestProgression;
		foreach (ChestProgression chestProgression in array)
		{
			if (chestProgression.type.Equals(type))
			{
				return chestProgression.pointsToOpen;
			}
		}
		return -1;
	}

	public ChestStruct GetChest(Rarity rarity)
	{
		return chests[(int)rarity];
	}

	public Rarity[] GetRaritiesOrderedPriorityLow(Rarity input)
	{
		switch (input)
		{
		default:
			return new Rarity[3]
			{
				Rarity.Common,
				Rarity.Rare,
				Rarity.Epic
			};
		case Rarity.Rare:
			return new Rarity[3]
			{
				Rarity.Rare,
				Rarity.Common,
				Rarity.Epic
			};
		case Rarity.Epic:
			return new Rarity[3]
			{
				Rarity.Epic,
				Rarity.Rare,
				Rarity.Common
			};
		}
	}

	public Rarity[] GetRaritiesOrderedPriorityHigh(Rarity input)
	{
		switch (input)
		{
		default:
			return new Rarity[3]
			{
				Rarity.Common,
				Rarity.Rare,
				Rarity.Epic
			};
		case Rarity.Rare:
			return new Rarity[3]
			{
				Rarity.Rare,
				Rarity.Epic,
				Rarity.Common
			};
		case Rarity.Epic:
			return new Rarity[3]
			{
				Rarity.Epic,
				Rarity.Rare,
				Rarity.Common
			};
		}
	}

	public int GetWinStreakBonus(int wins)
	{
		return winStreakBonuses[Mathf.Clamp(wins - 1, 0, winStreakBonuses.Length - 1)];
	}
}
