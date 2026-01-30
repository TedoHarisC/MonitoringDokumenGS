# Quick Setup - Secrets Configuration

## ⚡ For New Team Members

If you just cloned this repository, follow these steps:

### 1. Initialize User Secrets

```bash
cd "MonitoringDokumenGS"
dotnet user-secrets init
```

### 2. Configure Email Settings

```bash
# Set your email password
dotnet user-secrets set "Email:Smtp:Password" "YOUR_EMAIL_PASSWORD"

# Optional: Update other email settings if different
dotnet user-secrets set "Email:Smtp:Host" "smtp.gmail.com"
dotnet user-secrets set "Email:Smtp:Port" "587"
dotnet user-secrets set "Email:Smtp:Username" "your.email@gmail.com"
```

### 3. Configure Database

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=YOUR_SERVER;Database=DB_MONITORING_KONTRAK_GS;User Id=YOUR_USER;Password=YOUR_PASSWORD;Trusted_Connection=False;MultipleActiveResultSets=true;Encrypt=True;TrustServerCertificate=True;"
```

### 4. Configure JWT

```bash
# Generate a strong secret key
dotnet user-secrets set "Jwt:Key" "$(openssl rand -base64 32)"

# Or use a custom key (minimum 32 characters)
dotnet user-secrets set "Jwt:Key" "your-super-secret-jwt-key-here-min-32-chars"
```

### 5. Verify Configuration

```bash
dotnet user-secrets list
```

You should see:

```
ConnectionStrings:DefaultConnection = Server=...
Email:Smtp:Password = ***
Jwt:Key = ***
```

### 6. Run the Application

```bash
dotnet run
```

---

## 🎯 For Gmail Users

If using Gmail, you need an **App Password** (not your regular password):

1. Go to Google Account: https://myaccount.google.com/security
2. Enable 2-Step Verification
3. Go to App Passwords: https://myaccount.google.com/apppasswords
4. Create password for "Mail" app
5. Use that 16-character password

```bash
dotnet user-secrets set "Email:Smtp:Password" "abcd efgh ijkl mnop"
dotnet user-secrets set "Email:Smtp:Host" "smtp.gmail.com"
dotnet user-secrets set "Email:Smtp:Port" "587"
dotnet user-secrets set "Email:Smtp:Username" "your.email@gmail.com"
```

---

## 🔧 For Outlook/Office365 Users

```bash
dotnet user-secrets set "Email:Smtp:Host" "smtp.office365.com"
dotnet user-secrets set "Email:Smtp:Port" "587"
dotnet user-secrets set "Email:Smtp:Username" "your.email@outlook.com"
dotnet user-secrets set "Email:Smtp:Password" "your_password"
```

---

## ❓ Troubleshooting

**Issue: Secrets not loading**

```bash
# Check if UserSecretsId exists in .csproj
cat MonitoringDokumenGS.csproj | grep UserSecretsId

# Verify secrets file exists
# macOS/Linux:
cat ~/.microsoft/usersecrets/*/secrets.json

# Windows:
type %APPDATA%\Microsoft\UserSecrets\*\secrets.json
```

**Issue: SMTP connection failed**

- Check firewall/antivirus
- Verify SMTP credentials
- Try telnet: `telnet smtp.gmail.com 587`

---

## 📞 Need Help?

Contact the project maintainer or check [SECRETS_MANAGEMENT.md](./SECRETS_MANAGEMENT.md) for detailed documentation.
