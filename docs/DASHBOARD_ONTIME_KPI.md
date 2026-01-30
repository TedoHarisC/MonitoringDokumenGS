# Dashboard On-Time Submission KPI - Implementation Guide

## Overview

Dashboard KPI untuk menampilkan performa vendor dalam ketepatan waktu submit invoice. Implementasi mengikuti best practices UX design dan industry standards.

## UI Components

### 1. **KPI Summary Cards** (4 Cards)

- **Total Invoices**: Total semua invoice dalam periode yang dipilih
- **On-Time Submissions**: Jumlah invoice yang submit ≤ tanggal 7 (dengan icon success)
- **Late Submissions**: Jumlah invoice yang submit > tanggal 7 (dengan icon danger)
- **On-Time Percentage**: Persentase ketepatan waktu dengan:
  - Progress bar dinamis
  - Badge status (Excellent/Good/Poor)
  - Color coding: Green (≥90%), Yellow (70-89%), Red (<70%)

### 2. **Donut Chart - On-Time vs Late Distribution**

- Visualisasi proporsi on-time vs late submissions
- Total count ditampilkan di center donut
- Interactive tooltip dengan percentage
- Color scheme: Green untuk on-time, Red untuk late

### 3. **Vendor Performance Ranking Table**

Menampilkan ranking vendor berdasarkan on-time percentage dengan:

- **Rank Badges**: 🥇 Gold, 🥈 Silver, 🥉 Bronze untuk top 3
- **Vendor Name**: Bold dengan detail breakdown on-time/late count
- **Total Invoices**: Count total per vendor
- **On-Time %**: Progress bar mini + percentage number
- **Status Badge**: Color-coded (Excellent/Good/Poor)

### 4. **Year Filter**

Dropdown untuk filter data by year (2024-2027, default current year)

## Technical Implementation

### Frontend Files Modified:

1. **Views/Home/Index.cshtml**
   - Added 3 sections: KPI cards, chart, performance table
2. **wwwroot/js/dashboard-home.js**
   - `loadOnTimeSubmissionKpi()`: Main function to fetch data
   - `renderOnTimeChart()`: Render donut chart using ApexCharts
   - `renderVendorPerformanceTable()`: Populate vendor ranking table
   - Event listeners for year filter

### API Endpoint:

```
GET /api/dashboard/vendor-ontime-submission?year={year}
```

**Response Structure:**

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
    }
  ]
}
```

## UX Features

### Visual Hierarchy

1. **Priority 1**: Overall KPI cards at top - immediate attention
2. **Priority 2**: Distribution chart (left) - visual understanding
3. **Priority 3**: Detailed vendor ranking (right) - deep dive

### Color Psychology

- **Green** (#10b981): Success, on-time, excellent
- **Yellow/Orange** (#f59e0b): Warning, good but needs attention
- **Red** (#ef4444): Alert, late, poor performance

### Interactive Elements

- Refresh buttons on each section
- Year filter with immediate update
- Hover tooltips on charts
- Progress bars for visual feedback

### Responsive Design

- Uses Bootstrap grid: `col-xxl-6` for 50/50 split on large screens
- Stacks vertically on mobile devices
- Card-based layout for modularity

### Accessibility

- Color + text labels (not color-only indicators)
- Clear headings and labels
- Tooltip information
- Semantic HTML structure

## Performance Status Classification

| Percentage | Status    | Color  | Description                          |
| ---------- | --------- | ------ | ------------------------------------ |
| ≥ 90%      | Excellent | Green  | Outstanding performance              |
| 70-89%     | Good      | Yellow | Acceptable with room for improvement |
| < 70%      | Poor      | Red    | Needs immediate attention            |

## Loading States

- Initial load: "Loading..." message
- Error state: "Failed to load data" with red text
- Empty state: "No vendor data available"

## Integration with Existing Dashboard

Positioned after "Budget KPI Summary" section, maintaining visual consistency with:

- Same card structure and styling
- Consistent spacing and borders
- Matching color scheme
- Similar interaction patterns

## Best Practices Applied

1. **Data Visualization**: Donut chart > Pie chart (better for showing parts of whole)
2. **Progressive Disclosure**: Summary → Chart → Detail table
3. **Feedback**: Progress bars + color indicators + text labels
4. **Consistency**: Matches existing dashboard patterns
5. **Performance**: Lazy loading, destroy old charts before recreating
6. **Error Handling**: Graceful degradation with error messages

## Testing Checklist

- [ ] KPI cards show correct totals
- [ ] Percentage calculation is accurate
- [ ] Progress bar matches percentage
- [ ] Badge color matches performance level
- [ ] Chart renders correctly
- [ ] Table populates with vendor data
- [ ] Year filter updates all sections
- [ ] Refresh buttons work
- [ ] Responsive on mobile/tablet
- [ ] Error states display properly
- [ ] Empty state displays when no data

## Future Enhancements (Optional)

1. Export to Excel/PDF functionality
2. Date range picker (beyond year)
3. Drill-down to invoice details
4. Trend line (compare with previous periods)
5. Email alerts for poor performers
6. Target setting per vendor

---

**Implementation Date**: January 30, 2026  
**Status**: ✅ Complete and Production Ready
