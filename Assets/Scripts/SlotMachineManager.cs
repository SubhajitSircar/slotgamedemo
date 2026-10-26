
using System.Collections;
using TMPro;
using UnityEngine;

public class SlotMachineManager : MonoBehaviour
{
    [Header("Reels - assign Reel1, Reel2, Reel3 in order")]
    [SerializeField] private ReelController[] reels = new ReelController[3];

    [Header("UI - assign TextMeshPro text components")]
    [SerializeField] private TMP_Text balanceText;
    [SerializeField] private TMP_Text statusText;

    [Header("Game Settings")]
    [SerializeField, Min(1)] private int startingBalance = 500;

    [Tooltip("Symbol order: Seven, Cherries, Bell, BAR")]
    [SerializeField] private int[] payoutMultipliers = { 10, 5, 3, 8 };

    [Header("Lever Animation")]
    [SerializeField] private LeverController leverController;

    private int balance;
    private int currentBet;
    private bool isSpinning;

    private string statusMessage = "Choose a bet to spin.";

    private readonly string[] symbolNames =
    {
        "Seven",
        "Cherries",
        "Bell",
        "BAR"
    };

    private void Start()
    {
        balance = startingBalance;

        // Verify that all three reels have been assigned.
        if (reels == null ||
            reels.Length != 3 ||
            reels[0] == null ||
            reels[1] == null ||
            reels[2] == null)
        {
            Debug.LogError(
                "Assign Reel1, Reel2, and Reel3 to SlotMachineManager.",
                this
            );
        }

        RefreshUI();
    }

    // These methods will be connected to the UI buttons.
    public void Bet10G()
    {
        StartBetAndSpin(10);
    }

    public void Bet50G()
    {
        StartBetAndSpin(50);
    }

    public void Bet100G()
    {
        StartBetAndSpin(100);
    }
    public void PullLever()
    {
        // Use the previous bet, or default to 10G.
        int betToPlay = currentBet > 0 ? currentBet : 10;

        StartBetAndSpin(betToPlay);
    }

    public void StartBetAndSpin(int bet)
    {
        if (isSpinning)
        {
            SetStatus("Please wait for the reels to stop.");
            return;
        }

        if (reels == null ||
            reels.Length != 3 ||
            reels[0] == null ||
            reels[1] == null ||
            reels[2] == null)
        {
            SetStatus("Reel references are missing. Check the Inspector.");
            Debug.LogError(statusMessage, this);
            return;
        }

        if (bet != 10 && bet != 50 && bet != 100)
        {
            SetStatus("Choose a valid bet.");
            return;
        }

        if (balance < bet)
        {
            SetStatus("Not enough credits for that bet.");
            return;
        }

        // Deduct the stake once when the spin begins.
        currentBet = bet;
        balance -= currentBet;
        isSpinning = true;

        // Automatically pull the lever when a valid bet starts.
        if (leverController != null)
        {
            leverController.Pull();
        }

        SetStatus($"Spinning for {currentBet}G...");

        RefreshUI();

        StartCoroutine(SpinAllReels());
    }


    private IEnumerator SpinAllReels()
    {
        float[] durations = { 1.8f, 2.2f, 2.6f };
        float longestDuration = 0f;

        for (int i = 0; i < reels.Length; i++)
        {
            StartCoroutine(reels[i].SpinFor(durations[i]));

            longestDuration = Mathf.Max(
                longestDuration,
                durations[i]
            );
        }

        yield return new WaitForSeconds(longestDuration + 0.1f);

        int first = reels[0].CenterSymbolIndex;
        int second = reels[1].CenterSymbolIndex;
        int third = reels[2].CenterSymbolIndex;

        // A win requires all three center symbols to match.
        if (first >= 0 &&
            first == second &&
            second == third)
        {
            int multiplier = 1;

            if (first < payoutMultipliers.Length)
            {
                multiplier = payoutMultipliers[first];
            }

            int payout = currentBet * multiplier;
            balance += payout;

            string symbolName = "Symbol";

            if (first < symbolNames.Length)
            {
                symbolName = symbolNames[first];
            }

            SetStatus(
                $"WIN! Three {symbolName} symbols! +{payout}G"
            );
        }
        else
        {
            SetStatus($"No win. Bet {currentBet}G lost.");
        }

        isSpinning = false;
        RefreshUI();
    }

    private void SetStatus(string message)
    {
        statusMessage = message;
        RefreshUI();
    }


    private void RefreshUI()
    {
        if (balanceText != null)
        {
            balanceText.text = $"Balance: {balance}G";
        }

        if (statusText != null)
        {
            statusText.text = statusMessage;
        }
    }


    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
