using UnityEngine;

// Gently bobs and tilts a UI element (used for the hero on the main menu).
public class UIFloat : MonoBehaviour
{
    public float amplitude = 18f;
    public float speed = 1.4f;
    public float tilt = 3f;

    RectTransform rt;
    Vector2 basePos;

    void Awake()
    {
        rt = (RectTransform)transform;
        basePos = rt.anchoredPosition;
    }

    void Update()
    {
        float t = Time.unscaledTime * speed;
        rt.anchoredPosition = basePos + new Vector2(0f, Mathf.Sin(t) * amplitude);
        rt.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(t * 0.8f) * tilt);
    }
}
