using System;
using System.Collections.Generic;

[Serializable]
public class ChestRewards
{
	public List<Card> cards;

	public int coins;

	public int gems;

	public ChestRewards()
	{
		cards = new List<Card>();
		coins = 0;
		gems = 0;
	}

	public static ChestRewards operator +(ChestRewards lhs, ChestRewards rhs)
	{
		lhs.cards.AddRange(rhs.cards);
		lhs.coins += rhs.coins;
		lhs.gems += rhs.gems;
		return lhs;
	}
}
