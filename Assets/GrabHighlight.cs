using UnityEngine;

public class TargetBoxHighlight : MonoBehaviour
{
    public Material highlightMaterial;
    public float blinkSpeed = 2f;

    private Renderer boxRenderer;
    private Material originalMaterial;

    void Start()
    {
        boxRenderer = GetComponent<Renderer>();

        if (boxRenderer == null || highlightMaterial == null)
        {
            Debug.LogWarning("TargetBoxHighlight missing Renderer or Highlight Material on " + gameObject.name);
            return;
        }

        originalMaterial = boxRenderer.material;
    }

    void Update()
    {
        if (boxRenderer == null || highlightMaterial == null) return;

        float t = Mathf.PingPong(Time.time * blinkSpeed, 1f);
        boxRenderer.material = t > 0.5f ? highlightMaterial : originalMaterial;
    }
}