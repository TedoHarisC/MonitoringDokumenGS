# Budget Per Vendor Category - Documentation

## Overview

Budget notifications support **per vendor category budgeting**. You can create multiple budgets per year, one for each vendor category, and the system will track spending and send alerts independently for each category.

## How It Works

### 1. Budget Setup

Create separate budgets for each vendor category:

| Year | TypeBudget (Category) | Total Budget  | Monthly Budget |
| ---- | --------------------- | ------------- | -------------- |
| 2026 | IT                    | 500,000,000   | 41,666,666     |
| 2026 | Construction          | 1,000,000,000 | 83,333,333     |
| 2026 | Consulting            | 300,000,000   | 25,000,000     |

**Steps:**

1. Go to **Master > Budget**
2. Click **Create Budget**
3. Select **Year** (e.g., 2026)
4. Select **Vendor Category** from dropdown (e.g., "IT")
5. Enter **Total Budget** and **Monthly Budget**
6. Save

**Important:**

- One budget entry per category per year
- TypeBudget field stores the vendor category name (e.g., "IT", "Construction")
- Vendors must be assigned to categories in Master > Vendor

### 2. Invoice Tracking Per Category

When invoices are created/updated:

```
Invoice → VendorId → Vendor.VendorCategoryId → VendorCategory.Name
```

The system automatically:

1. Finds the budget with matching `TypeBudget = VendorCategory.Name`
2. Sums only invoices from vendors in that category
3. Checks if category-specific budget is exceeded

### 3. Notification System

#### Scheduled Daily Check (9 AM)

**BudgetCheckBackgroundService** runs daily and:

- Gets ALL budgets for current year (multiple per vendor category)
- For each budget:
  - Filters vendors by category name
  - Sums invoices from those vendors only
  - Checks yearly and monthly overbudget
  - Sends alerts with category information

#### Real-Time Check (Invoice Changes)

When invoice is created/updated:

- Checks ALL budgets for the invoice year
- Filters spending by vendor category
- Sends immediate alert if overbudget threshold triggered

### 4. Alert Examples

#### Category-Specific Overbudget Alert

```
Subject: ⚠️ BUDGET OVERRUN ALERT - IT - Year 2026

Category: IT
Period: Year 2026
Budget: 500,000,000
Spent: 520,000,000
Over Budget: 20,000,000 (4.0%)
Utilization: 104.0%

Immediate action required!
```

#### Category-Specific Warning (>90% utilization)

```
Subject: ⚡ Budget Warning - Construction - March 2026

Category: Construction
Period: March 2026
Budget: 83,333,333
Spent: 75,500,000
Remaining: 7,833,333
Utilization: 90.6%

Please monitor spending closely.
```

## Database Schema

### MST_Budget Table

```sql
BudgetId         UNIQUEIDENTIFIER PRIMARY KEY
Year             INT              -- e.g., 2026
TypeBudget       NVARCHAR(100)    -- Vendor Category Name (e.g., "IT")
TotalBudget      DECIMAL(18,2)    -- Yearly limit
MonthlyBudget    DECIMAL(18,2)    -- Monthly limit per category
CreatedAt        DATETIME
CreatedBy        UNIQUEIDENTIFIER
```

### Vendor Table (Reference)

```sql
VendorId         UNIQUEIDENTIFIER PRIMARY KEY
VendorName       NVARCHAR(200)
VendorCategoryId INT             -- FK to MST_VendorCategory
...
```

### Invoice Table (Tracking)

```sql
InvoiceId        UNIQUEIDENTIFIER PRIMARY KEY
VendorId         UNIQUEIDENTIFIER -- FK to Vendor
InvoiceAmount    DECIMAL(18,2)
InvoiceYear      INT
InvoiceMonth     INT
...
```

## Code Implementation

### GetBudgetStatusForCategoryAsync()

