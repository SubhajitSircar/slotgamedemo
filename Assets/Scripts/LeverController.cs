
using System.Collections;
using UnityEngine;

public class LeverController : MonoBehaviour
{
    [Header("Handle Sprites")]
    [SerializeField] private SpriteRenderer raisedHandle;
    [SerializeField] private SpriteRenderer pulledHandle;

    [Header("Animation")]
    [SerializeField, Min(0.05f)]
    private float pulledDuration = 0.25f;

    private bool isPulling;

    private void Awake()
    {
        ShowRestPosition();
    }

    // Called automatically by SlotMachineManager when a valid bet starts.
    public void Pull()
    {
        if (!isPulling)
            StartCoroutine(PullRoutine());
    }

    private IEnumerator PullRoutine()
    {
        isPulling = true;

        // Keep the shaft enabled. Change only the handle.
        if (raisedHandle != null)
            raisedHandle.enabled = false;

        if (pulledHandle != null)
            pulledHandle.enabled = true;

        yield return new WaitForSeconds(pulledDuration);

        ShowRestPosition();
        isPulling = false;
    }

    private void ShowRestPosition()
    {
        if (raisedHandle != null)
            raisedHandle.enabled = true;

        if (pulledHandle != null)
            pulledHandle.enabled = false;
    }
}
