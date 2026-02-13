# Budget Per Vendor Category - Quick Reference

## ✅ Sudah Terakomodir!

Ya, sistem budget notification **sudah mendukung budget per vendor category**! Setiap kategori vendor bisa punya budget terpisah dan notifikasi dikirim independen per kategori.

---

## 📌 Cara Kerja Singkat

### 1. Buat Budget Per Kategori

```
Year: 2026, TypeBudget: "IT",           TotalBudget: 500 juta
Year: 2026, TypeBudget: "Construction", TotalBudget: 1 miliar
Year: 2026, TypeBudget: "Consulting",   TotalBudget: 300 juta
```

### 2. Sistem Otomatis Tracking

```
Invoice → Vendor → VendorCategory → Budget
```

Contoh:

- Invoice 100 juta dari "PT ABC" (kategori IT) → Masuk ke budget IT
- Invoice 200 juta dari "PT XYZ" (kategori Construction) → Masuk ke budget Construction

### 3. Notifikasi Terpisah Per Kategori

- ✅ IT overbudget? → Alert khusus IT dikirim
- ✅ Construction masih aman? → Tidak ada alert Construction
- ✅ Consulting >90%? → Warning khusus Consulting dikirim

---

## 🔍 Contoh Skenario

### Input: 3 Budget Untuk 2026

| TypeBudget   | TotalBudget | MonthlyBudget |
| ------------ | ----------- | ------------- |
| IT           | 500 juta    | 42 juta       |
| Construction | 1 miliar    | 83 juta       |
| Consulting   | 300 juta    | 25 juta       |

### Spending Bulan Maret 2026

| Vendor        | Category     | Invoice Amount |
| ------------- | ------------ | -------------- |
| PT TechSoft   | IT           | 50 juta ✅     |
| PT BuildCorp  | Construction | 80 juta ✅     |
| PT ConsultPro | Consulting   | 30 juta ⚠️     |

### Hasil Notifikasi

**📧 Email 1: IT - Overbudget Monthly**

```
⚠️ BUDGET OVERRUN ALERT - IT - March 2026

Category: IT
Period: March 2026
Budget: 42,000,000
Spent: 50,000,000
Over Budget: 8,000,000 (19.0%)
```

**📧 Email 2: Consulting - Overbudget Monthly**

```
⚠️ BUDGET OVERRUN ALERT - Consulting - March 2026

Category: Consulting
Period: March 2026
Budget: 25,000,000
Spent: 30,000,000
Over Budget: 5,000,000 (20.0%)
```

**✅ Construction:** Masih aman (80 juta < 83 juta), **tidak ada notifikasi**.

---

## 🚀 Setup Steps

### Langkah 1: Pastikan Vendor Category Sudah Ada

```
Master > Master Data > Vendor Categories
- IT
- Construction
- Consulting
- Maintenance
```

### Langkah 2: Assign Vendor ke Category

```
Master > Vendors > Edit Vendor
- PT TechSoft → Category: IT
- PT BuildCorp → Category: Construction
- PT ConsultPro → Category: Consulting
```

### Langkah 3: Buat Budget Per Category

```
Master > Budget > Create Budget

Budget 1:
- Year: 2026
- Vendor Category: IT
- Total Budget: 500,000,000
- Monthly Budget: 41,666,666

Budget 2:
- Year: 2026
- Vendor Category: Construction
- Total Budget: 1,000,000,000
- Monthly Budget: 83,333,333

Budget 3:
- Year: 2026
- Vendor Category: Consulting
- Total Budget: 300,000,000
- Monthly Budget: 25,000,000
```

### Langkah 4: System Otomatis Running

- ✅ Background service cek setiap hari jam 9 pagi
- ✅ Real-time check saat invoice dibuat/diupdate
- ✅ Email otomatis ke ADMIN jika overbudget atau >90%

---

## 📊 Cek Status Budget Per Category

### Via Dashboard (Home)

```
Dashboard > Budget Summary (pilih year)
```

Chart "Budget vs Realisasi per Vendor" menampilkan breakdown per category.

### Via SQL Query

