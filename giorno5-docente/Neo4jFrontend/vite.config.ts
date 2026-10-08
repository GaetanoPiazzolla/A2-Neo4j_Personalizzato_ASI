import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

export default defineConfig({
  plugins: [react()],
  server: {
    open: true,
  },
  // Nota sul routing: usiamo la History API, quindi /movies deve servire index.html.
  // Vite lo fa gia' di suo (appType: 'spa') sia in dev sia in `vite preview`,
  // quindi qui non serve configurare nulla. In produzione invece la regola di
  // fallback va scritta sul server statico (nginx, Apache, S3...).
});
