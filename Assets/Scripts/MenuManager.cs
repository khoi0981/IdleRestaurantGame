using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public GameObject menuPanel;
    public Button menuButton;
    public Button closeButton;

    public GameObject itemPrefab;
    public Transform contentContainer;
    public MenuDatabase database; // drag ScriptableObject vào

    public Text categoryHeader; // nếu muốn lọc theo danh mục

    private void Awake()
    {
        menuButton.onClick.AddListener(ToggleMenu);
        closeButton.onClick.AddListener(ToggleMenu);
        menuPanel.SetActive(false);
    }

    public void ToggleMenu()
    {
        bool active = !menuPanel.activeSelf;
        menuPanel.SetActive(active);
        if (active)
            PopulateMenu();
        else
            ClearMenu();
    }

    private void ClearMenu()
    {
        foreach (Transform child in contentContainer)
            Destroy(child.gameObject);
    }

    private void PopulateMenu(string filter = "All")
    {
        ClearMenu();

        foreach (var item in database.items)
        {
            if (filter != "All" && item.category != filter) continue;

            GameObject go = Instantiate(itemPrefab, contentContainer);
            go.name = item.name;

            Text[] texts = go.GetComponentsInChildren<Text>();
            // giả định child Text thứ 0: name, 1: price
            if (texts.Length >= 2)
            {
                texts[0].text = item.name;
                texts[1].text = item.price + " VND";
            }

            Image icon = go.GetComponentInChildren<Image>();
            if (icon != null && item.icon != null)
                icon.sprite = item.icon;

            Button b = go.GetComponent<Button>();
            if (b != null)
                b.onClick.AddListener(() => OnFoodSelected(item));
        }
    }

    public void OnFoodSelected(FoodItem food)
    {
        Debug.Log("Chọn: " + food.name + " - " + food.price + " VND");
        // TODO: add vào giỏ hoặc gọi hàm xử lý mua hàng
    }

    // Thêm method cho filter nếu cần
    public void SetFilter(string category)
    {
        PopulateMenu(category);
    }
}