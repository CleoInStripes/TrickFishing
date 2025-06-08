using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ImageCoverFitter : MonoBehaviour
{
    private RectTransform imageRect;
    private Image image;

    [Tooltip("Optional container. If empty, will use parent.")]
    public RectTransform container;

    void Awake()
    {
        imageRect = GetComponent<RectTransform>();
        image = GetComponent<Image>();
        if (container == null)
            container = transform.parent as RectTransform;
    }

    void LateUpdate()
    {
        if (image.sprite == null || container == null) return;

        // Get sizes
        float imageAspect = (float)image.sprite.texture.width / image.sprite.texture.height;
        float containerAspect = container.rect.width / container.rect.height;

        float width = 0f;
        float height = 0f;

        if (imageAspect > containerAspect)
        {
            // Image is wider: match height
            height = container.rect.height;
            width = height * imageAspect;
        }
        else
        {
            // Image is taller: match width
            width = container.rect.width;
            height = width / imageAspect;
        }

        imageRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
        imageRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        imageRect.anchoredPosition = Vector2.zero;
    }
}