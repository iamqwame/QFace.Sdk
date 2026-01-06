# Microsoft Graph Email Provider

This SDK now supports sending emails via Microsoft Graph API in addition to SMTP.

## Configuration

To use Microsoft Graph instead of SMTP, set the `Provider` to `"Graph"` in your `appsettings.json`:

```json
{
  "MessageSettings": {
    "Email": {
      "Provider": "Graph",
      "TenantId": "your-tenant-id",
      "ClientId": "your-client-id",
      "ClientSecret": "your-client-secret",
      "SendAsUser": "noreply@qfacesolutions.com",
      "FromEmail": "noreply@qfacesolutions.com",
      "FromName": "QIM ERP Notifications"
    }
  }
}
```

## Azure App Registration Setup

1. Go to [Azure Portal](https://portal.azure.com) → Azure Active Directory → App registrations
2. Create a new app registration or use an existing one
3. Note the **Application (client) ID** and **Directory (tenant) ID**
4. Go to **Certificates & secrets** → Create a new client secret
5. Copy the secret value (you won't be able to see it again)
6. Go to **API permissions** → Add permission → Microsoft Graph → Application permissions
7. Add `Mail.Send` permission
8. Click **Grant admin consent** for your organization

## Required Permissions

- `Mail.Send` (Application permission) - Required to send emails on behalf of users

## Benefits

- ✅ Bypasses Security Defaults restrictions
- ✅ No SMTP AUTH configuration needed
- ✅ More secure (OAuth 2.0 client credentials flow)
- ✅ Better integration with Microsoft 365
- ✅ Supports sending as any user in the tenant (with proper permissions)

## Fallback

If Graph provider is not selected or not properly configured, the SDK will fall back to SMTP provider automatically.
