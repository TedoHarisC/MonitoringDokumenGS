# Monitoring Dokumen GS

Sistem monitoring dokumen kontrak dan invoice untuk ABB.

## 🚀 Quick Start

### Prerequisites

- .NET 8.0 SDK
- SQL Server
- SMTP Account (Gmail/Outlook untuk email notifications)

### Installation

1. **Clone Repository**

   ```bash
   git clone <repository-url>
   cd MonitoringDokumenGS
   ```

2. **Configure Database**

   Update connection string di `appsettings.json`:

   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=your-server;Database=DB_MONITORING_KONTRAK_GS;..."
     }
   }
   ```

3. **Configure Email (Optional)**

   Update SMTP settings di `appsettings.json`:

   ```json
   {
     "Email": {
       "Provider": "Gmail",
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

4. **Run Application**

   ```bash
   dotnet build
   dotnet run
   ```

5. **Access Application**

   Open browser: `http://localhost:5170`

## 📚 Documentation

Dokumentasi lengkap tersedia di folder **[docs/](docs/README.md)**:

- 🔐 [Forgot Password Guide](docs/FORGOT_PASSWORD_GUIDE.md) - Reset password via email
- 📧 [Email System Guide](docs/EMAIL_USAGE_GUIDE.md) - SMTP configuration & usage
- 📧 [Email Templates Guide](docs/EMAIL_TEMPLATES_GUIDE.md) - Email template documentation
- 🔔 [Notification Guide](docs/NOTIFICATION_PAGE_GUIDE.md) - Notification management
- 👤 [Avatar User Guide](docs/AVATAR_USER_GUIDE.md) - User avatar with initials

## 🎯 Features

### For Super Admin / Admin

- ✅ View all transactions (all vendors)
- ✅ Manage invoices & contracts
- ✅ Manage master data (vendors, statuses, categories)
- ✅ Delete records
- ✅ Access all notifications

### For Regular Users

- ✅ View transactions from their vendor only
- ✅ Create/update invoices & contracts for their vendor
- ✅ Receive notifications
- ✅ Manage their profile

### System Features

- ✅ Role-based access control (Super Admin, Admin, User)
- ✅ Email notifications (Invoice, Contract, Password Reset, Welcome)
- ✅ Notification management page
- ✅ Forgot password with email
- ✅ User avatar with initials
- ✅ Audit logging
- ✅ File attachments

## 🔐 Security

- Cookie-based authentication with Claims
- Role-based authorization
- Vendor-based data isolation for regular users
- Password hashing with BCrypt
- SMTP email validation

## 🛠️ Tech Stack

- **Backend:** ASP.NET Core 8.0 MVC
- **Database:** SQL Server with Entity Framework Core
- **Authentication:** Cookie Authentication + JWT (for API)
- **Frontend:** Razor Views, jQuery, Bootstrap 5, DataTables
- **Email:** System.Net.Mail with HTML templates

## 📁 Project Structure

```
MonitoringDokumenGS/
├── Controllers/          # MVC Controllers
│   ├── API/             # API Controllers
│   └── Web/             # Web Controllers
├── Models/              # Data models
│   ├── Auth/            # Authentication models
│   ├── Master/          # Master data models
│   └── Transaction/     # Transaction models
├── Services/            # Business logic services
├── Views/               # Razor views
├── wwwroot/             # Static files (CSS, JS, images)
├── EmailTemplates/      # HTML email templates
├── docs/                # 📚 Documentation
└── appsettings.json     # Configuration
```

## 🧪 Testing

### Test SMTP Configuration

1. Login as admin
2. Go to Settings > Notification
3. Scroll to "Email SMTP Testing" section
4. Enter test email address
5. Click "Send Test Email"

See: [Email SMTP Testing Guide](docs/EMAIL_SMTP_TESTING_GUIDE.md)

### Test Forgot Password

1. Go to login page
2. Click "Forget password?"
3. Enter username & email
4. Check email inbox
5. Click reset link and set new password

See: [Forgot Password Guide](docs/FORGOT_PASSWORD_GUIDE.md)

## 📝 License

Copyright © 2026 ABB. All rights reserved.

## 👥 Team

ABB Development Team

---

**For detailed documentation, visit [docs/](docs/README.md)**
