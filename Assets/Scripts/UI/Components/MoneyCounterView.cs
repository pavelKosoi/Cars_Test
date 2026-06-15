using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MoneyCounterView : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI moneyText;
    [SerializeField] Image image;
    [SerializeField] GameObject winnerView;

    MatchScoreModel model;
    IMoneyCollector myCollector;

    public void Bind(CompetitorProfile profile, MatchScoreModel moneyModel)
    {
        this.myCollector = profile.Car;
        this.model = moneyModel;

        image.color = profile.CarSetings.color;
        
        UpdateScoreText(model.GetScore(myCollector));
        model.OnScoreChanged += HandleScoreChanged;
    }

    void HandleScoreChanged(IMoneyCollector collector, int newScore)
    {
        if (collector == myCollector) UpdateScoreText(newScore);
    }

    void UpdateScoreText(int money)
    {
        moneyText.text = $"${money}K";
    }

    public void SetWinnerView()
    {
        winnerView.SetActive(true);
    }

    private void OnDestroy()
    {
        if (model != null)
        {
            model.OnScoreChanged -= HandleScoreChanged;
        }
    }
}