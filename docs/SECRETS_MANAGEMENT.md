# 🔐 Secrets Management Guide

## ⚠️ SECURITY WARNING

**NEVER commit sensitive data to Git!** This includes:

- Passwords
- Connection strings with credentials
- JWT keys
- API keys
- Email passwords
- Any other secrets

---

## 📋 Best Practices by Environment

### 🛠️ Development (Local Machine)

#### Option 1: User Secrets (Recommended for ASP.NET Core)

**Setup:**

```bash
# Initialize user secrets
dotnet user-secrets init

# Set individual secrets
dotnet user-secrets set "Email:Smtp:Password" "your_actual_password"
dotnet user-secrets set "Jwt:Key" "your_jwt_secret_key"
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=...;Password=...;"

# List all secrets
dotnet user-secrets list

# Remove a secret
dotnet user-secrets remove "Email:Smtp:Password"

# Clear all secrets
dotnet user-secrets clear
```

**Location:** Secrets are stored in:

- **Windows:** `%APPDATA%\Microsoft\UserSecrets\<user_secrets_id>\secrets.json`
- **macOS/Linux:** `~/.microsoft/usersecrets/<user_secrets_id>/secrets.json`

**Advantages:**

- ✅ Never committed to Git
- ✅ Specific to your machine
- ✅ Easy to manage
- ✅ Built into .NET

#### Option 2: Environment Variables

```bash
# macOS/Linux (add to ~/.zshrc or ~/.bashrc)
export Email__Smtp__Password="your_password"
export Jwt__Key="your_jwt_key"
export ConnectionStrings__DefaultConnection="Server=...;Password=...;"

# Windows PowerShell
$env:Email__Smtp__Password="your_password"
$env:Jwt__Key="your_jwt_key"

# Windows CMD
set Email__Smtp__Password=your_password
```

**Note:** Use double underscore `__` to represent nested configuration (e.g., `Email:Smtp:Password` becomes `Email__Smtp__Password`)

---

### 🚀 Production

#### Option 1: Environment Variables (Recommended for Servers)

**Linux/Docker:**

```bash
# Set in /etc/environment or systemd service file
Email__Smtp__Password=production_password
Jwt__Key=production_jwt_key
ConnectionStrings__DefaultConnection="Server=prod;Password=prod_pass;"
```

**Docker Compose:**

```yaml
version: "3.8"
services:
  app:
    image: monitoring-gs:latest
    environment:
      - Email__Smtp__Password=${SMTP_PASSWORD}
      - Jwt__Key=${JWT_KEY}
      - ConnectionStrings__DefaultConnection=${DB_CONNECTION}
    env_file:
      - .env.production # Never commit this file!
```

**IIS:**

1. Open IIS Manager
2. Select your application
3. Configuration Editor → `system.webServer/aspNetCore`
4. Add environment variables in `environmentVariables` section

#### Option 2: Azure Key Vault (Enterprise)

```bash
# Install package
dotnet add package Azure.Extensions.AspNetCore.Configuration.Secrets
```

```csharp
// Program.cs
builder.Configuration.AddAzureKeyVault(
    new Uri($"https://{keyVaultName}.vault.azure.net/"),
    new DefaultAzureCredential());
```

#### Option 3: AWS Secrets Manager

```bash
dotnet add package Amazon.Extensions.Configuration.SystemsManager
```

---

### 🏢 Team Development

#### Option 1: appsettings.Development.json (Gitignored)

Create `appsettings.Development.json` with real values (this file is in .gitignore):

```json
{
  "Jwt": {
    "Key": "actual_dev_key"
  },
  "Email": {
    "Smtp": {
      "Password": "actual_dev_password"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=dev;Password=dev_pass;"
  }
}
```

**Share template** in documentation or separate file:

```json
// appsettings.Development.json.template
{
  "Jwt": {
    "Key": "YOUR_JWT_KEY_HERE"
  },
  "Email": {
    "Smtp": {
      "Password": "YOUR_EMAIL_PASSWORD_HERE"
    }
  }
}
```

#### Option 2: Shared Key Vault for Team

Use Azure Key Vault or AWS Secrets Manager with proper IAM roles for team access.

---

## 🔧 Current Configuration

### Secrets Already Configured:

✅ **Email Password:** Stored in User Secrets  
✅ **Database Connection String:** Stored in User Secrets  
✅ **JWT Secret Key:** Stored in User Secrets

### Commands to Update:

```bash
# Update email password
dotnet user-secrets set "Email:Smtp:Password" "new_password"

# Update database connection
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=10.2.182.220;Database=DB_MONITORING_KONTRAK_GS;User Id=sa;Password=NEW_PASSWORD;Trusted_Connection=False;MultipleActiveResultSets=true;Encrypt=True;TrustServerCertificate=True;"

# Update JWT key (generate strong key)
dotnet user-secrets set "Jwt:Key" "$(openssl rand -base64 32)"
```

---

## 📝 Configuration Priority (ASP.NET Core)

ASP.NET Core loads configuration in this order (later sources override earlier):

1. `appsettings.json`
2. `appsettings.{Environment}.json`
3. **User Secrets** (Development only)
4. **Environment Variables**
5. **Command-line arguments**

Example:

```
appsettings.json: Password = "PLACEHOLDER"
User Secrets:     Password = "dev_password"      ← This wins in Development!
```

---

## 🛡️ Security Checklist

- [ ] ✅ Secrets removed from `appsettings.json`
- [ ] ✅ `.gitignore` includes sensitive files
- [ ] ✅ User Secrets configured for development
- [ ] ✅ Environment variables ready for production
- [ ] ❌ Never share secrets via email/chat
- [ ] ❌ Never log sensitive data
- [ ] ❌ Never expose secrets in error messages
- [ ] ✅ Rotate secrets regularly
- [ ] ✅ Use strong, unique passwords

---

## 🔍 Verify Configuration

Run the application and check if it's using User Secrets:

```bash
# Build and run
dotnet run

# Check what values are being loaded (add logging in Startup)
builder.Services.AddLogging(logging => {
    logging.AddConsole();
});
```

To verify secrets are loaded without exposing them:

```csharp
// Startup.cs or Program.cs (for debugging only, remove after verification)
var emailPassword = builder.Configuration["Email:Smtp:Password"];
var isFromSecrets = !string.IsNullOrEmpty(emailPassword) &&
                    emailPassword != "REPLACE_WITH_USER_SECRETS_OR_ENV_VAR";
Console.WriteLine($"Secrets loaded: {isFromSecrets}");
```

---

## 📚 Additional Resources

- [Safe Storage of App Secrets in ASP.NET Core](https://docs.microsoft.com/aspnet/core/security/app-secrets)
- [Configuration in ASP.NET Core](https://docs.microsoft.com/aspnet/core/fundamentals/configuration)
- [Azure Key Vault Configuration Provider](https://docs.microsoft.com/aspnet/core/security/key-vault-configuration)

---

## ⚡ Quick Commands Reference

```bash
# Initialize
dotnet user-secrets init

# Set secrets
dotnet user-secrets set "Key:Path" "Value"

# List all secrets
dotnet user-secrets list

# Remove specific secret
dotnet user-secrets remove "Key:Path"

# Clear all secrets
dotnet user-secrets clear

# Generate strong password (macOS/Linux)
openssl rand -base64 32
```

---

**Remember:** Security is not a one-time setup. Regularly review and update your secrets management strategy! 🔐
