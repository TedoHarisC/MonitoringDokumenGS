# Debugging: Contracts Expiring Soon Loading Issue

## Masalah

Bagian "Contracts Expiring Soon" selalu menampilkan "Loading contracts..." dan tidak menampilkan data.

## Langkah-langkah Debug

### 1. Cek Browser Console

Buka halaman dashboard (`http://localhost:5008`) dan buka Developer Tools (F12).

Di tab **Console**, cari pesan:

- `Loading contracts expiring in 30 days` - ini menandakan fungsi dipanggil
- `Contracts response:` - ini menandakan API response berhasil
- Error apapun berwarna merah

### 2. Cek Network Tab

Di Developer Tools, buka tab **Network**:

- Cari request ke `/api/dashboard/contracts-expiring?days=30`
- Jika request ada:
  - Klik untuk melihat Response
  - Cek Status Code (harus 200)
  - Cek Response Body (harus ada `success: true` dan `data` array)
- Jika request tidak ada:
  - JavaScript tidak berjalan atau ada error sebelumnya

### 3. Test API Endpoint Manual

Buka tab baru di browser dan akses langsung:

```
http://localhost:5008/api/dashboard/contracts-expiring?days=30
```

**Expected Response:**

```json
{
  "success": true,
  "data": [...contracts...],
  "count": 0
}
```

Jika redirect ke login page → **Authorization Issue**
Jika error 500 → **Server Error** (cek log server)

### 4. Cek HTML Element

Di Console, ketik:

```javascript
$("#contractsExpiringTableBody").length;
$("#expiringContractsCount").length;
$("#expiryDaysFilter").length;
```

Harus return **1** untuk setiap element. Jika return **0**, element HTML tidak ada di page.

### 5. Manual Trigger JavaScript

Di Console, ketik:

```javascript
loadContractsExpiring(30);
```

Lalu cek apakah ada response atau error.

### 6. Cek jQuery Loaded

Di Console, ketik:

```javascript
typeof $;
```

Harus return **"function"**. Jika **"undefined"**, jQuery tidak loaded.

## Kemungkinan Penyebab

### A. Authorization Issue

**Symptom:** Request tidak pernah sampai ke API atau redirect ke login
**Solution:** Pastikan sudah login dan memiliki cookie auth

### B. JavaScript Error Before Load

**Symptom:** Function `loadContractsExpiring()` tidak dipanggil
**Solution:** Cek console untuk error di atas script contracts

- Fix error lain di dashboard-home.js terlebih dahulu

### C. HTML Section Tidak Ada

**Symptom:** `$('#contractsExpiringTableBody').length` return 0
**Solution:**

- Refresh cache browser (Ctrl+F5)
- Cek View/Home/Index.cshtml apakah section ada

### D. API Error

**Symptom:** Status 500 di Network tab
**Solution:** Cek server log untuk Exception details

### E. Data Kosong

**Symptom:** API return success tapi data array kosong
**Solution:** Tidak ada contracts expiring dalam range yang di-query

- Normal jika tidak ada contracts atau semua masih lama
- Try ubah filter ke 60 atau 90 days

## Quick Fix Test

Jika semua gagal, test dengan data dummy di console:

```javascript
updateContractsExpiringTable([
  {
    contractId: "00000000-0000-0000-0000-000000000001",
    contractNo: "TEST-001",
    vendorName: "Test Vendor",
    startDate: "2025-01-01",
    endDate: "2026-02-15",
    daysRemaining: 15,
    contractValue: 100000000,
    status: "Active",
    alertLevel: "Warning",
    description: "Test Contract",
  },
]);
```

Jika ini berhasil → **API issue atau data kosong**
Jika ini gagal → **JavaScript or HTML issue**

## Logs to Check

Jalankan di terminal untuk melihat request:

```bash
dotnet run | grep -i "contracts-expiring"
```

Expected log saat dashboard di-load:

```
info: DashboardController[0]
      Fetching contracts expiring in 30 days
info: DashboardController[0]
      Found X contracts expiring
```

## Contact

Jika masih belum berhasil, berikan screenshot dari:

1. Browser Console (tab Console)
2. Browser Network (filter: contracts-expiring)
3. Server terminal log
