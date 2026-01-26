# Forgot Password & Reset Password Guide

## Overview

Fitur Forgot Password memungkinkan user yang lupa password untuk mereset password mereka melalui email. Sistem akan mengirim email dengan link reset password yang valid selama 1 jam.

## ✅ Features Implemented

### 1. **Forgot Password Page**

- **URL:** `/Auth/ForgotPassword`
- **Description:** Halaman untuk request password reset
- **Form Fields:**
  - Username (required)
  - Email Address (required)
- **Actions:** Mengirim reset link via email

### 2. **Reset Password Page**

- **URL:** `/Auth/ResetPassword?token={token}`
- **Description:** Halaman untuk set password baru dengan token
- **Form Fields:**
  - New Password (min 6 characters, required)
  - Confirm Password (required)
- **Features:**
  - Toggle password visibility (show/hide)
  - Password match validation
  - Token validation

### 3. **Email Integration**

- Menggunakan template `PasswordReset.html` yang sudah ada
- Email dikirim otomatis saat user request forgot password
- Email berisi:
  - User name
  - Reset link dengan token
  - Expiration time (1 hour)
  - Security contact email

### 4. **Backend API**

- **POST** `/api/auth/forgot-password` - Generate reset token & send email
- **POST** `/api/auth/reset-password` - Reset password dengan token

## 🔄 Flow Process

```
┌─────────────┐
│ Login Page  │
└──────┬──────┘
       │ Click "Forget password?"
       ▼
┌─────────────────────┐
│ Forgot Password Page│
└──────┬──────────────┘
       │ Submit username & email
       ▼
┌──────────────────────┐
│  Backend validates   │
│  & generates token   │
└──────┬───────────────┘
       │
       ▼
┌──────────────────────┐
│  Send email with     │
│  reset link + token  │
└──────┬───────────────┘
       │
       ▼
┌──────────────────────┐
│  User clicks link    │
│  in email            │
└──────┬───────────────┘
       │
       ▼
┌──────────────────────┐
│ Reset Password Page  │
│ (with token)         │
└──────┬───────────────┘
       │ Submit new password
       ▼
┌──────────────────────┐
│  Backend validates   │
│  token & resets pwd  │
└──────┬───────────────┘
       │
       ▼
┌──────────────────────┐
│  Redirect to Login   │
│  with success msg    │
└──────────────────────┘
```

## 📁 Files Created/Modified

### Views

1. **`/Views/Auth/ForgotPassword.cshtml`** (NEW)
   - Form untuk request password reset
   - Input: username & email
   - Submit ke API endpoint

2. **`/Views/Auth/ResetPassword.cshtml`** (NEW)
   - Form untuk set password baru
   - Input: new password & confirm password
   - Token validation
   - Password visibility toggle

3. **`/Views/Auth/Index.cshtml`** (MODIFIED)
   - Updated link "Forget password?" → `/Auth/ForgotPassword`

### Controllers

4. **`/Controllers/Web/AuthController.cs`** (MODIFIED)
   - Added `ForgotPassword()` action (GET)
   - Added `ResetPassword(string token)` action (GET)

### Services

5. **`/Services/Auth/AuthService.cs`** (MODIFIED)
   - Updated `GenerateResetTokenAsync()` to send email
   - Integrated with `IEmailService`
   - Uses `EmailTemplateHelper.GetPasswordResetEmail()`

### JavaScript

6. **`/wwwroot/js/auth-forgot-password.js`** (NEW)
   - Form validation & submission
   - API call to `/api/auth/forgot-password`
   - Success/error handling
   - Email validation

7. **`/wwwroot/js/auth-reset-password.js`** (NEW)
   - Form validation & submission
   - Password match validation
   - API call to `/api/auth/reset-password`
   - Password toggle visibility
   - Success/error handling

### Configuration

8. **`/appsettings.json`** (MODIFIED)
   - Added `AppUrl` configuration for reset link generation

## 🔧 Configuration

### appsettings.json

```json
{
  "AppUrl": "http://localhost:5170",
  "Email": {
    "Provider": "Gmail",
    "FromName": "ABB Monitoring System",
    "FromEmail": "noreply@company.com",
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

**Production:**

- Update `AppUrl` to production URL (e.g., `https://monitoring.company.com`)
- Configure SMTP credentials properly
- Use environment variables for sensitive data

## 📝 API Documentation

### 1. Forgot Password (Generate Reset Token)

**Endpoint:** `POST /api/auth/forgot-password`

**Request:**

```json
{
  "username": "john.doe",
  "email": "john.doe@company.com"
}
```