```csharp
// Get budget for specific category
var budget = await _context.MST_Budget
    .Where(b => b.Year == year && b.TypeBudget == categoryName)
    .FirstOrDefaultAsync();

// Get vendors in this category
var vendorIds = await _context.Vendors
    .Include(v => v.VendorCategory)
    .Where(v => !v.IsDeleted && v.VendorCategory.Name == categoryName)
    .Select(v => v.VendorId)
    .ToListAsync();

// Sum invoices from category vendors only
var totalSpent = await _context.Invoices
    .Where(i => i.InvoiceYear == year &&
                !i.IsDeleted &&
                vendorIds.Contains(i.VendorId))
    .SumAsync(i => i.InvoiceAmount);
```

### RunAsync() - Loop All Categories

```csharp
var budgets = await _context.MST_Budget
    .Where(b => b.Year == currentYear)
    .ToListAsync();

foreach (var budget in budgets)
{
    var categoryName = budget.TypeBudget?.Trim();
    var budgetStatus = await GetBudgetStatusForCategoryAsync(
        currentYear, categoryName);

    // Check and send alerts for this category
    if (budgetStatus.IsOverBudget)
    {
        await SendOverbudgetAlert(
            "YEARLY", currentYear, null,
            budgetStatus.TotalBudget, budgetStatus.TotalSpent,
            budgetStatus.BudgetUtilizationPercent,
            categoryName  // ← Category name included
        );
    }
}
```

## Anti-Spam Mechanism

Notifications use category-aware reference IDs:

| Alert Type | Reference ID Format         | Example                |
| ---------- | --------------------------- | ---------------------- |
| Yearly     | `{year}-{category}`         | `2026-IT`              |
| Monthly    | `{year}-{month}-{category}` | `2026-03-Construction` |

**Cooldown:** 24 hours per category per period

This prevents spam while allowing simultaneous alerts for different categories:

- ✅ IT yearly alert + Construction monthly alert = Both sent
- ❌ IT yearly alert sent twice within 24h = Second blocked

## Testing

### Preview Budget Alert for Category

```bash
GET /api/notifications/test/preview/budget-alert?category=IT&year=2026&month=3
```

### Manual Trigger Category Check

```bash
POST /api/notifications/test/trigger/budget-check?year=2026
```

Will check ALL categories for that year.

### View Notification Logs by Category

```bash
GET /api/notifications/test/logs?type=BUDGET_OVERBUDGET_YEARLY
```

Check `Details` column for category information.

## Best Practices

### 1. ✅ Define All Categories

Ensure all vendor categories have budgets:

```sql
SELECT vc.Name, b.Year, b.TotalBudget
FROM MST_VendorCategory vc
LEFT JOIN MST_Budget b ON b.TypeBudget = vc.Name AND b.Year = 2026
WHERE b.BudgetId IS NULL
ORDER BY vc.Name;
```

### 2. ✅ Audit Spending Distribution

Check which categories are consuming most:

```sql
SELECT
    vc.Name AS Category,
    SUM(i.InvoiceAmount) AS TotalSpent,
    b.TotalBudget,
    (SUM(i.InvoiceAmount) / b.TotalBudget * 100) AS UtilizationPct
FROM TRX_Invoice i
INNER JOIN MST_Vendor v ON i.VendorId = v.VendorId
INNER JOIN MST_VendorCategory vc ON v.VendorCategoryId = vc.VendorCategoryId
LEFT JOIN MST_Budget b ON b.Year = i.InvoiceYear AND b.TypeBudget = vc.Name
WHERE i.InvoiceYear = 2026 AND i.IsDeleted = 0
GROUP BY vc.Name, b.TotalBudget
ORDER BY UtilizationPct DESC;
```

### 3. ✅ Monitor Unbudgeted Categories

If a vendor category has no budget entry, no alerts will be sent. Create a report to find gaps:

