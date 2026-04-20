import { defineConfig, loadEnv } from "vite";
import react from "@vitejs/plugin-react";
import path from "path";

// https://vite.dev/config/
export default defineConfig(({ mode }) => {
  // Carrega as variáveis de ambiente do arquivo .env na raiz do frontend
  const env = loadEnv(mode, process.cwd(), "");

  return {
    plugins: [react()],
    test: {
      environment: "jsdom",
      setupFiles: ["./src/setupTests.ts"],
    },

    // 1. Configuração de Path Aliases (Boas práticas)
    resolve: {
      alias: {
        "@": path.resolve(__dirname, "./src"),
      },
    },

    // 2. Configuração Global de SCSS
    css: {
      preprocessorOptions: {
        scss: {
          // Injeta esse arquivo no topo de todos os seus arquivos .scss automaticamente.
          // Dica: Use @use em vez de @import (que será descontinuado pelo Sass).
          additionalData: `@use "@/styles/default.scss" as *;\n`,
        },
      },
    },

    // 3. Otimização de Build (Chunks de JS e CSS)
    build: {
      // O Vite já faz code-splitting de CSS por padrão, mas podemos otimizar o JS:
      rollupOptions: {
        output: {
          manualChunks(id) {
            // Se o pacote vier da pasta node_modules, vamos separá-lo
            if (id.includes("node_modules")) {
              // Isolando o React e o React DOM num chunk próprio para aproveitar o cache do navegador
              if (id.includes("react") || id.includes("react-dom")) {
                return "vendor-react";
              }
              // Outras bibliotecas pesadas (ex: axios, date-fns) vão para um chunk genérico
              return "vendor";
            }
          },
        },
      },
      // Avisa se algum chunk ficar maior que 1000kb (o padrão é 500kb, mas com React às vezes passa um pouco)
      chunkSizeWarningLimit: 1000,
    },

    // 4. Configuração do Proxy de Desenvolvimento (para evitar erros de CORS)
    server: {
      proxy: {
        // Qualquer requisição que comece com /api será redirecionada
        "/api/v1/": {
          target: env.VITE_BACKEND_URL || "http://localhost:8080",
          changeOrigin: true,
        },
        "/realtime/ws": {
          target: env.VITE_BACKEND_URL || "http://localhost:8080",
          changeOrigin: true,
          ws: true, // <- MUITO IMPORTANTE: Diz ao Vite para habilitar o upgrade para WebSocket
        },
      },
    },
  };
});
