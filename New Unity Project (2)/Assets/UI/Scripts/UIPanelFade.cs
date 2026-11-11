using UnityEngine;

// Fades and slightly zooms a menu panel in every time it is shown.
[RequireComponent(typeof(CanvasGroup))]
public class UIPanelFade : MonoBehaviour
{
    public float duration = 0.25f;

    CanvasGroup group;
    float t;

    void Awake() { group = GetComponent<CanvasGroup>(); }

    void OnEnable()
    {
        t = 0f;
        Apply(0f);
    }

    void Update()
    {
        if (t >= 1f) return;
        t = Mathf.Min(1f, t + Time.unscaledDeltaTime / duration);
        Apply(1f - Mathf.Pow(1f - t, 3f));
    }

    void Apply(float k)
    {
        group.alpha = k;
        transform.localScale = Vector3.one * Mathf.Lerp(0.96f, 1f, k);
    }
}
