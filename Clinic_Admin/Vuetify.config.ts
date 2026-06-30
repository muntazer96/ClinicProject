import { createVuetify, type ThemeDefinition } from 'vuetify'
import * as components from 'vuetify/components'
import * as directives from 'vuetify/directives'
import { ar, en } from 'vuetify/locale'

const lightTheme: ThemeDefinition = {
  colors: {
    primary: '#13796b',
    'on-primary': '#ffffff',
    primaryDark: '#0b5c51',
    primaryLight: '#42a799',
    primarySoft: '#e4f4f0',
    secondary: '#42a799',
    'on-secondary': '#ffffff',
    secondaryLight: '#6fafa6',
    success: '#167163',
    'on-success': '#ffffff',
    successLight: '#e1f4ef',
    warning: '#b56b2f',
    'on-warning': '#ffffff',
    warningLight: '#fff1df',
    error: '#b33c3c',
    'on-error': '#ffffff',
    errorLight: '#ffeded',
    info: '#346fb1',
    'on-info': '#ffffff',
    infoLight: '#eaf3ff',
    background: '#f6f9f8',
    surface: '#ffffff',
    'on-surface': '#16312d',
    surfaceVariant: '#fbfdfc',
    text: '#16312d',
    textMuted: '#71827f',
    textSecondary: '#58706c',
    border: '#e2ebe9',
    borderLight: '#edf2f1',
    disabled: '#cccccc',
    shadow: 'rgba(29, 74, 68, 0.08)',
  },
}

export default createVuetify({
  components,
  directives,
  icons: {
    defaultSet: 'mdi',
  },
  theme: {
    defaultTheme: 'light',
    themes: {
      light: lightTheme,
    },
  },
  locale: {
    locale: 'ar',
    fallback: 'en',
    messages: { ar, en },
    rtl: { ar: true },
  },
})