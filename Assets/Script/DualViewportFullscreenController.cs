using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class DualViewportFullscreenController : MonoBehaviour
{
    [Header("Cameras")]
    public Camera leftCamera;
    public Camera rightCamera;

    [Header("UI Buttons")]
    public Button leftButton;
    public Button rightButton;

    [Header("Button Images")]
    public Image leftButtonImage;
    public Image rightButtonImage;

    [Header("Sprites")]
    public Sprite fullScreenSprite;
    public Sprite minimizeSprite;

    [Header("DoTween Settings")]
    public float transitionTime = 0.4f;

    private bool leftFull = false;
    private bool rightFull = false;

    // Store exact original X positions (like 0.503)
    private float leftOriginalX;
    private float rightOriginalX;

    void Awake()
    {
        // Capture real starting viewport X values
        leftOriginalX = leftCamera.rect.x;   // -0.5
        rightOriginalX = rightCamera.rect.x; // 0.503

        leftButton.onClick.AddListener(ToggleLeft);
        rightButton.onClick.AddListener(ToggleRight);
    }

    // ---------------- LEFT FULLSCREEN -----------------

    void ToggleLeft()
    {
        if (!leftFull && !rightFull)
        {
            // Expand left camera fullscreen
            AnimateX(leftCamera, 0f);
            AnimateX(rightCamera, 1f);

            leftFull = true;

            leftButtonImage.sprite = minimizeSprite;
            rightButton.interactable = false;
        }
        else if (leftFull)
        {
            RestoreBoth();
        }
    }

    // ---------------- RIGHT FULLSCREEN -----------------

    void ToggleRight()
    {
        if (!rightFull && !leftFull)
        {
            // Expand right camera fullscreen
            AnimateX(rightCamera, 0f);
            AnimateX(leftCamera, -1f);

            rightFull = true;

            rightButtonImage.sprite = minimizeSprite;
            leftButton.interactable = false;
        }
        else if (rightFull)
        {
            RestoreBoth();
        }
    }

    // ---------------- RESTORE BOTH CAMERAS -----------------

    void RestoreBoth()
    {
        AnimateX(leftCamera, leftOriginalX);
        AnimateX(rightCamera, rightOriginalX);

        leftFull = false;
        rightFull = false;

        leftButton.interactable = true;
        rightButton.interactable = true;

        leftButtonImage.sprite = fullScreenSprite;
        rightButtonImage.sprite = fullScreenSprite;
    }

    // ---------------- ANIMATION -----------------

    void AnimateX(Camera cam, float targetX)
    {
        Rect r = cam.rect;

        DOTween.To(
            () => cam.rect.x,
            x => cam.rect = new Rect(x, r.y, r.width, r.height),
            targetX,
            transitionTime
        )
        .SetEase(Ease.InOutSine);
    }
}
