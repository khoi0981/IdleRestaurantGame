# 🍔 Food Purchase System - Setup Guide

## Overview
This system allows **customers to only order food items that have been purchased/unlocked** in the database. Food prices are fetched from **FoodData** instead of hardcoded values.

## How It Works

### 1. **Customer Ordering Flow**
```
Customer sits down 
  → GetPurchasedFoodsMenu() [from DataManager]
  → Random selection from purchased foods
  → ConvertFoodIDToItemType() [FoodID → ItemType]
  → Fetch Price from FoodData
  → Display order bubble
  → Eat and pay (using FoodData price)
```

### 2. **Key Components**

#### **DataManager.cs** (Enhanced)
Manages purchased foods with these methods:

```csharp
// Buy/Unlock a food
DataManager.Instance.BuyFood("BIGBURGER");

// Check if food purchased
bool isPurchased = DataManager.Instance.IsFoodPurchased("BIGBURGER");

// Get all purchased food IDs
List<string> purchased = DataManager.Instance.GetPurchasedFoodIDs();
```

#### **CustomerAI.cs** (Updated)
- Reads purchased foods from DataManager
- Converts FoodData to orders
- Fetches prices from FoodData

---

## 🔧 Setup Instructions

### Step 1: Create FoodData Assets (if not already done)
1. Right-click in Assets → Create → Food → Food Data
2. Fill in:
   - **Food ID**: `BIGBURGER` (must match in ConvertFoodIDToItemType mapping)
   - **Food Name**: `Big Burger`
   - **Cost**: `100` (purchase cost)
   - **Price**: `50` (selling price to customers)
   - **Icon**: Your food icon

### Step 2: Create FoodDataList
1. Right-click in Assets → Create → Food → Food Data List
2. Add your FoodData items to the list
3. **Important**: Save in `Resources/FoodDataList.asset` (or assign in inspector)

### Step 3: Setup Customer Prefab
1. Open **Customer 1.prefab**
2. In **CustomerAI** component:
   - Drag **FoodDataList** into the "Food Data List" field
   - OR leave empty (will auto-load from Resources)

### Step 4: Initialize Purchased Foods (Optional)
Add this to your startup/shop code:

```csharp
void InitializeShop()
{
    // Start with BIG BURGER available
    DataManager.Instance.BuyFood("BIGBURGER");
    DataManager.Instance.BuyFood("MEATCOOKED");
}
```

---

## 📋 Food ID to ItemType Mapping

Currently supported mappings in `ConvertFoodIDToItemType()`:

| Food ID | ItemType | Notes |
|---------|----------|-------|
| BIGBURGER / HAMBURGER | ItemType.HAMBURGER | Burger food |
| MEATCOOKED / COOKEDMEAT | ItemType.COOKEDMEAT | Cooked meat |

### ➕ To Add More Foods:

1. **Create ItemType enum value** (in `IGetItem.cs`):
```csharp
public enum ItemType
{
    // ... existing items ...
    PIZZA,      // ✅ Add this
    PASTA,      // ✅ Add this
}
```

2. **Add mapping in CustomerAI.ConvertFoodIDToItemType()**:
```csharp
switch (normalizedID)
{
    case "PIZZA":
        return ItemType.PIZZA;
    
    case "PASTA":
        return ItemType.PASTA;
    
    // ... rest of cases ...
}
```

3. **Create FoodData for your new foods** with matching Food IDs

---

## 🎮 Testing

### Test Case 1: No Foods Purchased
- Expected: If no foods purchased, system falls back to old `availableMenu`
- Result: Customers should still work (backward compatible)

### Test Case 2: One Food Purchased
```csharp
DataManager.Instance.BuyFood("BIGBURGER");
```
- Expected: All customers should ONLY order Big Burger
- Verify: Customer talks show only burger bubble

### Test Case 3: Multiple Foods Purchased
```csharp
DataManager.Instance.BuyFood("BIGBURGER");
DataManager.Instance.BuyFood("MEATCOOKED");
```
- Expected: Customers should randomly choose between available foods
- Verify: Some customers order burger, some order meat

### Test Case 4: Correct Price from FoodData
- Set FoodData Price to 100
- Customer eats food
- Expected: totalMoney should increase by 100 (not 50)
- Verify: Check DataManager.totalMoney in inspector

---

## 🐛 Troubleshooting

### Issue: "❌ Menu trống! Khách không có lựa chọn món ăn"
**Solution**: Buy at least one food first
```csharp
DataManager.Instance.BuyFood("BIGBURGER");
```

### Issue: "⚠️ Không có mapping cho Food ID"
**Solution**: 
1. Check FoodData.FoodID matches mapping in `ConvertFoodIDToItemType()`
2. Add new mapping if needed (see section above)

### Issue: "FoodDataList không tìm thấy!"
**Solution**:
1. Either drag FoodDataList in Customer prefab inspector
2. OR place FoodDataList.asset in `Assets/Resources/FoodDataList.asset`
3. Load manually: `Resources.Load<FoodDataList>("FoodDataList")`

### Issue: Customer not paying correct price
**Solution**: Check that FoodData.Price is set correctly
```csharp
// Debug: Check what price was fetched
FoodData food = foodDataList.GetFoodByID("BIGBURGER");
Debug.Log("Price: " + food.Price); // Should show correct price
```

---

## 💾 Data Storage

All purchased foods are saved to **PlayerPrefs** with key: `FoodPurchaseData`

### Save format:
```json
{
  "purchasedFoodIDs": ["BIGBURGER", "MEATCOOKED"]
}
```

### Manual reset (for testing):
```csharp
PlayerPrefs.DeleteKey("FoodPurchaseData");
```

---

## 🔗 Integration Points

### Where to add food purchase UI:
1. **Shop System**: Add "Buy Food" button → calls `DataManager.Instance.BuyFood(foodID)`
2. **Menu Manager**: Can check `DataManager.Instance.IsFoodPurchased(foodID)`
3. **Upgrade Menu**: Show which foods are unlocked

### Example integration:
```csharp
// In your ShopUI or MenuManager
public void OnBuyFoodClicked(string foodID, int cost)
{
    if (DataManager.Instance.totalMoney >= cost)
    {
        DataManager.Instance.SubstractGold(cost);
        DataManager.Instance.BuyFood(foodID);
        Debug.Log("✅ Food purchased: " + foodID);
    }
}
```

---

## 📝 Notes

- ✅ **Backward Compatible**: If no foods purchased, uses old `availableMenu` system
- ✅ **Automatic Fallback**: If FoodDataList not found, loads from Resources
- ✅ **Flexible Mapping**: Easy to add new foods without changing core code
- ⚠️ **Resources Path**: Default path is `Resources/FoodDataList.asset`

---

## 🎯 Next Steps

1. Create your FoodData items
2. Create FoodDataList and populate it
3. Add purchase logic to your shop/upgrade system
4. Test purchasing and ordering flow
5. Add more foods as needed (update ItemType enum + mapping)

Enjoy! 🍽️
