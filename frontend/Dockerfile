# Estágio 1: Build da aplicação
FROM node:20-alpine AS builder

# Define o diretório de trabalho dentro do contêiner
WORKDIR /app

# Copia os arquivos de configuração de dependências
COPY package.json package-lock.json* ./

# Instala as dependências do projeto
RUN npm install

# Copia o restante do código-fonte do frontend
COPY . .

# Compila (build) do projeto para a pasta dist/
RUN npm run build

# Estágio 2: Setup do servidor Nginx para servir a SPA
FROM nginx:alpine

# Remove o arquivo padrão de configuração do Nginx e injeta o nosso (para o React Router funcionar direito)
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Copia os arquivos estáticos built do estágio builder (na pasta dist que o Vite cria)
COPY --from=builder /app/dist /usr/share/nginx/html

# Expõe a porta que o Nginx está escutando
EXPOSE 80

# Inicia o servidor Web
CMD ["nginx", "-g", "daemon off;"]
