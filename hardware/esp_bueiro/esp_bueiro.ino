#include <WiFi.h>
#include <HTTPClient.h>
#include <ArduinoJson.h>
#include "secrets.h"

// DICA PARA ERRO HTTP -1 (Connection Refused):
// Verifique o IP configurado em API_URL no arquivo secrets.h.
// O erro -1 geralmente ocorre quando o ESP32 não alcança o servidor.
// Evite usar IPs de adaptadores virtuais (como WSL/Hyper-V, ex: 172.x.x.x).
// Rode 'ipconfig' no terminal do Windows, procure pelo endereço IPv4 do seu
// adaptador de rede principal (Wi-Fi) (ex: 192.168.x.x) e coloque-o na sua API_URL.
// Garanta que o PC e o ESP32 estejam conectados rigorosamente na mesma rede.

const char* ssid = WIFI_SSID;
const char* senha = WIFI_PASS;
const char* hardwareToken = HARDWARE_TOKEN;
const char* urlApi = API_URL;

const String ID_BUEIRO = "B-01-CENTRO";

int const trig = 4;
int const echo = 2;

const float alturaTotal = 100.0;
const float DELTA_MINIMO = 2.0;
const unsigned long HEARTBEAT_MS = 5UL * 60UL * 1000UL;
const unsigned long INTERVALO_LEITURA_MS = 500UL;

float ultimaDistancia = 0.0;
unsigned long ultimoEnvioMillis = 0;
bool jaEnviouLeitura = false;

// medição da distância
float medirDistancia() {
  //controla o disparo do sensor
  digitalWrite(trig, LOW);
  delayMicroseconds(2);
  digitalWrite(trig, HIGH);
  delayMicroseconds(10);
  digitalWrite(trig, LOW);
  // mede quanto tempo o echo ficou ativo, esse é o tempo de ida e volta do som
  long duracao = pulseIn(echo, HIGH);
  //multiplica a duração pelo pela velocidade do som em cm por microsegundo assim tendo a distância
  float distancia = (duracao * 0.0343) / 2;

  return distancia;
}

// filtra as medições afim de eliminar falsos positivos vindos de ruidos
float distanciaFiltrada() {
  const int quantidadeAmostras = 5;
  float leitura[quantidadeAmostras];

  // preenche o vetor com os valores do sensor
  for (int i = 0; i < quantidadeAmostras; i++) {
    leitura[i] = medirDistancia();
    delay(30);
  }

  // bubble sort ordenando os dados de forma crescente
  for (int i = 0; i < quantidadeAmostras - 1; i++) {
    for (int j = 0; j < quantidadeAmostras - i - 1; j++) {
      if (leitura[j] > leitura[j + 1]) {
        float temp = leitura[j];
        leitura[j] = leitura[j + 1];
        leitura[j + 1] = temp;
      }
    }
  }

  // media dos valores ignorando o menor e maior valor
  float soma = 0;
  for (int i = 1; i < quantidadeAmostras - 1; i++) {
    soma += leitura[i];
  }

  return soma / 3;
}

bool bueiroJson(float distancia, float nivel) {
  (void)nivel;

  if (WiFi.status() == WL_CONNECTED) {
    HTTPClient http;
    String urlFinal = String(urlApi) + "?token=" + String(hardwareToken);
    http.begin(urlFinal);
    http.setTimeout(5000); // 5s timeout evita congelamento se a rede oscilar
    http.addHeader("Content-Type", "application/json");

    // Capacidade de 200 bytes é suficiente para o payload de ~82 bytes,
    // mas elevamos a 256 por precaução extra caso adicionem mais campos no futuro.
    StaticJsonDocument<256> jsonDoc;

    jsonDoc["id_bueiro"] = ID_BUEIRO;
    jsonDoc["distancia_cm"] = distancia;
    jsonDoc["latitude"] = 0.0;
    jsonDoc["longitude"] = 0.0;

    String payload;
    serializeJson(jsonDoc, payload);

    Serial.println("Enviando JSON:");
    Serial.println(payload);

    int httpResponseCode = http.POST(payload);
    bool sucesso = false;

    // A API enfileira o processamento no Hangfire, retornando 202 Accepted
    if (httpResponseCode == 202) {
      Serial.print("Código HTTP: ");
      Serial.println(httpResponseCode);

      String response = http.getString();
      Serial.println("Resposta: " + response);
      sucesso = true;
    } else {
      Serial.print("Erro HTTP: ");
      Serial.println(httpResponseCode);
      if (httpResponseCode > 0) {
        String response = http.getString();
        Serial.println("Resposta: " + response);
      }
    }

    http.end();
    return sucesso;
  }

  Serial.println("WiFi desconectado; envio bloqueado.");
  return false;
}

void setup() {
  Serial.begin(115200);
  pinMode(trig, OUTPUT);
  pinMode(echo, INPUT);

  WiFi.begin(ssid, senha);
  Serial.print("Conectando");

  while (WiFi.status() != WL_CONNECTED) {
    delay(500);
    Serial.print(".");
  }

  Serial.println("\nWiFi conectado!");
}

void loop() {
  float distancia = distanciaFiltrada();
  float nivel = ((alturaTotal - distancia) / alturaTotal) * 100;

  if (nivel >= 60) {
    Serial.println("Nivel Critico!");
  } else if (nivel >= 40) {
    Serial.println("Alerta! acima de 40%!");
  } else if (nivel >= 15) {
    Serial.println("Nivel normal! abaixo de 40% e acima de 15%!");
  } else {
    Serial.println("Nivel baixo, possível falha no sensor ou bueiro vazio");
  }

  float delta = jaEnviouLeitura
                  ? (distancia >= ultimaDistancia ? distancia - ultimaDistancia : ultimaDistancia - distancia)
                  : DELTA_MINIMO + 1.0;
  unsigned long agora = millis();
  bool heartbeatVencido = jaEnviouLeitura && (agora - ultimoEnvioMillis >= HEARTBEAT_MS);
  bool deveEnviar = !jaEnviouLeitura || delta > DELTA_MINIMO || heartbeatVencido;

  if (deveEnviar) {
    if (bueiroJson(distancia, nivel)) {
      ultimaDistancia = distancia;
      ultimoEnvioMillis = agora;
      jaEnviouLeitura = true;
    }
  } else {
    Serial.println("Envio bloqueado por delta mínimo.");
  }

  Serial.print("Distancia: ");
  Serial.print(distancia);
  Serial.print(" cm | Nivel: ");
  Serial.print(nivel);
  Serial.print("% | Ultima enviada: ");
  Serial.print(ultimaDistancia);
  Serial.println(" cm");

  delay(INTERVALO_LEITURA_MS);
}
