# Vendor On-Time Submission Feature

## Overview

Fitur **Vendor On-Time Submission** menampilkan indikator ketepatan waktu vendor dalam melakukan submit invoice. Target ketepatan waktu adalah **maksimal tanggal 7** di awal bulan sesuai dengan periode invoice (InvoiceYear dan InvoiceMonth).

## Business Rules

### Kriteria Ketepatan Waktu

- **Tepat Waktu**: Invoice dibuat (CreatedAt) pada atau sebelum tanggal 7 dari bulan periode invoice
- **Terlambat**: Invoice dibuat setelah tanggal 7 dari bulan periode invoice

**Contoh:**

- Invoice untuk periode January 2026 dibuat pada 5 Januari 2026 → **Tepat Waktu** ✅
- Invoice untuk periode January 2026 dibuat pada 10 Januari 2026 → **Terlambat** ❌
- Invoice untuk periode February 2026 dibuat pada 7 Februari 2026 → **Tepat Waktu** ✅

### Performance Status (KPI)

Untuk breakdown per vendor:

- **Excellent**: ≥ 90% invoice tepat waktu
- **Good**: 70% - 89% invoice tepat waktu
- **Poor**: < 70% invoice tepat waktu

## Database Changes

### Model Invoice

Ditambahkan property baru:

```csharp
public bool IsOnTime { get; set; }
```

Property ini dihitung secara otomatis saat:

1. **Create Invoice**: Dihitung berdasarkan CreatedAt vs deadline (tanggal 7)
2. **Update Invoice**: Recalculated jika InvoiceYear atau InvoiceMonth berubah

## Implementation Details

### 1. Backend Service

**File**: `Services/Transaction/InvoiceService.cs`

**CreateAsync Method**:

```csharp
var createdAt = DateTime.UtcNow;
var deadline = new DateTime(dto.InvoiceYear, dto.InvoiceMonth, 7);
var isOnTime = createdAt.Date <= deadline.Date;
entity.IsOnTime = isOnTime;
```

**UpdateAsync Method**:

```csharp
var deadline = new DateTime(dto.InvoiceYear, dto.InvoiceMonth, 7);
var isOnTime = entity.CreatedAt.Date <= deadline.Date;
entity.IsOnTime = isOnTime;
```

### 2. DTO & Mapping

**File**: `Dtos/Transaction/InvoiceDto.cs`

```csharp
public bool IsOnTime { get; set; }
```

**File**: `Mappings/Transaction/InvoiceMappings.cs`

```csharp
IsOnTime = x.CreatedAt.Date <= new DateTime(x.InvoiceYear, x.InvoiceMonth, 7).Date
```

### 3. Frontend Display

**File**: `Views/Invoice/Index.cshtml`

- Menambahkan kolom "On-Time Status" di tabel

**File**: `wwwroot/js/transaction-invoice.js`

```javascript
{
    data: 'isOnTime',
    className: 'text-center',
    render: function (data) {
        if (data === true) {
            return '<span class="badge bg-success"><i class="feather-check-circle me-1"></i>Tepat Waktu</span>'
        } else {
            return '<span class="badge bg-danger"><i class="feather-alert-circle me-1"></i>Terlambat</span>'
        }
    }
}
```

## Dashboard KPI

### New API Endpoint

```
GET /api/dashboard/vendor-ontime-submission?year={year}
```

**Query Parameters:**

- `year` (optional): Filter by specific year. If not provided, shows all invoices.

**Response:**

```json
{
  "totalInvoices": 150,
  "onTimeSubmissions": 120,
  "lateSubmissions": 30,
  "onTimePercentage": 80.0,
  "vendorBreakdown": [
    {
      "vendorName": "Vendor A",
      "totalInvoices": 50,
      "onTimeSubmissions": 48,
      "lateSubmissions": 2,
      "onTimePercentage": 96.0,
      "performanceStatus": "Excellent"
    },
    {
      "vendorName": "Vendor B",
      "totalInvoices": 50,
      "onTimeSubmissions": 40,
      "lateSubmissions": 10,
      "onTimePercentage": 80.0,
      "performanceStatus": "Good"
    },
    {
      "vendorName": "Vendor C",
      "totalInvoices": 50,
      "onTimeSubmissions": 32,
      "lateSubmissions": 18,
      "onTimePercentage": 64.0,
      "performanceStatus": "Poor"
    }
  ]
}
```

### New DTOs

**File**: `Dtos/Dashboard/VendorOnTimeSubmissionDto.cs`

```csharp
public class VendorOnTimeSubmissionDto
{
    public string VendorName { get; set; }
    public int TotalInvoices { get; set; }
    public int OnTimeSubmissions { get; set; }
    public int LateSubmissions { get; set; }
    public decimal OnTimePercentage { get; set; }
    public string PerformanceStatus { get; set; } // "Excellent", "Good", "Poor"
}

public class OnTimeSubmissionKpiDto
{
    public int TotalInvoices { get; set; }
    public int OnTimeSubmissions { get; set; }
    public int LateSubmissions { get; set; }
    public decimal OnTimePercentage { get; set; }
    public List<VendorOnTimeSubmissionDto> VendorBreakdown { get; set; }
}
```

