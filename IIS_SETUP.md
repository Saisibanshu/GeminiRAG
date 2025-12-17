# 🚀 Windows Server + IIS Setup Guide for Versnn

Complete step-by-step guide to deploy your .NET API and Angular frontend to Windows Server with IIS.

---

## 📋 Server Requirements

✅ **Operating System**: Windows Server 2019 or 2022  
✅ **RAM**: Minimum 4 GB  
✅ **CPU**: 2 cores or more  
✅ **Storage**: 50 GB available  
✅ **Network**: Public IP address  
✅ **Domain**: DNS record pointing to your server IP (versnn.com)

---

## 🎯 Overview - What We're Building

```
versnn.com (Port 80/443)          versnn.com/api (Port 5000)
       ↓                                    ↓
Angular Frontend (IIS)              .NET API (IIS)
       ↓                                    ↓
   wwwroot/versnn                    C:/inetpub/versnn-api
                                             ↓
                                    SQL Server LocalDB/Express
```

---

## 📦 Part 1: Install Required Software

### Step 1.1: Install IIS with Required Features

Open **PowerShell as Administrator** and run:

```powershell
# Install IIS with all required features
Install-WindowsFeature -name Web-Server -IncludeManagementTools
Install-WindowsFeature -name Web-Asp-Net45
Install-WindowsFeature -name Web-ISAPI-Ext
Install-WindowsFeature -name Web-ISAPI-Filter
Install-WindowsFeature -name Web-WebSockets

# Verify installation
Get-WindowsFeature -Name Web-Server
```

**✅ Checkpoint**: IIS Manager should now appear in your Start Menu

---

### Step 1.2: Install .NET 9 Hosting Bundle

1. **Download** the ASP.NET Core Runtime & Hosting Bundle:
   - Visit: https://dotnet.microsoft.com/download/dotnet/9.0
   - Download: **"Hosting Bundle"** (not just runtime!)

2. **Install** the bundle (follow the installer prompts)

3. **Restart IIS**:
   ```powershell
   iisreset
   ```

4. **Verify** installation:
   ```powershell
   dotnet --list-runtimes
   ```

**✅ Checkpoint**: Should see `Microsoft.AspNetCore.App 9.0.x`

---

### Step 1.3: Install URL Rewrite Module

1. Download from: https://www.iis.net/downloads/microsoft/url-rewrite
2. Install the module
3. Restart IIS:
   ```powershell
   iisreset
   ```

**✅ Checkpoint**: Open IIS Manager → Click on server name → Should see "URL Rewrite" icon

---

### Step 1.4: Install SQL Server

**Choose One:**

#### Option A: SQL Server Express (Free, Recommended for Start)
1. Download SQL Server 2022 Express
2. Choose "Basic" installation
3. Accept defaults and complete installation
4. **Note your server name**: Usually `localhost\SQLEXPRESS` or `(localdb)\MSSQLLocalDB`

#### Option B: Use Existing SQL Server LocalDB
If you already have SQL Server LocalDB from development:
```powershell
# Check if LocalDB is installed
sqllocaldb info

# Create instance if needed
sqllocaldb create "MSSQLLocalDB"
sqllocaldb start "MSSQLLocalDB"
```

**✅ Checkpoint**: Can connect using SQL Server Management Studio (SSMS)

---

### Step 1.5: Install OpenSSH Server (For GitHub Actions Deployment)

```powershell
# Install OpenSSH Server
Add-WindowsCapability -Online -Name OpenSSH.Server~~~~0.0.1.0

# Start the service
Start-Service sshd

# Set to start automatically
Set-Service -Name sshd -StartupType 'Automatic'

# Confirm running
Get-Service sshd
```

**✅ Checkpoint**: Service should show "Running"

---

## 🗄️ Part 2: Database Setup

### Step 2.1: Create Database and User

Open **SQL Server Management Studio** or **Azure Data Studio**, connect to your server, and run:

