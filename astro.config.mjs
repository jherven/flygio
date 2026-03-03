import { defineConfig } from 'astro/config';
import tailwindcss from '@tailwindcss/vite';
import sitemap from '@astrojs/sitemap';
import compress from '@playform/compress';

export default defineConfig({
  site: 'https://flygio.se',
  integrations: [sitemap(), compress()],
  vite: {
    plugins: [tailwindcss()],
  },
});
