using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(LineRenderer))]
public class AimGuide : MonoBehaviour
{
    [Header("Guide")]
    [SerializeField] private Transform playerVisual;
    [SerializeField] private float guideLength = 4.8f;
    [SerializeField] private float dotSize = 0.08f;
    [SerializeField] private float dotSpacing = 0.3f;
    [SerializeField] private float startOffset = 0.9f;
    [SerializeField, Range(0f, 1f)] private float opacity = 0.72f;
    [SerializeField] private Color guideColor =
        new Color(0.25f, 0.85f, 1f, 1f);

    private LineRenderer lineRenderer;
    private Material guideMaterial;
    private Texture2D dotTexture;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();

        if (playerVisual == null)
            playerVisual = transform.root.Find("PlayerVisual");

        if (playerVisual == null)
        {
            lineRenderer.enabled = false;
            enabled = false;
            return;
        }

        ConfigureLine();
        UpdateLine();
    }

    private void LateUpdate()
    {
        if (playerVisual == null)
        {
            lineRenderer.enabled = false;
            return;
        }

        UpdateLine();
    }

    private void ConfigureLine()
    {
        lineRenderer.positionCount = 2;
        lineRenderer.useWorldSpace = true;
        lineRenderer.loop = false;
        lineRenderer.startWidth = dotSize;
        lineRenderer.endWidth = dotSize;
        lineRenderer.numCapVertices = 2;
        lineRenderer.textureMode = LineTextureMode.Tile;
        lineRenderer.shadowCastingMode = ShadowCastingMode.Off;
        lineRenderer.receiveShadows = false;
        lineRenderer.sortingOrder = 11;

        Color visibleColor = guideColor;
        visibleColor.a = Mathf.Clamp01(opacity);
        lineRenderer.startColor = visibleColor;
        lineRenderer.endColor = visibleColor;

        Shader shader = Shader.Find("Sprites/Default");

        if (shader == null)
        {
            lineRenderer.enabled = false;
            Debug.LogWarning(
                "AimGuide: Sprites/Default shader was not found.",
                this
            );
            return;
        }

        float safeSpacing = Mathf.Max(0.01f, dotSpacing);
        float safeDotSize = Mathf.Clamp(
            dotSize,
            0.005f,
            safeSpacing * 0.95f
        );

        lineRenderer.startWidth = safeDotSize;
        lineRenderer.endWidth = safeDotSize;

        const int textureSize = 64;
        dotTexture = new Texture2D(
            textureSize,
            textureSize,
            TextureFormat.RGBA32,
            false
        )
        {
            name = "Aim Guide Dot Texture",
            wrapMode = TextureWrapMode.Repeat,
            filterMode = FilterMode.Bilinear
        };

        float dotWidthFraction =
            safeDotSize / safeSpacing;
        Color[] pixels = new Color[textureSize * textureSize];

        for (int y = 0; y < textureSize; y++)
        {
            for (int x = 0; x < textureSize; x++)
            {
                float normalizedX =
                    ((x + 0.5f) / textureSize - 0.5f) /
                    (dotWidthFraction * 0.5f);
                float normalizedY =
                    ((y + 0.5f) / textureSize - 0.5f) /
                    0.5f;

                pixels[y * textureSize + x] =
                    normalizedX * normalizedX +
                    normalizedY * normalizedY <= 1f
                        ? Color.white
                        : Color.clear;
            }
        }

        dotTexture.SetPixels(pixels);
        dotTexture.Apply();

        guideMaterial = new Material(shader)
        {
            name = "Aim Guide Material"
        };

        guideMaterial.mainTexture = dotTexture;
        guideMaterial.mainTextureScale = new Vector2(
            Mathf.Max(0f, guideLength) / safeSpacing,
            1f
        );

        lineRenderer.sharedMaterial = guideMaterial;
    }

    private void UpdateLine()
    {
        if (guideMaterial == null)
        {
            lineRenderer.enabled = false;
            return;
        }

        Vector3 direction = playerVisual.up.normalized;
        Vector3 start = playerVisual.position +
                        direction * Mathf.Max(0f, startOffset);

        lineRenderer.enabled = true;
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(
            1,
            start + direction * Mathf.Max(0f, guideLength)
        );
    }

    private void OnDestroy()
    {
        if (guideMaterial != null)
            Destroy(guideMaterial);

        if (dotTexture != null)
            Destroy(dotTexture);
    }
}
