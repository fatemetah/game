using UnityEngine;

// The game was designed for a 9:16 screen. On taller phones (19.5:9, 20:9 ...) a normal camera
// keeps the height and cuts the sides, so the game looks zoomed in and the doogh can leave the screen.
// This keeps the full width visible and adds the extra space at the top (the bottom stays where it was).
[DefaultExecutionOrder(-1000)]
[RequireComponent(typeof(Camera))]
public class FitCameraWidth : MonoBehaviour
{
    public float designAspect = 9f / 16f;

    void Awake()
    {
        var cam = GetComponent<Camera>();
        if (!cam.orthographic) return;
        float baseSize = cam.orthographicSize;
        float aspect = (float)Screen.width / Screen.height;
        if (aspect >= designAspect) return;
        float newSize = baseSize * designAspect / aspect;
        Vector3 p = transform.position;
        p.y += newSize - baseSize;   // keep the bottom edge in the same place
        transform.position = p;
        cam.orthographicSize = newSize;
    }
}
