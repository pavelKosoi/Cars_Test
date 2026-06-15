using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public class GameplayScreen : ScreenBase
{
    [SerializeField] AssetReferenceGameObject counterPrefab;
    [SerializeField] Transform countersContainer;

    [SerializeField] Button continueButton;
    [SerializeField] GameObject draw;

    [SerializeField] GameObject timer;
    [SerializeField] TextMeshProUGUI timerText;
    [SerializeField] TweenSettings timerTweenSettings;

    Dictionary<IMoneyCollector, MoneyCounterView> activeMoneyCounters = new();
    public ITimerVisual TimerVisual {  get; private set; }

    private void Awake()
    {
        TimerVisual = new MinutesTextTimerVisual(timerText, timerTweenSettings, Vector3.one, Vector3.one * 1.3f);
        continueButton.onClick.AddListener(() => GeneralGameManager.Instance.SetState<MenuState>());
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

        foreach (var profile in profiles)
        {         
            var counterInstance = await counterPrefab
                .InstantiateAsync(countersContainer).ToUniTask(cancellationToken: token);

            var counterView = counterInstance.GetComponent<MoneyCounterView>();
            activeMoneyCounters[profile.Car] = counterView;
            counterView.Bind(profile, scoreModel);
        }
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