### Dashboard Service Method

**File**: `Services/Dashboard/DashboardService.cs`

Method: `GetVendorOnTimeSubmissionKpiAsync(int? year = null)`

Logic:

1. Fetch semua invoice (dengan optional filter year)
2. Hitung total on-time vs late submissions
3. Group by vendor untuk breakdown detail
4. Hitung percentage dan tentukan performance status per vendor
5. Sort by on-time percentage (descending)

## Database Migration

Migration file telah dibuat: `AddIsOnTimeToInvoice`

Untuk apply migration ke database:

```bash
dotnet ef database update
```

SQL yang akan di-generate:

```sql
ALTER TABLE [Invoices] ADD [IsOnTime] bit NOT NULL DEFAULT 0;
```

**Note**: Setelah migration, untuk data existing perlu update manual atau run script untuk recalculate IsOnTime berdasarkan CreatedAt dan InvoiceYear/Month.

### Update Script untuk Existing Data

```sql
UPDATE Invoices
SET IsOnTime = CASE
    WHEN CAST(CreatedAt AS DATE) <= DATEFROMPARTS(InvoiceYear, InvoiceMonth, 7)
    THEN 1
    ELSE 0
END
WHERE IsDeleted = 0;
```

## Usage Example

### Invoice List View

Setelah implementasi, di halaman Invoice List akan tampil:

| Invoice Number | Vendor   | Status    | Amount    | Tax      | Year | Month | **On-Time Status** | Created At | Actions     |
| -------------- | -------- | --------- | --------- | -------- | ---- | ----- | ------------------ | ---------- | ----------- |
| INV-001        | Vendor A | Submitted | 10,000.00 | 1,000.00 | 2026 | Jan   | 🟢 **Tepat Waktu** | 05/01/2026 | Edit Delete |
| INV-002        | Vendor B | Submitted | 15,000.00 | 1,500.00 | 2026 | Jan   | 🔴 **Terlambat**   | 12/01/2026 | Edit Delete |

### Dashboard KPI Display (Future Implementation)

Recommended visualization:

1. **Overall KPI Card**:
   - Total invoices
   - On-time percentage (with color indicator)
   - On-time count vs late count

2. **Vendor Performance Table**:
   - List vendors dengan on-time percentage
   - Performance status badge
   - Sortable by performance

3. **Chart Visualization**:
   - Pie chart: On-time vs Late ratio
   - Bar chart: Vendor comparison

## Testing

### Test Cases

1. **Create Invoice - On Time**
   - Create invoice untuk January 2026 pada tanggal 5 Januari 2026
   - Expected: IsOnTime = true, badge hijau "Tepat Waktu"

2. **Create Invoice - Late**
   - Create invoice untuk January 2026 pada tanggal 10 Januari 2026
   - Expected: IsOnTime = false, badge merah "Terlambat"

3. **Create Invoice - Deadline Day**
   - Create invoice untuk January 2026 pada tanggal 7 Januari 2026
   - Expected: IsOnTime = true (inclusive)

4. **Update Invoice - Change Period**
   - Update invoice dari January ke February
   - Expected: IsOnTime recalculated based on new deadline

5. **Dashboard API**
   - Call `/api/dashboard/vendor-ontime-submission?year=2026`
   - Verify calculation and vendor breakdown

## Future Enhancements

1. **Configurable Deadline**: Allow admin to set deadline (currently hardcoded as 7th)
2. **Email Notifications**: Remind vendors approaching deadline
3. **Historical Trend**: Show on-time percentage trend over time
4. **Dashboard Widgets**: Add KPI cards and charts to main dashboard
5. **Export Report**: Download on-time submission report as Excel/PDF

## Files Modified/Created

### Created:

- `Dtos/Dashboard/VendorOnTimeSubmissionDto.cs`
- `docs/VENDOR_ONTIME_SUBMISSION.md` (this file)

### Modified:

- `Models/Transaction/Invoice.cs` - Added IsOnTime property
- `Dtos/Transaction/InvoiceDto.cs` - Added IsOnTime property
- `Services/Transaction/InvoiceService.cs` - Added IsOnTime calculation
- `Mappings/Transaction/InvoiceMappings.cs` - Added IsOnTime mapping
- `Views/Invoice/Index.cshtml` - Added On-Time Status column
- `wwwroot/js/transaction-invoice.js` - Added On-Time Status display
- `Interfaces/IDashboard.cs` - Added GetVendorOnTimeSubmissionKpiAsync
- `Services/Dashboard/DashboardService.cs` - Implemented KPI method
- `Controllers/API/DashboardController.cs` - Added KPI endpoint

## Support

For questions or issues, contact the development team.

---

**Last Updated**: January 30, 2026
**Version**: 1.0
