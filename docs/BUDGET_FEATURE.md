# Master Budget Feature

## Overview

Halaman Master Budget adalah fitur untuk mengelola data budget tahunan. Fitur ini **hanya dapat diakses oleh ADMIN dan SUPER_ADMIN**.

## Akses

- **URL**: `/Master/Budget`
- **Authorization**: `SUPER_ADMIN`, `ADMIN`
- **Menu**: Master > Budget (Hanya tampil untuk Admin & Super Admin)

## Fitur

### 1. Tampilan List Budget

- Menampilkan daftar budget dalam bentuk tabel
- Kolom: Year, Total Budget, Monthly Budget, Created At, Actions
- Dilengkapi dengan DataTables untuk pagination, sorting, dan search
- Data diurutkan berdasarkan tahun (terbaru ke terlama)

### 2. Add Budget

- Form untuk menambah budget baru
- Field:
  - **Year**: Tahun budget (required, 2000-2100)
  - **Total Budget**: Total budget untuk seluruh tahun (required, format Rupiah)
  - **Monthly Budget**: Budget per bulan (required, auto-calculate dari total/12)
- Auto-calculate: Ketika Total Budget diisi, Monthly Budget otomatis terhitung (Total/12)

### 3. Edit Budget

- Form untuk mengubah data budget yang sudah ada
- Menggunakan modal yang sama dengan Add
- Data budget akan dimuat dari API

### 4. Delete Budget

- Menghapus data budget
- Dilengkapi dengan konfirmasi sebelum menghapus

## Teknologi

- **Backend**: ASP.NET Core MVC
- **API**: REST API (`/api/budgets`)
- **Frontend**:
  - Bootstrap 5
  - jQuery
  - DataTables
- **Format**: Currency format Indonesia (Rupiah)

## API Endpoints

- `GET /api/budgets` - Get all budgets (with pagination)
- `GET /api/budgets/{id}` - Get budget by ID
- `POST /api/budgets` - Create new budget
- `PUT /api/budgets/{id}` - Update budget
- `DELETE /api/budgets/{id}` - Delete budget

## Files Created

1. **Controller**: `Controllers/Web/MasterController.cs` (updated)
2. **View**: `Views/Master/Budget.cshtml`
3. **JavaScript**: `wwwroot/js/master-budget.js`
4. **Layout**: `Views/Shared/_LayoutDashboard.cshtml` (updated - menu sidebar)
5. **Interface**: `Interfaces/IBudget.cs`
6. **Service**: `Services/Master/BudgetService.cs`
7. **API Controller**: `Controllers/API/BudgetsController.cs`
8. **Mapping**: `Mappings/Master/BudgetMappings.cs`

## Security

- Endpoint dilindungi dengan `[Authorize(Roles = "SUPER_ADMIN,ADMIN")]`
- Menu hanya tampil untuk user dengan role SUPER_ADMIN atau ADMIN
- User biasa tidak dapat mengakses halaman ini (akan redirect ke access denied)

## Usage

1. Login sebagai Admin atau Super Admin
2. Buka menu **Master > Budget**
3. Klik tombol **"Add Budget"** untuk menambah data
4. Isi form:
   - Year: 2026
   - Total Budget: 1000000000 (akan diformat otomatis menjadi Rp 1.000.000.000)
   - Monthly Budget: Akan auto-calculate menjadi 83.333.333
5. Klik **Save**
6. Data akan muncul di tabel

## Notes

- Monthly Budget akan otomatis dihitung saat blur dari field Total Budget
- Format currency menggunakan format Indonesia (titik sebagai pemisah ribuan)
- Semua operasi CRUD dilengkapi dengan alert notification
- Alert akan otomatis hilang setelah 5 detik
