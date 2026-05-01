using UnityEngine;

public class MenuToggler : MonoBehaviour
{
    [SerializeField] private GameObject targetMenu;
    [SerializeField] private bool useAnimation = false;

    private SettingUI menuAnim;
    public static bool isOpen = false;

    private void Start()
    {
        if (useAnimation && targetMenu != null)
        {
            menuAnim = targetMenu.GetComponent<SettingUI>();
        }
    }

    public void ShowMenu()
    {
        isOpen = true;
        if (useAnimation && menuAnim != null)
        {
            StartCoroutine(menuAnim.FadeIn());
        }
        else
        {
            targetMenu.SetActive(true);
        }
        Time.timeScale = 0f;
    }

    public void HideMenu()
    {
        isOpen = false;
        Time.timeScale = 1f;
        if (useAnimation && menuAnim != null)
        {
            StartCoroutine(menuAnim.FadeOut());
        }
        else
        {
            targetMenu.SetActive(false);
        }
    }

    public void ToggleMenu()
    {
        if (isOpen) HideMenu();
        else ShowMenu();
    }
}