```sql
-- Create the database
CREATE DATABASE VersnnDb;
GO

-- Create SQL login for the API
CREATE LOGIN VersnnApiUser WITH PASSWORD = 'YourSecurePassword123!';
GO

-- Switch to the new database
USE VersnnDb;
GO

-- Create user from login
CREATE USER VersnnApiUser FOR LOGIN VersnnApiUser;

-- Grant permissions
ALTER ROLE db_datareader ADD MEMBER VersnnApiUser;
ALTER ROLE db_datawriter ADD MEMBER VersnnApiUser;
ALTER ROLE db_ddladmin ADD MEMBER VersnnApiUser;
GO

-- Verify
SELECT name FROM sys.database_principals WHERE name = 'VersnnApiUser';
```

**✅ Checkpoint**: Query should return 'VersnnApiUser'

**📝 Save These Values:**
- Server: `localhost\SQLEXPRESS` or `(localdb)\MSSQLLocalDB`
- Database: `VersnnDb`
- User: `VersnnApiUser`
- Password: `YourSecurePassword123!`

---

## 🌐 Part 3: IIS Configuration

### Step 3.1: Create Directories

```powershell
# Create backend directory
New-Item -Path "C:\inetpub\versnn-api" -ItemType Directory -Force

# Create frontend directory  
New-Item -Path "C:\inetpub\wwwroot\versnn" -ItemType Directory -Force

# Verify
Test-Path "C:\inetpub\versnn-api"
Test-Path "C:\inetpub\wwwroot\versnn"
```

---

### Step 3.2: Configure Backend API

```powershell
# Import IIS module
Import-Module WebAdministration

# Create dedicated App Pool
New-WebAppPool -Name "VersnnApiPool"

# Configure App Pool for .NET Core
Set-ItemProperty -Path "IIS:\AppPools\VersnnApiPool" -Name "managedRuntimeVersion" -Value ""
Set-ItemProperty -Path "IIS:\AppPools\VersnnApiPool" -Name "startMode" -Value "AlwaysRunning"
Set-ItemProperty -Path "IIS:\AppPools\VersnnApiPool" -Name "processModel.idleTimeout" -Value "00:00:00"

# Create IIS Website
New-Website -Name "VersnnAPI" `
    -Port 5000 `
    -PhysicalPath "C:\inetpub\versnn-api" `
    -ApplicationPool "VersnnApiPool" `
    -Force

# Grant permissions to App Pool
$acl = Get-Acl "C:\inetpub\versnn-api"
$rule = New-Object System.Security.AccessControl.FileSystemAccessRule("IIS AppPool\VersnnApiPool", "FullControl", "ContainerInherit,ObjectInherit", "None", "Allow")
$acl.SetAccessRule($rule)
Set-Acl "C:\inetpub\versnn-api" $acl

# Create logs directory
New-Item -Path "C:\inetpub\versnn-api\logs" -ItemType Directory -Force
```

**✅ Checkpoint**: 
- Open IIS Manager
- Should see "VersnnAPI" website
- Should see "VersnnApiPool" app pool

---

### Step 3.3: Configure Frontend

```powershell
# Create IIS Website for Frontend
New-Website -Name "VersnnWeb" `
    -Port 80 `
    -PhysicalPath "C:\inetpub\wwwroot\versnn" `
    -ApplicationPool "DefaultAppPool" `
    -Force

# Grant permissions
icacls "C:\inetpub\wwwroot\versnn" /grant "IIS_IUSRS:(OI)(CI)F" /T
```

**✅ Checkpoint**: IIS Manager should show "VersnnWeb" website on port 80

---

### Step 3.4: Configure Firewall Rules

