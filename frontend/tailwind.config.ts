import type { Config } from "tailwindcss";

// Palette extracted directly from the School Bot logo:
// mascot orange #F8945C / mascot sky-blue #68C8F2 / white outline
// navy shifted to deep blue #0B2045 (cleaner, no green cast)
const config: Config = {
  content: [
    "./app/**/*.{ts,tsx}",
    "./components/**/*.{ts,tsx}",
    "./lib/**/*.{ts,tsx}",
  ],
  theme: {
    extend: {
      colors: {
        navy: {
          DEFAULT: "#0B2045",
          dark: "#060D23",
          light: "#16346C",
        },
        orange: {
          DEFAULT: "#F8945C",
          dark: "#E2733A",
          light: "#FDC9A0",
        },
        sky: {
          DEFAULT: "#68C8F2",
          dark: "#3FA9D9",
          light: "#C9E9FA",
        },
        alert: {
          DEFAULT: "#E2473D",
          dark: "#C13226",
        },
        paper: {
          DEFAULT: "#FFFFFF",
          dim: "#F1F8FC",
        },
        ink: "#1F2A33",
      },
      fontFamily: {
        display: ["Mitr", "Leelawadee UI", "Tahoma", "sans-serif"],
        body: ["Sarabun", "Leelawadee UI", "Tahoma", "sans-serif"],
        mono: ["var(--font-mono)"],
      },
      borderRadius: {
        chip: "999px",
      },
    },
  },
  plugins: [require("@tailwindcss/typography")],
};
export default config;
