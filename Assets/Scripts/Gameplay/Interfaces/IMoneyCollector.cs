using System;

public interface IMoneyCollector
{
    public event Action<IMoneyCollector, int> OnMoneyCollected;
}