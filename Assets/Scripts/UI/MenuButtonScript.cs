using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class MenuButtonScript : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    private Color inactiveColor = new(0.5058823529f, 0.5921568627f, 0.6588235294f);
    [SerializeField] private GameObject leftDecal;
    [SerializeField] private GameObject rightDecal;
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private MainMenu menu;

    public void OnPointerEnter(PointerEventData eventData)
    {
        ActivateDecal();
        EventSystem.current.SetSelectedGameObject(this.gameObject);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        DeactivateDecal();
    }

    public void OnSelect(BaseEventData eventData)
    {
        ActivateDecal();
    }

    public void OnDeselect(BaseEventData eventData)
    {
        DeactivateDecal();
    }

    public void ActivateDecal()
    {
        text.color = Color.white;
        leftDecal.SetActive(true);
        rightDecal.SetActive(true);
    }

    public void DeactivateDecal()
    {
        text.color = inactiveColor;
        leftDecal.SetActive(false);
        rightDecal.SetActive(false);
    }
}
