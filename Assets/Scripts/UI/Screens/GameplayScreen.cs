using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public class GameplayScreen : ScreenBase
{
    [SerializeField] AssetReferenceGameObject counterPrefab;
    [SerializeField] Transform countersContainer;
    [SerializeField] Transform playerCounterContainer;

    [SerializeField] Button continueButton;
    [SerializeField] GameObject draw;

    [SerializeField] GameObject timer;
    [SerializeField] TextMeshProUGUI timerText;
    [SerializeField] TweenSettings timerTweenSettings;
    [SerializeField] TweenSettings sortTweeenSettings;
    [SerializeField] float spacing = 10f;

    Dictionary<IMoneyCollector, MoneyCounterView> activeMoneyCounters = new();
    Sequence activeSortSequence;

    public ITimerVisual TimerVisual { get; private set; }

    private void Awake()
    {
        TimerVisual = new MinutesTextTimerVisual(timerText, timerTweenSettings, Vector3.one, Vector3.one * 1.3f);
        continueButton.onClick.AddListener(() => ServiceLocator.Get<IStateSwitcher<GameStateBase>>().SetState<MenuState>());
    }

    public async UniTask SetupCounters(List<CompetitorProfile> profiles, MatchScoreModel scoreModel)
    {
        CancellationToken token = this.GetCancellationTokenOnDestroy();

        foreach (var item in activeMoneyCounters)
        {
            Destroy(item.Value.gameObject);
        }
        activeMoneyCounters.Clear();

        await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate, cancellationToken: token);

        List<IMoneyCollector> initialOrder = new();

        foreach (var profile in profiles)
        {
            var container = profile.IsHuman ? playerCounterContainer : countersContainer;
            var counterInstance = await counterPrefab
                .InstantiateAsync(container).ToUniTask(cancellationToken: token);

            var rect = counterInstance.transform as RectTransform;

            if (profile.IsHuman)
            {
                rect.SetAnchor(AnchorPreset.MiddleCenter);
                rect.anchoredPosition = Vector2.zero;
            }
            else rect.SetAnchor(AnchorPreset.TopCenter);

            var counterView = counterInstance.GetComponent<MoneyCounterView>();
            activeMoneyCounters[profile.Car] = counterView;
            counterView.Bind(profile, scoreModel);

            initialOrder.Add(profile.Car);
        }

        ApplyLayout(initialOrder, false);

        scoreModel.OnModelChanged += SortCounters;
    }

    void SortCounters(Dictionary<IMoneyCollector, int> scores)
    {
        var sortedCollectors = scores.OrderByDescending(kvp => kvp.Value).Select(kvp => kvp.Key).ToList();

        ApplyLayout(sortedCollectors, true);
    }

    void ApplyLayout(List<IMoneyCollector> orderedCollectors, bool animate)
    {
        if (animate)
        {
            activeSortSequence?.Kill();
            activeSortSequence = DOTween.Sequence();
        }

        float currentY = 0f;

        for (int i = 0; i < orderedCollectors.Count; i++)
        {
            if (activeMoneyCounters.TryGetValue(orderedCollectors[i], out var view))
            {
                var rect = GetNodeToSort(view);

                rect.SetSiblingIndex(i);
                Vector2 targetPos = new Vector2(rect.anchoredPosition.x, -currentY);

                if (animate)
                {
                    rect.DOKill();
                    activeSortSequence.Join(rect.DOAnchorPos(targetPos, sortTweeenSettings.duration).SetEase(sortTweeenSettings.ease));
                }
                else rect.anchoredPosition = targetPos;
                
                currentY += rect.rect.height + spacing;
            }
        }
    }

    RectTransform GetNodeToSort(MoneyCounterView view)
    {
        return view.transform.parent == playerCounterContainer
            ? playerCounterContainer.transform as RectTransform
            : view.transform as RectTransform;
    }

    public void SetTimerActive(bool active) => timer.SetActive(active);
    public void SetContinueButtonActive(bool active) => continueButton.gameObject.SetActive(active);
    public void SetDrawActive(bool active) => draw.SetActive(active);

    public void ShowWinners(List<IMoneyCollector> winners)
    {
        foreach (var item in winners)
        {
            activeMoneyCounters[item].SetWinnerView();
        }
    }
}