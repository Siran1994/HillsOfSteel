using System;

[Serializable]
public class TransactionCost
{
	public CurrencyType currency;

	public int amount;

	public TransactionCost(CurrencyType type, int cost)
	{
		currency = type;
		amount = cost;
	}
}
