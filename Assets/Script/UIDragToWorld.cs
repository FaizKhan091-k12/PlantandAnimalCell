using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class UIDragToWorld_NoJump : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public bool isPlant,isAnimal;
    public GameObject animal_Hand,plant_Hand;
    public DragActivationManager dragActivationManager;
    public AdvancedOrbitCamera advancedOrbitCamera;

    [Header("Screen Space Canvas Info")]
    public Canvas screenCanvas;

    [Header("World Space Drop Settings")]
    public RectTransform worldDropArea;
    public RectTransform worldDropParent;
    public Canvas worldCanvas;
    public GameObject activateObejct;

    private Vector2 originalAnchored;
    private Transform originalParent;
    private RectTransform rect;

    private Vector3 worldOffset;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        if (screenCanvas == null)
            screenCanvas = GetComponentInParent<Canvas>();

        originalParent = rect.parent;
        originalAnchored = rect.anchoredPosition;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isAnimal)
        {
            if (animal_Hand.activeInHierarchy)
            {
                animal_Hand.SetActive(false);
            }
        }
        if (isPlant)
        {
            if (plant_Hand.activeInHierarchy)
            {
                plant_Hand.SetActive(false);
            }
        }
        advancedOrbitCamera.canOrbit = false;

        // ⭐ Enable only this object's drop area
        dragActivationManager?.EnableExclusiveDropArea(worldDropArea);

        Camera cam = screenCanvas.worldCamera;
        RectTransform canvasRect = screenCanvas.transform as RectTransform;

        RectTransformUtility.ScreenPointToWorldPointInRectangle(
            canvasRect,
            eventData.position,
            cam,
            out Vector3 pointerWorld);

        worldOffset = rect.position - pointerWorld;

        rect.SetParent(screenCanvas.transform, true);
        rect.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        Camera cam = screenCanvas.worldCamera;
        RectTransform canvasRect = screenCanvas.transform as RectTransform;

        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
            canvasRect, eventData.position, cam, out Vector3 pointerWorld))
        {
            rect.position = pointerWorld + worldOffset;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Camera cam = screenCanvas.worldCamera;

        bool inside = RectTransformUtility.RectangleContainsScreenPoint(
            worldDropArea,
            eventData.position,
            cam
        );

        // ⭐ Restore all drop areas
        dragActivationManager?.RestoreAllDropAreas();

        if (inside)
        {
            SnapToWorldCanvas(eventData);
        }
        else
        {
            ReturnToOriginal();
        }

        advancedOrbitCamera.canOrbit = true;
    }

    private void SnapToWorldCanvas(PointerEventData eventData)
    {
        Camera cam = worldCanvas.worldCamera;

        Plane plane = new Plane(worldDropParent.forward, worldDropParent.position);
        Ray ray = cam.ScreenPointToRay(eventData.position);

        if (plane.Raycast(ray, out float dist))
        {
            Vector3 hitPoint = ray.GetPoint(dist);
            AudioManager.Instance.PlayRightAnswer();
            activateObejct?.SetActive(true);
            gameObject.SetActive(false);
           
            rect.SetParent(worldDropParent, true);
            rect.position = hitPoint;
            rect.localRotation = Quaternion.identity;
            rect.localScale = Vector3.one;

            // notify manager
            dragActivationManager?.NotifySnapped(gameObject);
        }
        else
        {
            ReturnToOriginal();
          
        }
    }

    private void ReturnToOriginal()
    {  AudioManager.Instance.PlayWrongAnswer();
        rect.SetParent(originalParent, false);
        rect.anchoredPosition = originalAnchored;
    }
}
