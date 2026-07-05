using TMPro;
using UnityEngine;

public class MedkitCounterUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI counterText;
    private int count = 0;

    void Awake()
    {
        if (counterText == null)
            counterText = GetComponent<TextMeshProUGUI>();

        UpdateUI();
    }

    public void AddMedkit(int amount = 1)
    {
        count += amount;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (counterText != null)
            counterText.text = count.ToString();
    }
}
