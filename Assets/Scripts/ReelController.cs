using System.Collections;
using System.Linq;
using UnityEngine;


public class ReelController : MonoBehaviour
{
    [Header("Available symbols (use the same order on every reel)")]
    [Tooltip("Recommended order: Seven, Cherries, Bell, BAR")]
    [SerializeField] private Sprite[] availableSymbols;

    [Header("Animation")]
    [SerializeField, Min(0.1f)] private float spinSpeed = 7.5f;
    [SerializeField] private float wrapTop = 1.5f;
    [SerializeField] private float wrapBottom = -1.5f;
    [SerializeField, Min(0.05f)] private float settleDuration = 0.3f;

    private SpriteRenderer[] symbolRenderers;

    
    public int CenterSymbolIndex { get; private set; } = -1;
    public bool IsSpinning { get; private set; }

    private void Awake()
    {
        
        symbolRenderers = GetComponentsInChildren<SpriteRenderer>(true);

        if (symbolRenderers.Length != 3)
        {
            Debug.LogError($"{name} must have exactly three child SpriteRenderers (TopSymbol, CenterSymbol, BottomSymbol). Found {symbolRenderers.Length}.", this);
        }

        if (availableSymbols == null || availableSymbols.Length < 2 || availableSymbols.Any(sprite => sprite == null))
        {
            Debug.LogError($"Assign all available symbol sprites in the Inspector on {name}.", this);
        }
    }


    public IEnumerator SpinFor(float duration)
    {
        if (IsSpinning || symbolRenderers == null || symbolRenderers.Length != 3 ||
            availableSymbols == null || availableSymbols.Length < 2 || availableSymbols.Any(sprite => sprite == null))
        {
            yield break;
        }

        IsSpinning = true;
        float span = wrapTop - wrapBottom;
        if (span <= 0f)
        {
            Debug.LogError($"Wrap Top must be greater than Wrap Bottom on {name}.", this);
            IsSpinning = false;
            yield break;
        }

        float settleTime = Mathf.Max(0.05f, settleDuration);
        float movingTime = Mathf.Max(0.1f, duration - settleTime);
        float elapsed = 0f;

   
        while (elapsed < movingTime)
        {
            float delta = Time.deltaTime;
            foreach (SpriteRenderer symbol in symbolRenderers)
            {
                Vector3 position = symbol.transform.localPosition;
                position.y -= spinSpeed * delta;

                bool wrapped = false;
                while (position.y < wrapBottom)
                {
                    position.y += span;
                    wrapped = true;
                }

                symbol.transform.localPosition = position;
                if (wrapped)
                {
                    symbol.sprite = availableSymbols[Random.Range(0, availableSymbols.Length)];
                }
            }

            elapsed += delta;
            yield return null;
        }

        
        SpriteRenderer[] ordered = symbolRenderers
            .OrderByDescending(renderer => renderer.transform.localPosition.y)
            .ToArray();

        float[] startY = ordered.Select(renderer => renderer.transform.localPosition.y).ToArray();
        elapsed = 0f;
        while (elapsed < settleTime)
        {
            float t = Mathf.Clamp01(elapsed / settleTime);
            t = t * t * (3f - 2f * t); 

            for (int i = 0; i < ordered.Length; i++)
            {
                Transform symbolTransform = ordered[i].transform;
                Vector3 position = symbolTransform.localPosition;
                position.y = Mathf.Lerp(startY[i], 1f - i, t);
                symbolTransform.localPosition = position;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        
        for (int i = 0; i < ordered.Length; i++)
        {
            Transform symbolTransform = ordered[i].transform;
            Vector3 position = symbolTransform.localPosition;
            position.y = 1f - i; 
            symbolTransform.localPosition = position;

            int symbolIndex = Random.Range(0, availableSymbols.Length);
            ordered[i].sprite = availableSymbols[symbolIndex];
            if (i == 1)
            {
                CenterSymbolIndex = symbolIndex;
            }
        }

        IsSpinning = false;
    }
}
