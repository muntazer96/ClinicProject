import cors from 'cors';
import express from 'express';
import QRCode from 'qrcode';
import whatsapp from 'whatsapp-web.js';

const { Client, LocalAuth } = whatsapp;

const port = Number(process.env.PORT || 3001);
const token = process.env.BRIDGE_TOKEN || 'clinic-whatsapp-secret';
const app = express();
const executablePath = process.env.CHROME_PATH || undefined;

let ready = false;
let phone = null;
let qrDataUrl = null;
let lastMessage = 'Starting WhatsApp bridge';

app.use(cors());
app.use(express.json({ limit: '1mb' }));

function requireToken(req, res, next) {
  if (!token) return next();
  const header = req.headers.authorization || '';
  if (header === `Bearer ${token}`) return next();
  return res.status(401).json({ message: 'Unauthorized' });
}

function normalizePhone(value) {
  const digits = String(value || '').replace(/\D/g, '');
  if (!digits) return '';
  if (digits.startsWith('0')) return `964${digits.slice(1)}`;
  return digits;
}

const client = new Client({
  authStrategy: new LocalAuth({ clientId: 'clinic-admin' }),
  puppeteer: {
    headless: true,
    executablePath,
    args: ['--no-sandbox', '--disable-setuid-sandbox'],
  },
});

client.on('qr', async (qr) => {
  ready = false;
  phone = null;
  qrDataUrl = await QRCode.toDataURL(qr);
  lastMessage = 'Scan QR from the admin panel';
  console.log(lastMessage);
});

client.on('ready', () => {
  ready = true;
  qrDataUrl = null;
  phone = client.info?.wid?.user || null;
  lastMessage = 'WhatsApp bridge is ready';
  console.log(lastMessage);
});

client.on('authenticated', () => {
  lastMessage = 'WhatsApp authenticated';
  console.log(lastMessage);
});

client.on('auth_failure', (message) => {
  ready = false;
  lastMessage = `Authentication failed: ${message}`;
  console.error(lastMessage);
});

client.on('disconnected', (reason) => {
  ready = false
  phone = null
  qrDataUrl = null
  lastMessage = `Disconnected: ${reason}`
  console.warn(lastMessage)
});

app.get('/status', requireToken, (req, res) => {
  res.json({
    ready,
    authenticated: ready,
    phone,
    qrDataUrl,
    message: lastMessage,
  });
});

app.get('/qr', requireToken, (req, res) => {
  res.json({ qrDataUrl, ready, message: lastMessage });
});

app.post('/send-message', requireToken, async (req, res) => {
  if (!ready) {
    return res.status(409).json({ message: 'WhatsApp is not connected yet' });
  }

  const phoneNumber = normalizePhone(req.body.phone);
  const message = String(req.body.message || '').trim();
  if (!phoneNumber || !message) {
    return res.status(400).json({ message: 'phone and message are required' });
  }

  const chatId = `${phoneNumber}@c.us`;
  await client.sendMessage(chatId, message);
  res.json({ sent: true, phone: phoneNumber });
});

app.post('/logout', requireToken, async (req, res) => {
  try {
    await client.logout()

    ready = false
    phone = null
    qrDataUrl = null
    lastMessage = 'Logged out. Waiting for new QR...'

    setTimeout(() => {
      client.initialize()
    }, 1000)

    res.json({ success: true, message: lastMessage })
  } catch (error) {
    ready = false
    phone = null
    qrDataUrl = null
    lastMessage = error instanceof Error ? error.message : 'Logout failed'

    res.status(500).json({
      success: false,
      message: lastMessage,
    })
  }
});

app.listen(port, () => {
  console.log(`WhatsApp bridge listening on http://127.0.0.1:${port}`);
});



client.initialize();
