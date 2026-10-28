import { defineConfig } from "vite";
import tailwindcss from "@tailwindcss/vite";

export default defineConfig({
  base: "./",
  plugins: [tailwindcss()],
  server: { port: 5173, strictPort: true },
  build: {
    target: "es2022",
    chunkSizeWarningLimit: 1800,
    minify: true,
    sourcemap: false,
  },
});