**Response Success (200):**

```json
{
  "success": true,
  "message": "Reset token generated",
  "data": {
    "resetToken": "abc123xyz..."
  }
}
```

**Response Error (404):**

```json
{
  "success": false,
  "message": "User not found"
}
```

**Notes:**

- Token valid for 1 hour
- Email sent automatically with reset link
- Token is hashed in database (BCrypt)

### 2. Reset Password

**Endpoint:** `POST /api/auth/reset-password`

**Request:**

```json
{
  "token": "abc123xyz...",
  "newPassword": "newSecurePassword123"
}
```

**Response Success (200):**

```json
{
  "success": true,
  "message": "Password has been reset"
}
```

**Response Error (400):**

```json
{
  "success": false,
  "message": "Invalid or expired token"
}
```

**Notes:**

- Token expires after 1 hour
- Token can only be used once (revoked after use)
- Password is hashed with BCrypt before saving

## 🧪 Testing Guide

### Manual Testing

#### Test Case 1: Forgot Password Flow (Happy Path)

1. Go to `/Auth/Index` (Login page)
2. Click "Forget password?" link
3. Enter valid username & email
4. Click "Send Reset Link"
5. Check email inbox (and spam folder)
6. Open email and click reset link
7. Enter new password (min 6 chars)
8. Confirm password (must match)
9. Click "Reset Password"
10. Verify redirect to login page
11. Login with new password

**Expected Result:** ✅ Success - Password changed

#### Test Case 2: Invalid Username/Email

1. Go to Forgot Password page
2. Enter invalid username or email
3. Click submit

**Expected Result:** ❌ Error - "User not found"

#### Test Case 3: Expired Token

1. Request forgot password
2. Wait more than 1 hour
3. Try to use reset link

**Expected Result:** ❌ Error - "Invalid or expired token"

#### Test Case 4: Password Mismatch

1. Go to Reset Password page (with valid token)
2. Enter password: "password123"
3. Enter confirm: "password456"
4. Click submit

**Expected Result:** ❌ Error - "Passwords do not match"

#### Test Case 5: Weak Password

1. Go to Reset Password page
2. Enter password less than 6 characters
3. Click submit

**Expected Result:** ❌ Error - "Password must be at least 6 characters"

### API Testing with Postman

#### 1. Test Forgot Password

```http
POST http://localhost:5170/api/auth/forgot-password
Content-Type: application/json

{
  "username": "admin",
  "email": "admin@company.com"
}
```

#### 2. Test Reset Password

```http
POST http://localhost:5170/api/auth/reset-password
Content-Type: application/json

{
  "token": "YOUR_TOKEN_FROM_EMAIL",
  "newPassword": "newPassword123"
}
```

## 🔐 Security Considerations

### 1. Token Security

- ✅ Tokens are hashed with BCrypt before storage
- ✅ Tokens expire after 1 hour
- ✅ Tokens are single-use (revoked after use)
- ✅ Token passed via URL parameter (HTTPS required in production)

### 2. Email Security

- ✅ Reset link contains token in URL
- ✅ Email contains user name for verification
- ✅ Email sent only to registered email address
- ⚠️ Use HTTPS in production to protect token in URL

### 3. Rate Limiting (TODO - Future Enhancement)

- ⚠️ Consider adding rate limiting to prevent abuse
- ⚠️ Limit number of forgot password requests per email/IP
- ⚠️ Example: Max 3 requests per 15 minutes

### 4. Production Recommendations

- Use HTTPS for all pages (especially reset password)
- Use environment variables for SMTP credentials
- Consider adding CAPTCHA to forgot password form
- Log all password reset attempts for auditing
- Send notification email after successful password reset

## 🐛 Troubleshooting

### Issue 1: Email Not Received

**Symptoms:** User submits forgot password but no email received

**Possible Causes:**

1. SMTP configuration incorrect
2. Email in spam folder
3. Invalid email address
4. SMTP server blocking emails

**Solutions:**

1. Check SMTP configuration in appsettings.json
2. Check spam/junk folder
3. Verify email address in database
4. Test SMTP using email testing page: `/Notification/Index`
5. Check application logs for errors

### Issue 2: Token Invalid/Expired

**Symptoms:** "Invalid or expired token" error when resetting password

**Possible Causes:**

1. Token expired (>1 hour old)
2. Token already used
3. Token not found in database
4. Token string corrupted (copy/paste error)

**Solutions:**

1. Request new password reset
2. Check token expiration time in database
3. Verify token string is complete (not truncated)
4. Check `UserRefreshTokens` table for token

