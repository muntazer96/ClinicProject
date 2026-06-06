# Clinic WhatsApp Bridge

This local bridge connects a personal WhatsApp account to the Clinic backend through HTTP.

## Run

```bash
cd WhatsApp_Bridge
npm install
npm start
```

If Puppeteer cannot download Chrome because of Windows permissions, run:

```bash
set npm_config_cache=..\.npm-cache
set PUPPETEER_CACHE_DIR=..\.puppeteer-cache
npm install
```

If Chrome is already installed, you can skip the download and use it directly:

```bash
set PUPPETEER_SKIP_DOWNLOAD=true
set CHROME_PATH=C:\Program Files\Google\Chrome\Application\chrome.exe
npm install
npm start
```

Open the admin panel as `SuperAdmin`, go to `WhatsApp OTP`, then scan the QR.

## Environment

```bash
PORT=3001
BRIDGE_TOKEN=clinic-whatsapp-secret
```

The backend setting should point to:

```json
{
  "WhatsAppMessages": {
    "Enabled": true,
    "Provider": "CustomHttp",
    "Endpoint": "http://127.0.0.1:3001/send-message",
    "Token": "clinic-whatsapp-secret",
    "PhoneFieldName": "phone",
    "MessageFieldName": "message"
  }
}
```

Personal WhatsApp automation is unofficial and can disconnect or be restricted by WhatsApp. Use it for light OTP/testing only.
