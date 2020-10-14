using System;

[Serializable]
public class Card
{
	public string id;

	public int count;

	public CardType type;

	public Rarity rarity;

	public bool isNew;

	public bool Equals(Card other)
	{
		return id.Equals(other.id);
	}
}
