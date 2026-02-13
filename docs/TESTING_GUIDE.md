# 🧪 Testing Guide - Smart Notification System

## 📋 Overview

Testing tools untuk preview email template dan trial notification system dengan berbagai scenario.

---

## 🎨 1. Preview Email Template (Browser)

Lihat tampilan email **TANPA mengirim**:

### **Preview Budget Alert**

```
http://localhost:5008/api/test/notifications/preview/budget-alert
```

Buka di browser untuk melihat tampilan email budget overbudget.

### **Preview Contract Expiring (Custom Days)**

```
http://localhost:5008/api/test/notifications/preview/contract-expiring?daysLeft=7
http://localhost:5008/api/test/notifications/preview/contract-expiring?daysLeft=30
http://localhost:5008/api/test/notifications/preview/contract-expiring?daysLeft=3
```

Ubah `daysLeft` untuk melihat tampilan berbeda urgency.

---

## 📧 2. Send Test Email (Real Email)

Kirim test email ke email Anda **dengan data dummy**.

### **Setup:**

1. Login dulu sebagai Admin/Super Admin
2. Get Bearer token
3. Use Postman/Swagger/cURL

### **Test Budget Alert Email**

**Endpoint:**

```bash
POST http://localhost:5008/api/test/notifications/send/budget-alert
Authorization: Bearer {your-token}
Content-Type: application/json

{
  "email": "your-email@example.com"
}
```

**Response:**

```json
{
  "success": true,
  "message": "Test budget alert email sent to your-email@example.com",
  "timestamp": "2026-02-13T10:30:00"
}
```

### **Test Contract Expiring Email**

**Endpoint:**

```bash
POST http://localhost:5008/api/test/notifications/send/contract-expiring
Authorization: Bearer {your-token}
Content-Type: application/json

{
  "email": "your-email@example.com",
  "daysLeft": 7
}
```

**Test Different Scenarios:**

```json
// CRITICAL (3 days)
{ "email": "you@example.com", "daysLeft": 3 }

// URGENT (7 days)
{ "email": "you@example.com", "daysLeft": 7 }

// HIGH (14 days)
{ "email": "you@example.com", "daysLeft": 14 }

// MEDIUM (30 days)
{ "email": "you@example.com", "daysLeft": 30 }

// LOW (60 days)
{ "email": "you@example.com", "daysLeft": 60 }
```

---

## 🔍 3. Check System Stats

Lihat kondisi budget & contract **sebelum** trigger notifikasi.

**Endpoint:**

```bash
GET http://localhost:5008/api/test/notifications/stats
Authorization: Bearer {your-token}
```

**Response Example:**

```json
{
  "success": true,
  "timestamp": "2026-02-13T10:30:00",
  "budget": {
    "year": 2026,
    "isOverbudget": true,
    "utilizationPercent": 125.5,
    "totalBudget": 120000000,
    "totalSpent": 150660000,
    "remaining": -30660000
  },
  "contracts": {
    "expiring60Days": 5,
    "expiring30Days": 3,
    "expiring7Days": 1
  }
}
```

**Interpretation:**

- ✅ `isOverbudget: true` → Budget alert akan dikirim
- ✅ `expiring7Days: 1` → Ada 1 contract urgent

---

## 🚀 4. Trigger Real Notification (CAUTION!)

### ⚠️ **WARNING**

Ini akan kirim **REAL EMAIL** ke user/admin dengan **REAL DATA**!

### **Trigger Budget Check**

```bash
POST http://localhost:5008/api/test/notifications/trigger/budget-check
Authorization: Bearer {your-token}
```

**What happens:**

1. Check budget status (real data)
2. If overbudget → Send email to admins ✉️
3. If near limit (90%) → Send warning ⚡
4. Log to database

### **Trigger Contract Check**

```bash
POST http://localhost:5008/api/test/notifications/trigger/contract-check
Authorization: Bearer {your-token}
```

**What happens:**

1. Check all contracts expiring (real data)
2. Send multi-level reminders (60, 30, 14, 7, ≤3 days)
3. Email to PIC + Admin (for urgent) ✉️
4. Log to database

---

## 📜 5. View Notification Logs

Lihat history notifikasi yang sudah dikirim.

**Endpoint:**

```bash
GET http://localhost:5008/api/test/notifications/logs?limit=50
Authorization: Bearer {your-token}
```

**Response:**

```json
{
  "success": true,
  "count": 15,
  "logs": [
    {
      "id": 1,
      "notificationType": "BUDGET_OVERBUDGET_MONTHLY",
      "referenceId": "2026-01",
      "recipientEmail": "admin@example.com",
      "reminderLevel": 0,
      "sentAt": "2026-02-13T09:00:00",
      "details": "Overbudget: 2500000.00 (25.0%)"
    },
    {
      "id": 2,
      "notificationType": "CONTRACT_EXPIRING_7DAYS",
      "referenceId": "123e4567-e89b-12d3-a456-426614174000",
      "recipientEmail": "pic@example.com",
      "reminderLevel": 4,
      "sentAt": "2026-02-13T08:00:00",
      "details": "Days left: 7, Level: 4"
    }
  ]
}
```

---

## 🎯 Testing Scenarios

### **Scenario 1: Preview Email Templates**

1. Buka browser
2. Login ke aplikasi
3. Buka URL preview:
   - Budget: `http://localhost:5008/api/test/notifications/preview/budget-alert`
   - Contract 7 days: `http://localhost:5008/api/test/notifications/preview/contract-expiring?daysLeft=7`
   - Contract 3 days: `http://localhost:5008/api/test/notifications/preview/contract-expiring?daysLeft=3`
