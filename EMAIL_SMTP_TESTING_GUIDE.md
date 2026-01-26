# Email SMTP Testing Guide

## Overview

Fitur testing SMTP email telah ditambahkan ke halaman **Settings > Notification** untuk memudahkan testing konfigurasi email sebelum digunakan di production.

## Lokasi

**Menu:** Settings > Notification  
**URL:** `/Notification/Index`  
**Posisi:** Di bawah table notifikasi

## Fitur

### 1. Send Test Email

Mengirim test email dengan template yang dipilih ke alamat email yang diinginkan.

**Form Fields:**

- **Recipient Email Address** (Required): Email tujuan untuk menerima test email
- **Email Template**: Pilihan template yang akan dikirim
  - General Notification (default)
  - Invoice Created
  - Contract Created
  - Welcome Email

**Button Actions:**

- **Send Test Email**: Mengirim test email
- **Check SMTP Config**: Melihat konfigurasi SMTP saat ini

### 2. Test Result Display

Setelah mengirim test email, akan muncul alert box dengan informasi:

- ✅ **Success**: Email berhasil dikirim
- ❌ **Failed**: Email gagal dikirim dengan detail error

### 3. SMTP Configuration Viewer

Melihat konfigurasi SMTP yang sedang digunakan (tanpa menampilkan password/username):

- SMTP Provider
- SMTP Host
- SMTP Port
- From Email
- From Name
- Use SSL (Yes/No)

## API Endpoints

### 1. Send Test Email

```
POST /api/test/email/send
Content-Type: application/json

{
  "email": "recipient@example.com"
}
```

**Response Success:**

```json
{
  "success": true,
  "message": "Test email sent successfully to recipient@example.com",
  "timestamp": "2026-01-26T10:30:00"
}
```

**Response Error:**

```json
{
  "success": false,
  "message": "Failed to send email",
  "error": "SMTP connection failed",
  "details": "Unable to connect to SMTP server"
}
```

### 2. Send Test Invoice Email

```
POST /api/test/email/send-invoice
Content-Type: application/json

{
  "email": "recipient@example.com"
}
```

### 3. Send Test Contract Email

```
POST /api/test/email/send-contract
Content-Type: application/json

{
  "email": "recipient@example.com"
}
```

### 4. Send Test Welcome Email

```
POST /api/test/email/send-welcome
Content-Type: application/json

{
  "email": "recipient@example.com",
  "name": "Test User"
}
```

### 5. Get SMTP Configuration

```
GET /api/test/email/config
```

**Response:**

```json
{
  "success": true,
  "data": {
    "provider": "Gmail",
    "fromName": "ABB Monitoring System",
    "fromEmail": "noreply@monitoringdokumen.com",
    "host": "smtp.gmail.com",
    "port": 587,
    "useSsl": true,
    "isConfigured": true
  },
  "message": "SMTP configuration retrieved successfully"
}
```

## Cara Menggunakan

### Step 1: Buka Halaman Notification

1. Login ke aplikasi
2. Klik menu **Settings** di sidebar
3. Pilih **Notification**
4. Scroll ke bawah, akan terlihat card "Email SMTP Testing"

### Step 2: Test SMTP Configuration (Optional)

1. Klik button **"Check SMTP Config"**
2. Tunggu beberapa saat
3. Akan muncul tabel dengan informasi konfigurasi SMTP
4. Pastikan semua data sudah terisi dengan benar

### Step 3: Send Test Email

1. Masukkan email address tujuan di field **"Recipient Email Address"**
2. Pilih template yang ingin ditest (default: General Notification)
3. Klik button **"Send Test Email"**
4. Tunggu proses pengiriman (button akan disabled dengan loading spinner)
5. Setelah selesai, akan muncul alert:
   - **Success**: Email berhasil dikirim, cek inbox
   - **Failed**: Email gagal dikirim dengan detail error

### Step 4: Verify Email

1. Buka inbox email yang dituju
2. Cek apakah email masuk (termasuk folder Spam/Junk)
3. Buka email dan pastikan template tampil dengan baik
4. Klik action button untuk test link (jika ada)

## Troubleshooting

### Error: "Failed to send email"

**Penyebab:**

- SMTP credentials tidak valid
- SMTP host/port salah
- Firewall memblokir koneksi
- SSL/TLS configuration salah