```sql
SELECT
    vc.Name AS Category,
    b.TotalBudget AS Budget,
    ISNULL(SUM(i.InvoiceAmount), 0) AS Spent,
    b.TotalBudget - ISNULL(SUM(i.InvoiceAmount), 0) AS Remaining,
    CASE
        WHEN b.TotalBudget > 0 THEN
            (ISNULL(SUM(i.InvoiceAmount), 0) / b.TotalBudget * 100)
        ELSE 0
    END AS UtilizationPct
FROM MST_VendorCategory vc
LEFT JOIN MST_Budget b ON b.TypeBudget = vc.Name AND b.Year = 2026
LEFT JOIN MST_Vendor v ON v.VendorCategoryId = vc.VendorCategoryId AND v.IsDeleted = 0
LEFT JOIN TRX_Invoice i ON i.VendorId = v.VendorId AND i.InvoiceYear = 2026 AND i.IsDeleted = 0
WHERE vc.IsDeleted = 0
GROUP BY vc.Name, b.TotalBudget
ORDER BY UtilizationPct DESC;
```

---

## 🧪 Testing

### Test Overbudget Alert untuk Category IT

```bash
# 1. Buat budget IT untuk 2026: 100 juta
# 2. Buat invoice dari vendor IT: 110 juta
# 3. Preview alert
GET /api/notifications/test/preview/budget-alert?year=2026&month=3

# 4. Trigger manual check
POST /api/notifications/test/trigger/budget-check?year=2026

# 5. Cek log notifikasi
GET /api/notifications/test/logs
```

Expected result: Email dikirim dengan subject "⚠️ BUDGET OVERRUN ALERT - IT - Year 2026"

---

## ❓ FAQ

### Q: Apakah bisa 1 year punya banyak budget?

**A:** ✅ Ya! Satu year bisa punya banyak budget, satu untuk setiap vendor category.

### Q: Bagaimana kalau vendor tidak punya category?

**A:** Invoice dari vendor tanpa category tidak akan ter-track di budget manapun. **Pastikan semua vendor punya category assignment.**

### Q: Apakah anti-spam berlaku per category?

**A:** ✅ Ya! Cooldown 24 jam berlaku **per category**. Jadi bisa dapat alert IT dan Construction bersamaan.

### Q: Bagaimana kalau ada invoice mixed category?

**A:** Tidak bisa. Satu invoice = satu vendor = satu category. Jika perlu split, buat 2 invoice terpisah.

### Q: Apakah bisa edit TypeBudget setelah budget dibuat?

**A:** ⚠️ Bisa, tapi hati-hati! Ubah TypeBudget = pindah budget ke category lain. Historical spending tracking bisa jadi tidak akurat.

### Q: Bagaimana cara disable notifikasi untuk category tertentu?

**A:** Hapus budget entry untuk category tersebut. Tanpa budget = tidak ada alert.

### Q: Apakah background service running otomatis?

**A:** ✅ Ya! BudgetCheckBackgroundService running otomatis setiap hari jam 9 pagi, cek semua categories.

---

## 📝 Summary

| Feature                                  | Status                   |
| ---------------------------------------- | ------------------------ |
| Multiple budgets per year (per category) | ✅ Supported             |
| Auto-filter invoice by vendor category   | ✅ Implemented           |
| Independent alerts per category          | ✅ Working               |
| Anti-spam per category                   | ✅ Active (24h cooldown) |
| Real-time check on invoice change        | ✅ Running               |
| Scheduled daily check (9 AM)             | ✅ Running               |
| Email notification with category info    | ✅ Complete              |
| In-app notification                      | ✅ Complete              |

---

## 🎯 Next Steps

1. ✅ **Pastikan semua vendor sudah punya category**

   ```sql
   SELECT v.VendorName
   FROM MST_Vendor v
   LEFT JOIN MST_VendorCategory vc ON v.VendorCategoryId = vc.VendorCategoryId
   WHERE vc.VendorCategoryId IS NULL AND v.IsDeleted = 0;
   ```

2. ✅ **Buat budget untuk semua active categories**

   ```sql
   SELECT vc.Name
   FROM MST_VendorCategory vc
   LEFT JOIN MST_Budget b ON b.TypeBudget = vc.Name AND b.Year = 2026
   WHERE b.BudgetId IS NULL AND vc.IsDeleted = 0;
   ```

3. ✅ **Test notification untuk 1 category**
   - Buat sample budget
   - Buat invoice overbudget
   - Verify email dikirim dengan info category

4. ✅ **Monitor daily logs**
   ```
   Check server logs: "Checking budget for Category: {CategoryName}"
   ```

---

## 📚 Related Docs

- [VENDOR_CATEGORY_BUDGET.md](VENDOR_CATEGORY_BUDGET.md) - Dokumentasi lengkap
- [SMART_NOTIFICATION_SYSTEM.md](SMART_NOTIFICATION_SYSTEM.md) - Anti-spam mechanism
- [TESTING_GUIDE.md](TESTING_GUIDE.md) - Testing procedures

**✅ Budget per vendor category sudah terakomodir dan siap production!** 🚀
