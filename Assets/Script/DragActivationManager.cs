using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class DragActivationManager : MonoBehaviour
{
   

    [Header("Populate manually (drag & drop objects)")]
    [Tooltip("Drag the draggable GameObjects (those with UIDragToWorld_NoJump) here in inspector.")]
    public List<GameObject> draggables = new List<GameObject>();

    [Header("How many start active (random)")]
    [Tooltip("Number of draggables to enable at start (random selection).")]
    public int initialActiveCount = 3;

    [Header("Events")]
    public UnityEvent onAllSnapped; // invoked when all draggables have snapped

    // internals
    private HashSet<int> activatedIndices = new HashSet<int>(); // indices that were activated (so we don't activate same twice)
    private int snappedCount = 0;
    private System.Random rnd = new System.Random();
[Header("Drop Areas (Populate manually)")]
public List<RectTransform> allWorldDropAreas = new List<RectTransform>();

    void Awake()
    {
      
    }

    void Start()
    {
        if (draggables == null || draggables.Count == 0)
        {
            Debug.LogWarning("[DragActivationManager] No draggables assigned in inspector.");
            return;
        }

        initialActiveCount = Mathf.Clamp(initialActiveCount, 0, draggables.Count);

        // Disable all draggables first
        for (int i = 0; i < draggables.Count; i++)
        {
            var go = draggables[i];
            if (go == null) continue;
            // Ensure each draggable is inactive until selected (we assume your draggable has logic to reset state on activation if needed)
            go.SetActive(false);
        }

        // Randomly activate initialActiveCount distinct draggables
        int activated = 0;
        while (activated < initialActiveCount)
        {
            int idx = rnd.Next(draggables.Count);
            if (activatedIndices.Contains(idx)) continue;
            var go = draggables[idx];
            if (go == null) { activatedIndices.Add(idx); continue; } // reserve index even if null
            go.SetActive(true);
            activatedIndices.Add(idx);
            activated++;
        }

        snappedCount = 0;
    }

    /// <summary>
    /// Call this when a draggable has snapped & deactivated itself.
    /// The draggable GameObject is passed (it will likely be inactive already).
    /// Manager will activate one more random remaining draggable (if any).
    /// </summary>
    /// <param name="snappedObject">The GameObject that snapped</param>
    public void NotifySnapped(GameObject snappedObject)
    {
        if (snappedObject == null) return;

        snappedCount++;
        Debug.Log($"[DragActivationManager] Snapped: {snappedObject.name} ({snappedCount}/{draggables.Count})");

        // activate one more if available
        ActivateOneMore();

        // if all snapped, invoke event & debug
        if (snappedCount >= draggables.Count)
        {
            Debug.Log("[DragActivationManager] ✅ All draggables have been snapped to world space!");
            onAllSnapped?.Invoke();
        }
    }

    private void ActivateOneMore()
    {
        // build list of candidate indices that are not yet activated and not null
        List<int> candidates = new List<int>();
        for (int i = 0; i < draggables.Count; i++)
        {
            if (activatedIndices.Contains(i)) continue;           // already activated earlier
            var go = draggables[i];
            if (go == null) continue;
            // if object already snapped and deactivated, treat it as activated (skip)
            if (!go.activeSelf) continue;
            // Otherwise candidate
            candidates.Add(i);
        }

        // NOTE: Because snapped objects deactivate themselves, we can't reliably detect snapped by activeSelf alone.
        // So instead consider any index not in activatedIndices and whose GameObject is not null as candidate.
        if (candidates.Count == 0)
        {
            // Fallback: collect indices that are not in activatedIndices
            candidates.Clear();
            for (int i = 0; i < draggables.Count; i++)
            {
                if (!activatedIndices.Contains(i) && draggables[i] != null)
                    candidates.Add(i);
            }
        }

        if (candidates.Count == 0)
        {
            // nothing left to activate
            return;
        }

        int pick = candidates[rnd.Next(candidates.Count)];
        activatedIndices.Add(pick);

        var toActivate = draggables[pick];
        if (toActivate != null)
        {
            toActivate.SetActive(true);

            // optional: if your draggable requires resetting its internal state when re-enabled,
            // ensure that the component's Awake/OnEnable handles that. If not, you can call a reset API on it here.
            Debug.Log($"[DragActivationManager] Activated {toActivate.name}");
        }
    }

    public void EnableExclusiveDropArea(RectTransform activeArea)
{
    if (allWorldDropAreas == null || allWorldDropAreas.Count == 0)
        return;

    foreach (var area in allWorldDropAreas)
    {
        if (area == null) continue;
        var img = area.GetComponent<UnityEngine.UI.Image>();
        if (img == null) continue;

        img.raycastTarget = (area == activeArea);
    }
}

public void RestoreAllDropAreas()
{
    if (allWorldDropAreas == null) return;

    foreach (var area in allWorldDropAreas)
    {
        if (area == null) continue;
        var img = area.GetComponent<UnityEngine.UI.Image>();
        if (img == null) continue;

        img.raycastTarget = true;
    }
}

}