**Solusi:**

1. Cek konfigurasi di `appsettings.json`
2. Pastikan credentials benar
3. Coba gunakan App Password (untuk Gmail)
4. Pastikan port dan SSL setting benar

### Error: "Email is required"

**Penyebab:**

- Field email address kosong

**Solusi:**

- Isi field email address dengan valid email

### Email Tidak Masuk ke Inbox

**Penyebab:**

- Email masuk ke folder Spam/Junk
- Email address typo
- SMTP delay

**Solusi:**

1. Cek folder Spam/Junk
2. Tunggu beberapa menit (SMTP bisa delay)
3. Pastikan email address benar
4. Test dengan email address lain

### Configuration Not Loaded

**Penyebab:**

- Endpoint `/api/test/email/config` tidak tersedia
- SMTP belum dikonfigurasi di `appsettings.json`

**Solusi:**

1. Pastikan `appsettings.json` memiliki section `Email`
2. Restart aplikasi setelah ubah configuration
3. Check application logs untuk error

## SMTP Configuration Reference

### Gmail

```json
{
  "Email": {
    "Provider": "Gmail",
    "FromName": "Your App Name",
    "FromEmail": "your-email@gmail.com",
    "Smtp": {
      "Host": "smtp.gmail.com",
      "Port": 587,
      "Username": "your-email@gmail.com",
      "Password": "your-app-password",
      "UseSsl": true
    }
  }
}
```

**Note:** Untuk Gmail, gunakan App Password, bukan password akun.  
Generate di: https://myaccount.google.com/apppasswords

### Outlook/Office365

```json
{
  "Email": {
    "Provider": "Outlook",
    "FromName": "Your App Name",
    "FromEmail": "your-email@outlook.com",
    "Smtp": {
      "Host": "smtp-mail.outlook.com",
      "Port": 587,
      "Username": "your-email@outlook.com",
      "Password": "your-password",
      "UseSsl": true
    }
  }
}
```

## Files Modified

### 1. View

- **File:** `/Views/Notification/Index.cshtml`
- **Changes:** Added email SMTP testing section with form, result display, and config viewer

### 2. JavaScript

- **File:** `/wwwroot/js/notification-page.js`
- **Changes:** Added functions:
  - `sendTestEmail()` - Send test email via AJAX
  - `showTestResult()` - Display test result (success/failed)
  - `checkSmtpConfig()` - Get SMTP configuration
  - `displaySmtpConfig()` - Display SMTP config in table

### 3. API Controller

- **File:** `/Controllers/API/EmailTestController.cs`
- **Changes:**
  - Added `IOptions<EmailOptions>` dependency injection
  - Added `GetSmtpConfiguration()` endpoint (GET /api/test/email/config)
  - Returns SMTP config without sensitive data (username/password hidden)

## Security Notes

⚠️ **Important:**

- Password dan username SMTP **TIDAK** ditampilkan di UI untuk security
- Endpoint hanya menampilkan informasi non-sensitive (host, port, from email, etc.)
- Test email hanya bisa dikirim ke email yang diinput user (tidak bisa bulk spam)
- Sebaiknya test email feature ini hanya accessible untuk Admin role (add authorization jika perlu)

## Best Practices

1. **Test Sebelum Production**
   - Selalu test SMTP sebelum deploy ke production
   - Test dengan berbagai email provider (Gmail, Outlook, Yahoo)

2. **Monitor Logs**
   - Check application logs untuk error details
   - Log akan mencatat setiap email yang berhasil/gagal dikirim

3. **Limit Rate**
   - Jangan spam test email terlalu banyak
   - SMTP provider bisa block jika terlalu banyak email dalam waktu singkat

4. **Production Configuration**
   - Gunakan environment variables atau Azure Key Vault untuk credentials
   - Jangan commit credentials ke Git

## Next Steps

Jika test email berhasil, Anda bisa integrate email ke services:

1. **InvoiceService** - Send email saat invoice dibuat/diupdate
2. **ContractService** - Send email saat contract dibuat/diupdate
3. **NotificationService** - Send email notification ke user
4. **AuthService** - Send welcome email dan password reset

Refer to: **EMAIL_USAGE_GUIDE.md** untuk implementasi detail.

---

**Created:** 2026-01-26  
**Version:** 1.0  
**Author:** ABB Development Team
