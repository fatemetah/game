using UnityEngine;

// Moves the doogh left/right.
// Phone: put a finger anywhere and slide it left or right; the doogh follows the slide
//        (no jumping when you lift the finger and touch somewhere else).
// Computer: the doogh follows the mouse.
public class Harkat : MonoBehaviour
{
    public float minX = -3.8f;
    public float maxX = 3.8f;

    float lastX;
    bool dragging;

    void Start()
    {
        lastX = PointerX();
        dragging = !Application.isMobilePlatform;   // on a computer, always follow the mouse
    }

    void Update()
    {
        if (Time.timeScale == 0f) return;   // paused / game over

        if (Application.isMobilePlatform)
        {
            if (Input.GetMouseButtonDown(0)) { lastX = PointerX(); dragging = !PauseMenu.PointerOverUI(); }
            if (!Input.GetMouseButton(0)) { dragging = false; return; }
        }

        if (!dragging) return;

        float x = PointerX();
        Vector3 p = transform.position;
        p.x = Mathf.Clamp(p.x + (x - lastX), minX, maxX);
        transform.position = p;
        lastX = x;
    }

    static float PointerX()
    {
        Camera cam = Camera.main;
        return cam != null ? cam.ScreenToWorldPoint(Input.mousePosition).x : 0f;
    }
}
