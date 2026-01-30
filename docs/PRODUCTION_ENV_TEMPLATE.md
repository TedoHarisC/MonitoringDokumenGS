# Production Deployment - Environment Variables

## ⚠️ NEVER COMMIT THIS FILE TO GIT!

This is a template for production environment variables.
Copy this file and rename to `.env.production` (already in .gitignore)

---

## 🔐 Required Secrets

### JWT Configuration

JWT\_\_KEY=your-production-jwt-secret-key-minimum-32-characters-long

### Database Connection

CONNECTIONSTRINGS\_\_DEFAULTCONNECTION=Server=production-server;Database=DB_MONITORING_KONTRAK_GS;User Id=prod_user;Password=PROD_DB_PASSWORD;Trusted_Connection=False;MultipleActiveResultSets=true;Encrypt=True;TrustServerCertificate=True;

### Email SMTP Configuration

EMAIL**SMTP**HOST=smtp.company.com
EMAIL**SMTP**PORT=587
EMAIL**SMTP**USERNAME=noreply@company.com
EMAIL**SMTP**PASSWORD=PROD_EMAIL_PASSWORD
EMAIL**SMTP**USESSL=true
EMAIL**FROMNAME=Monitoring GS Production
EMAIL**FROMEMAIL=noreply@company.com
EMAIL\_\_PROVIDER=SMTP

### Application Settings

APPURL=https://monitoring-gs.company.com
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:5000

---

## 📝 How to Use

### Option 1: Docker

```bash
docker run -d \
  --name monitoring-gs \
  --env-file .env.production \
  -p 5000:5000 \
  monitoring-gs:latest
```

### Option 2: Docker Compose

```yaml
# docker-compose.yml
version: "3.8"
services:
  app:
    image: monitoring-gs:latest
    env_file:
      - .env.production
    ports:
      - "5000:5000"
```

### Option 3: Systemd Service (Linux)

```ini
# /etc/systemd/system/monitoring-gs.service
[Unit]
Description=Monitoring GS Application

[Service]
WorkingDirectory=/var/www/monitoring-gs
ExecStart=/usr/bin/dotnet /var/www/monitoring-gs/MonitoringDokumenGS.dll
Restart=always
RestartSec=10
SyslogIdentifier=monitoring-gs
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=JWT__KEY=prod_jwt_key
Environment=EMAIL__SMTP__PASSWORD=prod_email_pass
EnvironmentFile=/etc/monitoring-gs/secrets.env

[Install]
WantedBy=multi-user.target
```

### Option 4: IIS (Windows)

1. Open IIS Manager
2. Select your application
3. Configuration Editor
4. Section: `system.webServer/aspNetCore`
5. Add environment variables:
   - JWT\_\_KEY
   - EMAIL**SMTP**PASSWORD
   - CONNECTIONSTRINGS\_\_DEFAULTCONNECTION

---

## 🔒 Security Best Practices

1. **Restrict File Permissions**

   ```bash
   chmod 600 .env.production
   chown www-data:www-data .env.production
   ```

2. **Use Secrets Manager** (Recommended for Production)
   - Azure: Azure Key Vault
   - AWS: AWS Secrets Manager
   - GCP: Google Secret Manager
3. **Rotate Secrets Regularly**
   - Database passwords: Every 90 days
   - JWT keys: Every 180 days
   - API keys: Every 180 days

4. **Monitor Access**
   - Enable audit logging
   - Set up alerts for secret access
   - Review access logs regularly

---

## 🚀 Deployment Checklist

- [ ] Secrets configured in production environment
- [ ] .env.production file has correct permissions (600)
- [ ] Database connection tested
- [ ] Email SMTP tested
- [ ] JWT authentication tested
- [ ] HTTPS enabled with valid certificate
- [ ] Firewall rules configured
- [ ] Monitoring and logging enabled
- [ ] Backup strategy in place
- [ ] Rollback plan documented

---

## 📞 Emergency Contacts

**If secrets are compromised:**

1. Immediately rotate all affected secrets
2. Notify security team
3. Review access logs
4. Update deployment with new secrets
5. Document incident
