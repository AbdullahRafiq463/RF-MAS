# 24/7 Cloud Backend Deployment Guide

This guide explains how to host your **RF-MAS .NET 9 Web API backend** on a free, permanent 24/7 cloud server so your mobile app works anywhere, at any time, on any phone or Wi-Fi network without touching your computer.

---

## Recommended Free Hosting Option: **Render.com** (Zero-Configuration & Free)

Render provides free cloud hosting that automatically builds and deploys your C# backend directly from GitHub.

### Step 1: Push Your Project to GitHub
1. Open terminal in `d:\RF-DMAS`.
2. Push your codebase to your GitHub account:
   ```bash
   git add .
   git commit -m "Add Dockerfile and cloud deployment config"
   git push origin main
   ```

---

### Step 2: Create a Free Web Service on Render
1. Go to [https://render.com](https://render.com) and sign in (using GitHub).
2. Click **New +** in the top dashboard and select **Web Service**.
3. Connect your **`RF-MAS`** GitHub repository.
4. Fill in the following basic settings:
   - **Name**: `rf-mas-api` (or any name you choose)
   - **Region**: Choose the closest region (e.g. Frankfurt, Singapore, Oregon)
   - **Language / Runtime**: **Docker** (Render will automatically detect the `Dockerfile` at the root)
   - **Instance Type**: **Free**
5. Click **Deploy Web Service**.

Render will automatically build the Docker image, start the .NET 9 background device simulation service, and give you a permanent **HTTPS URL**, such as:
```
https://rf-mas-api.onrender.com
```

---

### Step 3: Connect Your Mobile App to the Cloud URL
1. Open the **RF-MAS App** on your phone.
2. Go to the **Settings** tab (gear icon at the bottom right).
3. In the **Backend Base URL** box, paste your cloud URL:
   ```
   https://rf-mas-api.onrender.com
   ```
4. Tap **Save & Apply Backend URL**.
5. Tap **Test Backend Connection** (shows green success confirmation).

**Done!** Your mobile app will now display live RF instrumentation telemetry and run automated tests anywhere in the world on 4G/5G mobile data or any Wi-Fi network 24/7.

---

## Alternative 1-Click Option: **Railway.app**

1. Go to [https://railway.app](https://railway.app) and sign in with GitHub.
2. Click **New Project** -> **Deploy from GitHub repo** -> Select your `RF-MAS` repository.
3. Railway automatically detects the `Dockerfile`, publishes the container, and generates an instant public domain (e.g. `https://rf-mas-production.up.railway.app`).
4. Paste that domain into the mobile app's Settings tab.

---

## Testing Cloud Swagger API

Once deployed, you and your interviewers can also test all REST API endpoints and see live interactive documentation directly in any web browser at:
```
https://<your-cloud-url>/swagger
```
