using UnityEngine;
using UnityEngine.EventSystems;

// Small "juice" for buttons: squishes when pressed, optional idle pulse.
public class UIButtonFX : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    public bool pulse = false;
    [Range(0.8f, 1f)] public float pressScale = 0.92f;

    Vector3 baseScale = Vector3.one;
    float current = 1f, target = 1f;

    void Awake() { baseScale = transform.localScale; }

    void OnEnable()
    {
        current = target = 1f;
        transform.localScale = baseScale;
    }

    void Update()
    {
        current = Mathf.Lerp(current, target, 1f - Mathf.Exp(-25f * Time.unscaledDeltaTime));
        float p = pulse ? 1f + 0.035f * Mathf.Sin(Time.unscaledTime * 3f) : 1f;
        transform.localScale = baseScale * current * p;
    }

    public void OnPointerDown(PointerEventData e) { target = pressScale; }
    public void OnPointerUp(PointerEventData e) { target = 1f; }
    public void OnPointerExit(PointerEventData e) { target = 1f; }
}