### Issue 3: AppUrl Wrong in Production

**Symptoms:** Reset link points to localhost instead of production URL

**Possible Causes:**

- AppUrl not configured in production appsettings.json

**Solutions:**

1. Update appsettings.json in production:
   ```json
   {
     "AppUrl": "https://your-production-domain.com"
   }
   ```
2. Or use environment variable:
   ```bash
   export AppUrl="https://your-production-domain.com"
   ```

### Issue 4: Password Not Meeting Requirements

**Symptoms:** "Password must be at least 6 characters" error

**Solutions:**

- Ensure password is at least 6 characters
- Consider adding more password requirements (uppercase, numbers, special chars)

## 📊 Database Schema

### UserRefreshTokens Table

Used for storing reset tokens (same table used for JWT refresh tokens):

| Column         | Type      | Description               |
| -------------- | --------- | ------------------------- |
| RefreshTokenId | GUID      | Primary key               |
| UserId         | GUID      | Foreign key to Users      |
| TokenHash      | String    | BCrypt hashed token       |
| ExpiresAt      | DateTime  | Token expiration (1 hour) |
| CreatedAt      | DateTime  | Token creation time       |
| RevokedAt      | DateTime? | Token revocation time     |

**Query to check reset tokens:**

```sql
SELECT
    rt.RefreshTokenId,
    rt.UserId,
    u.Username,
    u.Email,
    rt.CreatedAt,
    rt.ExpiresAt,
    rt.RevokedAt,
    CASE
        WHEN rt.RevokedAt IS NOT NULL THEN 'Used/Revoked'
        WHEN rt.ExpiresAt < GETDATE() THEN 'Expired'
        ELSE 'Valid'
    END AS Status
FROM UserRefreshTokens rt
JOIN Users u ON rt.UserId = u.UserId
WHERE rt.ExpiresAt > DATEADD(HOUR, -2, GETDATE())
ORDER BY rt.CreatedAt DESC;
```

## 🚀 Usage Example

### For End Users:

**Scenario:** User lupa password

1. Buka halaman login: `http://localhost:5170/Auth/Index`
2. Klik link **"Forget password?"** di bawah form login
3. Isi form:
   - Username: `john.doe`
   - Email: `john.doe@company.com`
4. Klik **"Send Reset Link"**
5. Cek email inbox (atau spam folder)
6. Buka email dengan subject: **"Password Reset Request - ABB Monitoring System"**
7. Klik button **"Reset Password"** di email
8. Di halaman reset password, isi:
   - New Password: `newSecurePass123`
   - Confirm Password: `newSecurePass123`
9. Klik **"Reset Password"**
10. Login dengan username dan password baru

### For Developers:

**Integrate forgot password in your service:**

```csharp
// In your custom service
private readonly IAuth _authService;

public async Task HandleUserForgotPassword(string username, string email)
{
    try
    {
        // This will generate token AND send email automatically
        var token = await _authService.GenerateResetTokenAsync(username, email);

        if (string.IsNullOrEmpty(token))
        {
            // User not found
            _logger.LogWarning("Password reset requested for non-existent user: {Username}", username);
            return;
        }

        _logger.LogInformation("Password reset email sent for user: {Username}", username);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error processing forgot password request");
        throw;
    }
}
```

## 📋 Checklist

### Before Going to Production:

- [ ] Configure production `AppUrl` in appsettings.json
- [ ] Configure production SMTP credentials
- [ ] Test forgot password flow end-to-end
- [ ] Test email delivery to various email providers (Gmail, Outlook, Yahoo)
- [ ] Verify HTTPS is enabled (for secure token transmission)
- [ ] Set up email monitoring/logging
- [ ] Consider adding rate limiting
- [ ] Consider adding CAPTCHA
- [ ] Test with real email addresses
- [ ] Document password policy for users
- [ ] Set up alerts for failed password resets
- [ ] Backup user database before deployment

## 🔗 Related Documentation

- [EMAIL_USAGE_GUIDE.md](EMAIL_USAGE_GUIDE.md) - General email system usage
- [EMAIL_TEMPLATES_GUIDE.md](EMAIL_TEMPLATES_GUIDE.md) - Email template documentation
- [EMAIL_SMTP_TESTING_GUIDE.md](EMAIL_SMTP_TESTING_GUIDE.md) - SMTP testing guide

---

**Created:** 2026-01-26  
**Version:** 1.0  
**Status:** ✅ Production Ready (after SMTP configuration)  
**Author:** ABB Development Team
