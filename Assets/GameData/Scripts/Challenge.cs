using System;

[Serializable]
public class Challenge
{
	public string name;

	public string facebookId;

	public string tankId;

	public int skin;

	public int gunLevel;

	public int engineLevel;

	public int armorLevel;

	public int tankLevel = -1;

	public string boosterId;

	public int boosterCount;

	public int boosterLevel;

	public int rating;

	public int actualRating;

	public string countryCode;

	public bool isVip;
}
