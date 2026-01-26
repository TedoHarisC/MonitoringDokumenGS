# 📚 Dokumentasi - Monitoring Dokumen GS

Selamat datang di dokumentasi lengkap untuk sistem Monitoring Dokumen GS. Berikut adalah panduan untuk berbagai fitur yang tersedia.

## 📋 Daftar Dokumentasi

### 🔐 Authentication & Security

- **[Forgot Password Guide](FORGOT_PASSWORD_GUIDE.md)**  
  Panduan lengkap fitur forgot password dan reset password dengan email integration
  - Flow process forgot password
  - Email template integration
  - API endpoints
  - Testing guide
  - Security considerations

- **[Avatar User Guide](AVATAR_USER_GUIDE.md)**  
  Panduan penggunaan avatar user dengan initial huruf
  - Dynamic avatar generation
  - Layout personalization
  - Implementation details

### 📧 Email System

- **[Email Usage Guide](EMAIL_USAGE_GUIDE.md)**  
  Panduan komprehensif penggunaan email system
  - SMTP configuration (Gmail, Outlook, Office365)
  - Service integration
  - Implementation examples
  - Testing guide
  - Best practices

- **[Email Templates Guide](EMAIL_TEMPLATES_GUIDE.md)**  
  Dokumentasi lengkap email templates yang tersedia
  - 7 email templates (Notification, Invoice, Contract, Welcome, Password Reset, Approval)
  - Template structure
  - Customization guide
  - Compatibility notes

- **[Email Quick Reference](EMAIL_QUICK_REFERENCE.md)**  
  Quick reference card untuk email templates
  - Template names & usage
  - Method signatures
  - Quick examples

- **[Email SMTP Testing Guide](EMAIL_SMTP_TESTING_GUIDE.md)**  
  Panduan testing SMTP configuration
  - Testing page di Settings > Notification
  - SMTP configuration viewer
  - Test email dengan berbagai templates
  - Troubleshooting

### 🔔 Notification System

- **[Notification Page Guide](NOTIFICATION_PAGE_GUIDE.md)**  
  Panduan halaman management notifikasi
  - Notification management page
  - Filter tabs (All, Unread, Read)
  - Mark as read & delete actions
  - Auto-refresh

- **[Notification Guide](NOTIFICATION_GUIDE.md)**  
  Panduan umum notification system
  - Notification service
  - Create & manage notifications
  - Integration with other modules

## 🎯 Quick Start

### Untuk Developer

1. **Setup Email System**
   - Baca [Email Usage Guide](EMAIL_USAGE_GUIDE.md)
   - Configure SMTP di `appsettings.json`
   - Test menggunakan [Email SMTP Testing Guide](EMAIL_SMTP_TESTING_GUIDE.md)

2. **Implementasi Notification**
   - Baca [Notification Guide](NOTIFICATION_GUIDE.md)
   - Gunakan `INotifications` service
   - Test di halaman [Notification Page](NOTIFICATION_PAGE_GUIDE.md)

3. **Setup Authentication**
   - Baca [Forgot Password Guide](FORGOT_PASSWORD_GUIDE.md)
   - Configure email templates
   - Test forgot password flow

### Untuk End User

1. **Manage Notifications**
   - Buka Settings > Notification
   - Lihat panduan: [Notification Page Guide](NOTIFICATION_PAGE_GUIDE.md)

2. **Forgot Password**
   - Klik "Forget password?" di login page
   - Ikuti panduan: [Forgot Password Guide](FORGOT_PASSWORD_GUIDE.md)

## 🔧 Technical Stack

- **Backend:** ASP.NET Core 8.0
- **Database:** SQL Server
- **Authentication:** Cookie-based with Claims
- **Email:** SMTP (System.Net.Mail)
- **Frontend:** Razor Views, jQuery, Bootstrap 5

## 📞 Support

Untuk pertanyaan atau issues, hubungi tim development ABB.

---

**Last Updated:** January 26, 2026  
**Version:** 1.0  
**Project:** Monitoring Dokumen GS - ABB
