using UnityEngine;

// Scales the background picture so it always fills the whole screen (any phone size),
// keeping the bottom of the picture (the table) at the bottom of the screen.
[ExecuteAlways]
[RequireComponent(typeof(SpriteRenderer))]
public class FitBackgroundToCamera : MonoBehaviour
{
    void Start() { Fit(); }

#if UNITY_EDITOR
    void Update() { if (!Application.isPlaying) Fit(); }
#endif

    void Fit()
    {
        Camera cam = Camera.main;
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (cam == null || sr == null || sr.sprite == null || !cam.orthographic) return;

        float viewH = cam.orthographicSize * 2f;
        float viewW = viewH * cam.aspect;
        Vector2 size = sr.sprite.bounds.size;
        float k = Mathf.Max(viewW / size.x, viewH / size.y);
        transform.localScale = new Vector3(k, k, 1f);

        Vector3 c = cam.transform.position;
        float bottom = c.y - viewH / 2f;
        float y = bottom + size.y * k / 2f;   // picture's bottom edge on the screen's bottom edge
        transform.position = new Vector3(c.x, y, transform.position.z);
    }
}
