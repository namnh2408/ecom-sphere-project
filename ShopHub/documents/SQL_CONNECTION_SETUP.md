# 🔗 SQL Server Connection Setup

## ✅ Current Configuration

### `appsettings.json` (Production)
```json
"DefaultConnection": "Server=localhost;Database=ShopHubDb;Trusted_Connection=true;TrustServerCertificate=true;"
```
**Auth Type**: Windows Authentication (Recommended for Production)

### `appsettings.Development.json` (Development)
```json
"DefaultConnection": "Server=localhost;Database=ShopHubDb;User Id=sa;Password=YourPassword123;TrustServerCertificate=true;"
```
**Auth Type**: SQL Authentication

---

## 🔧 Connection String Options

### Option 1️⃣: Windows Authentication (Recommended)
```
Server=localhost;Database=ShopHubDb;Trusted_Connection=true;TrustServerCertificate=true;
```
- ✅ Uses Windows credentials
- ✅ More secure
- ✅ No password in config
- ⚠️ Only works if SQL Server is on same domain/machine

---

### Option 2️⃣: SQL Authentication (Development)
```
Server=localhost;Database=ShopHubDb;User Id=sa;Password=YourPassword123;TrustServerCertificate=true;
```
- ✅ Works anywhere
- ⚠️ Password in config file
- ⚠️ Use strong password
- 📌 Change `YourPassword123` to actual password

---

### Option 3️⃣: SQL Express
```
Server=(local)\SQLEXPRESS;Database=ShopHubDb;Trusted_Connection=true;TrustServerCertificate=true;
```
- ✅ For SQL Server Express Edition
- ✅ Free version of SQL Server

---

### Option 4️⃣: Named Pipes (if TCP disabled)
```
Server=np:\\.\pipe\MSSQL$SQLEXPRESS\sql\query;Database=ShopHubDb;Trusted_Connection=true;
```
- ✅ Alternative connection protocol

---

## 🚀 Setup Steps

### 1️⃣ Install SQL Server
```
Download from: https://www.microsoft.com/en-us/sql-server/sql-server-downloads
- SQL Server 2022 Express (Free)
- SQL Server 2019 Express (Free)
- SQL Server Developer Edition (Free)
```

### 2️⃣ Configure Connection String
Edit **appsettings.Development.json** and replace:
- `YourPassword123` → actual SA password
- `localhost` → your server name (if different)
- `ShopHubDb` → database name (if different)

### 3️⃣ Create Database
```bash
# Option A: Run migrations
dotnet ef database update --project src/Services/ProductService

# Option B: Manual SQL
sqlcmd -S localhost -U sa -P YourPassword123
CREATE DATABASE ShopHubDb;
GO
```

### 4️⃣ Verify Connection
```bash
cd d:\PROJECT\GitHub\ecom-sphere-project\ShopHub
dotnet build
```

---

## 🧪 Test Connection String

### Using SQL Server Management Studio (SSMS)
1. Open SSMS
2. Server name: `localhost` or `(local)`
3. Authentication: 
   - Windows Authentication (for Windows auth)
   - SQL Server Authentication (for sa user)
4. Click **Connect**

### Using PowerShell
```powershell
$connectionString = "Server=localhost;Database=ShopHubDb;Trusted_Connection=true;TrustServerCertificate=true;"
$connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
$connection.Open()
Write-Host "✅ Connection successful!"
$connection.Close()
```

### Using Command Line
```bash
sqlcmd -S localhost -d ShopHubDb -Q "SELECT @@VERSION"
```

---

## 🔐 Security Best Practices

### ❌ DON'T
```
- Put passwords in appsettings.json (production)
- Use 'sa' user in production
- Use weak passwords
- Commit sensitive configs to git
```

### ✅ DO
```
- Use User Secrets in development
- Use Windows Authentication in production
- Use strong passwords (if SQL auth needed)
- Use Azure Key Vault / AWS Secrets Manager for production
```

---

## 🛠️ Environment-Specific Setup

### Development
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ShopHubDb;User Id=sa;Password=YourPassword123;TrustServerCertificate=true;"
  }
}
```

### Production
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=prod-db-server;Database=ShopHubDb;Trusted_Connection=true;TrustServerCertificate=true;"
  }
}
```

### Staging
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=staging-db-server;Database=ShopHubDb_Staging;Trusted_Connection=true;TrustServerCertificate=true;"
  }
}
```

---

## 🐛 Troubleshooting

### Issue: "Login failed for user"
```
Solution: Check username/password or switch to Windows Auth
```

### Issue: "Cannot open database"
```
Solution: Create database first or check database name
```

### Issue: "Connection timeout"
```
Solution: 
- Verify SQL Server is running
- Check firewall settings
- Verify server name is correct
```

### Issue: "Trusted_Connection=true not working"
```
Solution: 
- Use SQL Authentication instead
- Or set up domain trust
```

---

## 📝 Current Files Updated

✅ `appsettings.json` - Added `DefaultConnection`  
✅ `appsettings.Development.json` - Added `DefaultConnection`

---

## 🔗 Related Documentation

- See: `documents/06_IMPLEMENTATION_GUIDE.md` (Database configuration section)
- See: `documents/02_ARCHITECTURE.md` (Database layer section)

---

**Last Updated**: Oct 25, 2025  
**Status**: ✅ Ready to Use