4. Screenshot untuk dokumentasi

### **Scenario 2: Test Email Delivery**

1. Get token (login via API)
2. Gunakan Postman/Swagger
3. Send test budget alert ke email Anda
4. Check inbox (termasuk spam folder)
5. Verify email template muncul dengan benar

### **Scenario 3: Test Anti-Spam**

1. Trigger budget check (POST `/trigger/budget-check`)
2. Tunggu response sukses
3. Immediately trigger lagi
4. Check logs → Seharusnya blocked (cooldown)
5. Verify tidak ada duplicate email

**Check via logs:**

```bash
GET /api/test/notifications/logs?limit=10
```

### **Scenario 4: Test Multi-Level Contract Reminder**

1. Create test contract dengan EndDate = today + 30 days
2. Trigger contract check
3. Verify email Level 2 (30 days) terkirim
4. Update EndDate = today + 7 days
5. Trigger lagi
6. Verify email Level 4 (7 days) terkirim (bukan Level 2 lagi)
7. Check logs untuk reminder level progression

### **Scenario 5: Test Real-time Budget Alert**

1. Check current budget status (GET `/stats`)
2. Create invoice yang akan cause overbudget
3. Submit invoice
4. Check email inbox immediately
5. Verify real-time alert received
6. Check logs

---

## 🧰 Quick Testing Tools

### **Using Swagger UI**

1. Navigate to: `http://localhost:5008/swagger`
2. Authorize dengan token
3. Expand `/api/test/notifications`
4. Try endpoints dengan UI yang mudah

### **Using cURL**

```bash
# Get token first
TOKEN=$(curl -X POST "http://localhost:5008/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"yourpassword"}' \
  | jq -r '.token')

# Preview in browser (copy URL)
echo "http://localhost:5008/api/test/notifications/preview/budget-alert"

# Send test email
curl -X POST "http://localhost:5008/api/test/notifications/send/budget-alert" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"email":"your@email.com"}'

# Check stats
curl -X GET "http://localhost:5008/api/test/notifications/stats" \
  -H "Authorization: Bearer $TOKEN"

# View logs
curl -X GET "http://localhost:5008/api/test/notifications/logs?limit=10" \
  -H "Authorization: Bearer $TOKEN"
```

### **Using Postman**

1. Import collection (create new)
2. Set environment variable:
   - `baseUrl`: `http://localhost:5008`
   - `token`: (get from login)
3. Create requests:
   - Preview Budget Alert (GET)
   - Send Test Budget (POST)
   - Send Test Contract (POST)
   - Trigger Budget Check (POST)
   - Trigger Contract Check (POST)
   - Get Stats (GET)
   - Get Logs (GET)

---

## 📊 Expected Results

### **Budget Alert Email Should Contain:**

- ✅ Subject: "BUDGET OVERRUN ALERT - [Period]"
- ✅ Icon: ⚠️
- ✅ Budget amount
- ✅ Spent amount
- ✅ Over budget amount & percentage
- ✅ Action button "View Budget Details"
- ✅ Professional styling

### **Contract Expiring Email Should Contain:**

- ✅ Subject: "[URGENCY]: Contract [No] Expires in X Days"
- ✅ Icon based on urgency (🚨 ⚠️ ⏰ 📅 📋)
- ✅ Contract number
- ✅ Vendor name
- ✅ End date
- ✅ Days remaining
- ✅ Action message
- ✅ Link to contract detail
- ✅ Professional styling

### **Notification Logs Should Show:**

- ✅ Notification type
- ✅ Reference ID
- ✅ Recipient email
- ✅ Timestamp
- ✅ Reminder level (for contracts)
- ✅ Details

---

## 🐛 Troubleshooting

### **Email not received?**

1. Check spam/junk folder
2. Verify SMTP settings in `appsettings.json`
3. Check application logs for errors
4. Test SMTP connection separately

### **Preview shows 404?**

1. Ensure app is running
2. Check you're logged in (for authorization)
3. Verify URL is correct

### **Trigger returns 500 error?**

1. Check application logs
2. Verify database connection
3. Check if SYS_NotificationLog table exists
4. Run migration if needed

### **No notifications sending (real-time)?**

1. Create invoice that causes overbudget
2. Check console logs for "Real-time budget check"
3. Verify anti-spam not blocking
4. Check notification logs

---

## ✅ Testing Checklist

- [ ] Preview budget alert template
- [ ] Preview contract expiring template (different days)
- [ ] Send test budget email to your inbox
- [ ] Send test contract email (3, 7, 14, 30, 60 days)
- [ ] Verify email styling looks good
- [ ] Check stats endpoint returns data
- [ ] Trigger budget check (real)
- [ ] Trigger contract check (real)
- [ ] Verify emails received
- [ ] Check notification logs populated
- [ ] Test anti-spam (trigger twice rapidly)
- [ ] Test real-time budget alert (create invoice)
- [ ] Verify multi-level contract reminders
- [ ] Test background services (wait for scheduled time)

---

## 📝 Notes

- **Safe Testing**: Use `/preview/*` dan `/send/*` endpoints (tidak affect production data)
- **Real Testing**: Use `/trigger/*` endpoints (kirim real emails!)
- **Monitoring**: Use `/stats` dan `/logs` untuk visibility
- **Anti-Spam**: System akan block duplicate dalam cooldown period
- **Background Services**: Akan auto-run pada scheduled time (8 AM & 9 AM)

Happy Testing! 🎉