```powershell
# Allow HTTP (Port 80)
New-NetFirewallRule -DisplayName "Versnn HTTP" `
    -Direction Inbound `
    -LocalPort 80 `
    -Protocol TCP `
    -Action Allow

# Allow HTTPS (Port 443)
New-NetFirewallRule -DisplayName "Versnn HTTPS" `
    -Direction Inbound `
    -LocalPort 443 `
    -Protocol TCP `
    -Action Allow

# Allow API (Port 5000)
New-NetFirewallRule -DisplayName "Versnn API" `
    -Direction Inbound `
    -LocalPort 5000 `
    -Protocol TCP `
    -Action Allow

# Allow SSH (Port 22) for deployments
New-NetFirewallRule -DisplayName "SSH for Deployment" `
    -Direction Inbound `
    -LocalPort 22 `
    -Protocol TCP `
    -Action Allow

# Verify rules
Get-NetFirewallRule -DisplayName "Versnn*" | Select-Object DisplayName, Enabled
```

---

## ⚙️ Part 4: Application Configuration

### Step 4.1: Create appsettings.Production.json

Create this file at: `C:\inetpub\versnn-api\appsettings.Production.json`

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=VersnnDb;User Id=VersnnApiUser;Password=YourSecurePassword123!;TrustServerCertificate=True;MultipleActiveResultSets=true"
  },
  "Jwt": {
    "SecretKey": "your-super-secret-jwt-key-minimum-32-characters-long-for-production-use",
    "Issuer": "versnn.com",
    "Audience": "versnn.com",
    "ExpiryMinutes": 60
  },
  "Cors": {
    "AllowedOrigins": [
      "http://versnn.com",
      "https://versnn.com",
      "http://localhost",
      "http://localhost:4200"
    ]
  }
}
```

**⚠️ IMPORTANT**: 
- Replace `YourSecurePassword123!` with actual password
- Generate a strong JWT secret key
- Never commit this file to Git!

---

### Step 4.2: Update Frontend Environment for Production

Update API URL in Angular app before building:

File: `src/GeminiRAG.Web/src/environments/environment.ts`

```typescript
export const environment = {
  production: true,
  apiUrl: 'http://versnn.com:5000/api'  // or https://versnn.com:5000/api with SSL
};
```

---

## 🔐 Part 5: GitHub Secrets Configuration

Go to your GitHub repository:  
**Settings** → **Secrets and variables** → **Actions** → **New repository secret**

Add these **4 secrets**:

| Secret Name | Value | Example |
|-------------|-------|---------|
| `SERVER_HOST` | Your server's public IP or domain | `123.45.67.89` |
| `SERVER_USER` | Windows administrator username | `Administrator` |
| `SERVER_PASSWORD` | Windows user password | `YourWindowsP@ssw0rd` |
| `SERVER_PORT` | SSH port (usually 22) | `22` |

---

## 🧪 Part 6: Test Your Setup

### Test 1: Verify IIS Sites

```powershell
# Check if sites are running
Get-Website | Select-Object Name, State, Bindings

# Should show:
# VersnnAPI - Started - http *:5000:
# VersnnWeb - Started - http *:80:
```

### Test 2: Test Database Connection

```powershell
# Test SQL connection
sqlcmd -S localhost\SQLEXPRESS -U VersnnApiUser -P YourSecurePassword123! -d VersnnDb -Q "SELECT @@VERSION"
```

### Test 3: Test SSH Access

From another machine (or same machine):
```powershell
ssh YourUsername@YourServerIP
```

Should connect without errors.

---

## 🚀 Part 7: Deploy & Go Live!

### Option A: Deploy via GitHub Actions (Recommended)

1. **Commit and push** your code to GitHub repo on `main` branch
2. GitHub Actions will **automatically**:
   - Build your application
   - Run tests
   - Deploy to your server
   - Restart IIS

3. **Monitor** the deployment:
   - Go to your GitHub repo
   - Click **Actions** tab
   - Watch the deployment progress

### Option B: Manual First Deployment (Testing)

If you want to test manually first:

**Backend:**
```powershell
# On your development machine
cd D:\Projects\GeminiRAG\src\GeminiRAG.Api
dotnet publish -c Release -o C:\Temp\publish

# Copy to server (using SCP or manual copy)
# Then on server:
Copy-Item -Path "C:\Temp\publish\*" -Destination "C:\inetpub\versnn-api" -Recurse -Force

