using UnityEngine;

public class PlayerFeedbackVisualizer : MonoBehaviour
{
    IMoneyCollector targetCollector;
    Transform targetTransform;

    VisualsConfig visualsConfig => ServiceLocator.Get<VisualsConfig>();

    public void Init(IMoneyCollector collector, Transform carTransform)
    {
        if (targetCollector != null)
        {
            targetCollector.OnMoneyCollected -= SpawnFloatingText;
        }

        targetCollector = collector;
        targetTransform = carTransform;

        targetCollector.OnMoneyCollected += SpawnFloatingText;
    }

    private void SpawnFloatingText(IMoneyCollector collector, int amount)
    {
        if (targetTransform == null) return;

        Vector3 spawnPos = targetTransform.position + Vector3.up * 1.5f;

        var textObj = ObjectsPool.GetInstance(visualsConfig.FloatingTextRef, spawnPos, true);

        if (textObj.TryGetComponent(out FloatingTextEffect floatingText))
        {
            floatingText.Init(amount);
        }
    }

    private void OnDestroy()
    {
        if (targetCollector != null)
        {
            targetCollector.OnMoneyCollected -= SpawnFloatingText;
        }
    }
}