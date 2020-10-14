using System;
using UnityEngine;

[Serializable]
public class Collectible
{
	public string name;

	public string id;

	public CardType cardType;

	public Rarity rarity;

	public Sprite fullSprite;

	public Sprite bigCard;

	public Sprite card;

	public int coinValue;

	public Card CreateCards(int stackSize)
	{
		return new Card
		{
			id = (string.IsNullOrEmpty(id) ? name : id),
			count = stackSize,
			type = cardType,
			rarity = rarity
		};
	}
}
