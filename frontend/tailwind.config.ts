import type { Config } from "tailwindcss";

const config: Config = {
  content: ["./index.html", "./src/**/*.{ts,tsx,js,jsx}"],
  theme: {
    extend: {
      colors: {
        brand: {
          50: "#f4f0ff",
          100: "#ede4ff",
          200: "#d8c9ff",
          300: "#c3adff",
          400: "#a981ff",
          500: "#8f5aff",
          600: "#7431f6",
          700: "#5f28c4",
          800: "#4b1f97",
          900: "#341768",
        },
      },
      fontFamily: {
        sans: ["'Inter Variable'", "'Inter'", "system-ui", "sans-serif"],
        mono: ["'JetBrains Mono'", "Menlo", "monospace"],
      },
      boxShadow: {
        card: "0 25px 35px -20px rgba(15, 23, 42, 0.35)",
      },
    },
  },
  plugins: [],
};

export default config;
