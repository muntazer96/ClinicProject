import { stat } from 'node:fs/promises';
import path from 'node:path';
import { fileURLToPath, pathToFileURL } from 'node:url';
import { createRequire } from 'node:module';

const directory = path.dirname(fileURLToPath(import.meta.url));
const require = createRequire(import.meta.url);
const { chromium } = require('playwright');
const htmlPath = path.join(directory, 'clinic-brochure.html');
const pngPath = path.join(directory, 'clinic-brochure.png');
const pdfPath = path.join(directory, 'clinic-brochure.pdf');
const browserPath = path.resolve(
  directory,
  '..',
  '..',
  '.puppeteer-cache',
  'chrome',
  'win64-146.0.7680.31',
  'chrome-win64',
  'chrome.exe',
);

const browser = await chromium.launch({
  executablePath: browserPath,
  headless: true,
});
const page = await browser.newPage({
  viewport: { width: 1782, height: 1260 },
  deviceScaleFactor: 1,
});

await page.goto(pathToFileURL(htmlPath).href, { waitUntil: 'networkidle' });
await page.locator('.sheet').screenshot({ path: pngPath });
await page.pdf({
  path: pdfPath,
  printBackground: true,
  width: '297mm',
  height: '210mm',
  margin: { top: '0', right: '0', bottom: '0', left: '0' },
});

await browser.close();

const [png, pdf] = await Promise.all([stat(pngPath), stat(pdfPath)]);
console.log(`PNG: ${pngPath} (${png.size} bytes)`);
console.log(`PDF: ${pdfPath} (${pdf.size} bytes)`);
