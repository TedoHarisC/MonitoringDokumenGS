# Monitoring Dokumen GS

Sistem monitoring dokumen kontrak dan invoice untuk ABB.

## 🚀 Quick Start

### Prerequisites

- .NET 8.0 SDK
- SQL Server
- SMTP Account (Gmail/Outlook untuk email notifications)
- Git (for version control)

### Installation

1. **Clone Repository**

   ```bash
   git clone <repository-url>
   cd MonitoringDokumenGS
   ```

2. **Setup User Secrets (Recommended for Development)**

   **IMPORTANT:** Never commit sensitive data to Git! Use User Secrets for development.

   ```bash
   # Initialize User Secrets (already done, ID: 056bda08-7e60-47e1-839b-edf6048ee244)
   dotnet user-secrets init

   # Set sensitive configuration
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=your-server;Database=DB_MONITORING_KONTRAK_GS;User Id=your-user;Password=your-password;TrustServerCertificate=True;MultipleActiveResultSets=true"

   dotnet user-secrets set "Jwt:Key" "your-secret-jwt-key-minimum-32-characters"

   dotnet user-secrets set "Email:Smtp:Password" "your-smtp-password"
   ```

   See: 📖 **[Security Documentation](docs/SECRETS_SETUP_GUIDE.md)** for complete setup guide

3. **Configure Environment-Specific Settings**

   Copy and customize for your environment:

   ```bash
   # Development (already configured in appsettings.Development.json)
   # Edit appsettings.Development.json for non-sensitive values:
   # - AppUrl
   # - FileStorage.RootPath
   # - Email.FromEmail, Email.Smtp.Host, Email.Smtp.Username
   ```

   **Note:** `appsettings.Development.json` is gitignored for security

4. **Run Application**

   ```bash
   dotnet build
   dotnet run
   ```

5. **Access Application**

   Open browser: `http://localhost:5008`

## 📚 Documentation

Dokumentasi lengkap tersedia di folder **[docs/](docs/README.md)**:

### 🔐 Security & Configuration

- 🔒 [Security README](docs/SECURITY_README.md) - **START HERE** for security overview
- 🔑 [Secrets Management Guide](docs/SECRETS_MANAGEMENT.md) - Complete security practices
- ⚙️ [Secrets Setup Guide](docs/SECRETS_SETUP_GUIDE.md) - Step-by-step User Secrets setup
- 🚀 [Production Environment Template](docs/PRODUCTION_ENV_TEMPLATE.md) - Deploy to production
- 📝 [Secrets Quick Reference](docs/SECRETS_QUICK_REFERENCE.txt) - Common commands

### 📖 Feature Documentation

- 🔐 [Forgot Password Guide](docs/FORGOT_PASSWORD_GUIDE.md) - Reset password via email
- 📧 [Email System Guide](docs/EMAIL_USAGE_GUIDE.md) - SMTP configuration & usage
- 📧 [Email Templates Guide](docs/EMAIL_TEMPLATES_GUIDE.md) - Email template documentation
- 🔔 [Notification Guide](docs/NOTIFICATION_PAGE_GUIDE.md) - Notification management
- 👤 [Avatar User Guide](docs/AVATAR_USER_GUIDE.md) - User avatar with initials
- 💰 [Budget Feature](docs/BUDGET_FEATURE.md) - Budget management system

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

### Authentication & Authorization

- Cookie-based authentication with Claims
- JWT tokens for API authentication
- Role-based authorization (Super Admin, Admin, User)
- Vendor-based data isolation for regular users
- Password hashing with BCrypt

### Secure Configuration Management

- ✅ **User Secrets** for development (passwords, JWT keys, connection strings)
- ✅ **Environment Variables** recommended for production
- ✅ **appsettings.json** contains NO sensitive data (safe for Git)
- ✅ **appsettings.Development.json** & **appsettings.Production.json** are gitignored
- ✅ **.gitignore** configured to prevent accidental commits

### Best Practices

- Never commit passwords, API keys, or connection strings to Git
- Use different credentials for development, staging, and production
- Rotate secrets regularly
- Use strong JWT keys (minimum 32 characters)

**📖 See [Security Documentation](docs/SECURITY_README.md) for complete guidelines**

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
├── appsettings.json     # Base configuration (NO secrets)
├── appsettings.Development.json  # Dev overrides (gitignored)
├── appsettings.Production.json   # Prod overrides (gitignored)
├── ROLLBACK_BACKUP.txt  # Configuration rollback guide
└── .gitignore           # Git exclusions (secrets, build outputs)
```

### Configuration Files

- **appsettings.json** - Base template, safe for Git (no sensitive data)
- **appsettings.Development.json** - Development environment (gitignored)
- **appsettings.Production.json** - Production environment (gitignored)
- **User Secrets** - Sensitive data storage (~/.microsoft/usersecrets/)

Configuration priority (highest to lowest):

1. Command-line arguments
2. Environment variables
3. User Secrets (Development only)
4. appsettings.{Environment}.json
5. appsettings.json

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
