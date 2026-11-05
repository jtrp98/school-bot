import { defineConfig } from "vite";
import tailwindcss from "@tailwindcss/vite";

export default defineConfig({
  base: "./",
  plugins: [tailwindcss()],
  // The Block Lab toolbox imports the block catalog shared with the engine from contracts/.
  server: { port: 5173, strictPort: true, fs: { allow: [".", "../../contracts"] } },
  build: {
    target: "es2022",
    chunkSizeWarningLimit: 1800,
    minify: true,
    sourcemap: false,
  },
});
