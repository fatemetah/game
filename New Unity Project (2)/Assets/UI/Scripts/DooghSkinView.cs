using UnityEngine;

public class DooghSkinView : MonoBehaviour
{
    SpriteRenderer body, left, right;
    Sprite originalBody;
    Vector3 originalScale;
    Material originalMaterial, flapMaterial;

    void Awake()
    {
        body = GetComponent<SpriteRenderer>();
        left = transform.Find("LeftWing").GetComponent<SpriteRenderer>();
        right = transform.Find("RightWing").GetComponent<SpriteRenderer>();
        originalBody = body.sprite;
        originalScale = transform.localScale;
        originalMaterial = body.sharedMaterial;
        flapMaterial = new Material(Resources.Load<Shader>("DooghSkins/FeatherFlap"));
    }

    void OnEnable() { DooghSkins.Changed += Apply; Apply(); }
    void OnDisable() { DooghSkins.Changed -= Apply; }
    void OnDestroy() { Destroy(flapMaterial); }

    void Apply()
    {
        string id = DooghSkins.Current;
        bool compact = id == "Ayran" || id == "Milk" || id == "Juice" || id == "Persian";
        transform.localScale = compact ? originalScale * 0.85f : originalScale;
        bool classic = id == "Classic";
        body.sprite = classic ? originalBody : DooghSkins.Art(id);
        left.enabled = right.enabled = classic;
        body.sharedMaterial = classic ? originalMaterial : flapMaterial;
        if (classic) return;

        // UV coordinates of the stationary body, including its widest outline.
        Vector4 roots = new Vector4(.27f, .74f, 0, 0);
        Vector4 band = new Vector4(.06f, .96f, 0, 0);
        switch (id)
        {
            case "Persian": roots = new Vector4(.19f, .81f, 0, 0); break;
            case "Ayran":
                roots = new Vector4(.265f, .72f, 0, 0);
                // Keep the copper handle fixed above the right wing.
                band = new Vector4(.10f, .40f, .20f, 0);
                break;
            case "Water":
            case "Juice":
            case "Milk": roots = new Vector4(.24f, .76f, 0, 0); break;
        }
        flapMaterial.SetVector("_Roots", roots);
        flapMaterial.SetVector("_Band", band);
    }
}
