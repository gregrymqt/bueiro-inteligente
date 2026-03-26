# Usa o OpenJDK 17 como base, que é a versão recomendada para rodar as versões recentes do Gradle no Android
FROM eclipse-temurin:17-jdk-jammy

# Variáveis de ambiente para o Android SDK
ENV ANDROID_SDK_ROOT="/opt/android-sdk"
ENV PATH="$ANDROID_SDK_ROOT/cmdline-tools/latest/bin:$ANDROID_SDK_ROOT/platform-tools:$PATH"
# Define as versões do SDK de compilação e Build Tools (ajuste conforme o seu build.gradle.kts)
ENV ANDROID_COMPILE_SDK="34"
ENV ANDROID_BUILD_TOOLS="34.0.0"
# Versão do pacote commandlinetools do Android
ENV COMMAND_LINE_TOOLS="11076708" 

# Diretório de trabalho no container
WORKDIR /app

# Instala dependências do sistema necessárias para o SDK
RUN apt-get update && apt-get install -y \
    wget \
    unzip \
    git \
    && rm -rf /var/lib/apt/lists/*

# Baixa e configura o Android Command Line Tools
RUN mkdir -p ${ANDROID_SDK_ROOT}/cmdline-tools && \
    wget -q https://dl.google.com/android/repository/commandlinetools-linux-${COMMAND_LINE_TOOLS}_latest.zip -O android_tools.zip && \
    unzip android_tools.zip -d ${ANDROID_SDK_ROOT}/cmdline-tools && \
    rm android_tools.zip && \
    # Renomeia a pasta para o formato esperado ('latest')
    mv ${ANDROID_SDK_ROOT}/cmdline-tools/cmdline-tools ${ANDROID_SDK_ROOT}/cmdline-tools/latest

# Aceita as licenças do Android SDK e instala as plataformas necessárias
RUN yes | sdkmanager --licenses && \
    sdkmanager "platform-tools" "platforms;android-${ANDROID_COMPILE_SDK}" "build-tools;${ANDROID_BUILD_TOOLS}"

# Copia os arquivos do Gradle primeiro para melhorar o cache de dependências do Docker
COPY gradlew .
COPY gradle/ gradle/
COPY build.gradle.kts .
COPY settings.gradle.kts .
COPY gradle.properties .
COPY local.properties . 
# Nota: Remova ou ignore o local.properties no .dockerignore se houver conflito de paths da sua máquina

# Dá permissão de execução ao wrapper do Gradle
RUN chmod +x ./gradlew

# Copia o restante do código fonte do app
COPY . .

# Expõe a porta se for usar alguma funcionalidade de debug server, mas em Android normalmente não é necessário para compilar
# EXPOSE 8081

# O comando padrão constrói o APK de Debug da aplicação Mobile
# O APK gerado ficará em /app/app/build/outputs/apk/debug/app-debug.apk
CMD ["./gradlew", "assembleDebug", "--no-daemon"]
