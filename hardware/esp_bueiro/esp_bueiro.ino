#include <WiFi.h>        // Use <ESP8266WiFi.h> se for um ESP8266
#include <HTTPClient.h>
#include <ArduinoJson.h> // Você precisa instalar essa biblioteca na Arduino IDE

// 1. Configurações da Rede WiFi
const char* ssid = "NOME_DA_SUA_WIFI";
const char* password = "SENHA_DA_SUA_WIFI";

// 2. Configurações da sua API Python
// ATENÇÃO: O ESP32 não entende "localhost". Você precisa colocar o IP local da sua máquina (ex: 192.168.1.15) ou a URL do ngrok
const char* apiUrl = "http://SEU_IP_LOCAL:8000/monitoring/medicoes?token=SEU_TOKEN_AQUI"; 

// 3. Identificação deste Bueiro
const String ID_BUEIRO = "B-01-CENTRO";

void setup() {
  Serial.begin(115200);
  
  // Conectando ao WiFi
  WiFi.begin(ssid, password);
  Serial.print("Conectando ao WiFi");
  while(WiFi.status() != WL_CONNECTED) {
    delay(500);
    Serial.print(".");
  }
  Serial.println("\nConectado à rede WiFi!");
}

void loop() {
  if(WiFi.status() == WL_CONNECTED){
    HTTPClient http;
    
    // Inicia a conexão com a URL da sua API FastAPI
    http.begin(apiUrl);
    
    // Define que estamos enviando um JSON
    http.addHeader("Content-Type", "application/json");

    // --------------------------------------------------------
    // AQUI É A MÁGICA: Montando o JSON igual ao SensorPayloadDTO
    // --------------------------------------------------------
    StaticJsonDocument<200> jsonDoc;
    jsonDoc["id_bueiro"] = ID_BUEIRO;
    
    // Aqui você colocaria a leitura real do pino do sensor Ultrassônico
    // Exemplo simulando uma leitura de 45.5 cm
    float distanciaLida = 45.5; 
    jsonDoc["distancia_cm"] = distanciaLida;
    
    // Opcionais: se tiver módulo GPS no futuro
    // jsonDoc["latitude"] = -23.550520;
    // jsonDoc["longitude"] = -46.633308;

    // Converte o objeto JSON para uma string
    String payload;
    serializeJson(jsonDoc, payload);
    
    Serial.println("Enviando Payload: " + payload);

    // Faz a requisição POST
    int httpResponseCode = http.POST(payload);

    if (httpResponseCode > 0) {
      String response = http.getString();
      Serial.println("Código HTTP: " + String(httpResponseCode));
      Serial.println("Resposta da API: " + response);
    } else {
      Serial.print("Erro no envio HTTP: ");
      Serial.println(httpResponseCode);
    }
    
    http.end(); // Libera os recursos
  } else {
    Serial.println("WiFi Desconectado");
  }

  // Espera 10 segundos antes de enviar a próxima medição (ajuste conforme necessário)
  delay(10000); 
}