# Restart app pool
Restart-WebAppPool -Name "VersnnApiPool"
```

**Frontend:**
```powershell
# On your development machine  
cd D:\Projects\GeminiRAG\src\GeminiRAG.Web
npm run build -- --configuration production

# Copy dist folder to server
# Then on server:
Copy-Item -Path "C:\Temp\dist\*" -Destination "C:\inetpub\wwwroot\versnn" -Recurse -Force
```

---

## ✅ Final Verification

### Check API:
```
http://versnn.com:5000/api/health
or
http://your-server-ip:5000/api/health
```

### Check Frontend:
```
http://versnn.com
or
http://your-server-ip
```

---

## 🔍 Troubleshooting

### API Returns 500 Error

```powershell
# Check logs
Get-Content "C:\inetpub\versnn-api\logs\stdout_*.log" -Tail 50

# Check Event Viewer
Get-EventLog -LogName Application -Source "IIS*" -Newest 20
```

### Frontend Shows 404

```powershell
# Verify files exist
Get-ChildItem "C:\inetpub\wwwroot\versnn"

# Check IIS logs
Get-Content "C:\inetpub\logs\LogFiles\W3SVC*\*.log" -Tail 20
```

### Database Connection Failed

```powershell
# Test connection from command line
sqlcmd -S localhost\SQLEXPRESS -U VersnnApiUser -P YourPassword -d VersnnDb -Q "SELECT 1"

# Check SQL Server service
Get-Service -Name "MSSQL*"
```

### SSH Connection Refused

```powershell
# Check if service is running
Get-Service sshd

# Check firewall
Get-NetFirewallRule -DisplayName "*SSH*"

# View SSH logs
Get-Content "C:\ProgramData\ssh\logs\sshd.log" -Tail 20
```

---

## 📊 Monitoring & Maintenance

### View Real-time Logs

```powershell
# API application logs
Get-Content "C:\inetpub\versnn-api\logs\stdout_*.log" -Wait -Tail 10

# IIS access logs
Get-Content "C:\inetpub\logs\LogFiles\W3SVC*\*.log" -Wait -Tail 10
```

### Monitor App Pool Performance

```powershell
# CPU usage
Get-Counter "\Process(w3wp*)\% Processor Time"

# Memory usage
Get-Counter "\Process(w3wp*)\Working Set - Private"
```

### Restart Services

```powershell
# Restart API app pool
Restart-WebAppPool -Name "VersnnApiPool"

# Restart frontend
Restart-WebAppPool -Name "DefaultAppPool"

# Restart IIS completely
iisreset
```

---

## 🎯 Summary Checklist

Before you provide info back to me, verify:

- [ ] IIS installed with all features
- [ ] .NET 9 Hosting Bundle installed
- [ ] URL Rewrite Module installed
- [ ] SQL Server installed and running
- [ ] Database `VersnnDb` created
- [ ] Database user `VersnnApiUser` created with permissions
- [ ] OpenSSH Server installed and running
- [ ] Firewall rules created (ports 80, 443, 5000, 22)
- [ ] IIS website "VersnnAPI" created on port 5000
- [ ] IIS website "VersnnWeb" created on port 80
- [ ] App Pool "VersnnApiPool" created and configured
- [ ] Directories created with proper permissions
- [ ] `appsettings.Production.json` created
- [ ] GitHub Secrets configured
- [ ] Can access server via SSH
- [ ] Tested database connection

---

## 📝 Info to Provide Me

Once setup is complete, send me:

1. **Server IP Address**: `_______________`
2. **Domain Name** (if configured): `versnn.com`
3. **SSH Username**: `_______________`
4. **SSH Port**: `22` (or different if changed)
5. **Database Details**:
   - Server: `_______________`
   - Database: `VersnnDb`
   - User: `VersnnApiUser`
6. **Confirmation**: "GitHub Secrets are configured" ✅

Then just **push to main** and watch it deploy! 🚀