```sql
-- Find vendor categories without budget for 2026
SELECT DISTINCT vc.Name
FROM MST_VendorCategory vc
INNER JOIN MST_Vendor v ON v.VendorCategoryId = vc.VendorCategoryId
WHERE NOT EXISTS (
    SELECT 1 FROM MST_Budget b
    WHERE b.TypeBudget = vc.Name AND b.Year = 2026
)
AND vc.IsDeleted = 0;
```

### 4. ✅ Test Before Production

1. Create test budget for a category
2. Create invoice that exceeds budget
3. Verify category-specific alert is sent
4. Check email mentions correct category

## Migration Guide

### From Single Budget (Old) to Category Budgets (New)

**Before (Old System):**

```sql
-- Only 1 budget per year
BudgetId | Year | TotalBudget
---------|------|------------
GUID-1   | 2026 | 2,000,000,000
```

**After (New System):**

```sql
-- Multiple budgets per year (per category)
BudgetId | Year | TypeBudget    | TotalBudget
---------|------|---------------|------------
GUID-1   | 2026 | IT            | 500,000,000
GUID-2   | 2026 | Construction  | 1,000,000,000
GUID-3   | 2026 | Consulting    | 300,000,000
GUID-4   | 2026 | Maintenance   | 200,000,000
```

**Steps:**

1. Export existing budget total
2. Break down by vendor category proportion
3. Create separate budget entries per category
4. Test notifications for each category

## Troubleshooting

### Issue: No alerts sent for category

**Check:**

1. Budget exists for that category?

   ```sql
   SELECT * FROM MST_Budget WHERE Year = 2026 AND TypeBudget = 'IT';
   ```

2. Vendors assigned to category?

   ```sql
   SELECT v.VendorName, vc.Name
   FROM MST_Vendor v
   INNER JOIN MST_VendorCategory vc ON v.VendorCategoryId = vc.VendorCategoryId
   WHERE vc.Name = 'IT';
   ```

3. Category name matches exactly? (Case-sensitive comparison)
   ```csharp
   b.TypeBudget == categoryName // Must match VendorCategory.Name exactly
   ```

### Issue: Wrong spending total

**Check:**

- Vendor category assignment is correct
- Invoice VendorId links to correct Vendor
- Invoice is not soft-deleted (`IsDeleted = 0`)

### Issue: Duplicate alerts

**Check:**

- Anti-spam log: `SELECT * FROM SYS_NotificationLog WHERE NotificationType LIKE 'BUDGET_%' ORDER BY SentAt DESC;`
- Cooldown period still active
- Different categories should allow simultaneous alerts

## API Endpoints Reference

### Check Budget for Category

```http
POST /api/budget/check-category
Content-Type: application/json

{
  "year": 2026,
  "categoryName": "IT"
}
```

### Get Budget Status by Category

```http
GET /api/budget/status?year=2026&category=IT
```

Response:

```json
{
  "year": 2026,
  "categoryName": "IT",
  "totalBudget": 500000000,
  "totalSpent": 480000000,
  "remainingBudget": 20000000,
  "utilizationPercent": 96.0,
  "isOverBudget": false,
  "monthlyStatus": [...]
}
```

## Related Documentation

- [NOTIFICATION_GUIDE.md](NOTIFICATION_GUIDE.md) - General notification system
- [BUDGET_FEATURE.md](BUDGET_FEATURE.md) - Budget management overview
- [SMART_NOTIFICATION_SYSTEM.md](SMART_NOTIFICATION_SYSTEM.md) - Anti-spam mechanism
- [TESTING_GUIDE.md](TESTING_GUIDE.md) - How to test notifications

## Summary

✅ **Multiple budgets per year** (one per vendor category)  
✅ **Automatic category filtering** (invoices grouped by vendor category)  
✅ **Independent alerts** (IT overbudget ≠ Construction overbudget)  
✅ **Anti-spam per category** (24h cooldown per category per period)  
✅ **Real-time + scheduled checks** (immediate + daily 9 AM)

Budget notifications are now **fully category-aware** and ready for production use! 🚀
