#include <WiFi.h>
#include <PubSubClient.h> // Substitui a antiga biblioteca HTTPClient
#include <ArduinoJson.h>
#include "secrets.h"

// Configurações de rede
const char *ssid = WIFI_SSID;
const char *senha = WIFI_PASS;
const char *mqtt_broker = MQTT_BROKER_IP; 

// Topico MQTT que o Node-RED está escutando
const char *topico_telemetria = "bueiro/telemetria";

const String ID_BUEIRO = "B-02-CENTRO";
int const trig = 4;
int const echo = 2;

const float alturaTotal = 100.0;
const float DELTA_MINIMO = 2.0;
const unsigned long HEARTBEAT_MS = 5UL * 60UL * 1000UL;
const unsigned long INTERVALO_LEITURA_MS = 500UL;

float ultimaDistancia = 0.0;
unsigned long ultimoEnvioMillis = 0;
bool jaEnviouLeitura = false;

// Instâncias Globais de Rede e MQTT
WiFiClient espClient;
PubSubClient mqttClient(espClient);

// medição da distância (Lógica mantida)
float medirDistancia() {
  digitalWrite(trig, LOW);
  delayMicroseconds(2);
  digitalWrite(trig, HIGH);
  delayMicroseconds(10);
  digitalWrite(trig, LOW);
  long duracao = pulseIn(echo, HIGH);
  float distancia = (duracao * 0.0343) / 2;
  return distancia;
}

// filtra as medições para eliminar ruídos (Lógica mantida)
float distanciaFiltrada() {
  const int quantidadeAmostras = 5;
  float leitura[quantidadeAmostras];

  for (int i = 0; i < quantidadeAmostras; i++) {
    leitura[i] = medirDistancia();
    delay(30);
  }

  // Bubble sort
  for (int i = 0; i < quantidadeAmostras - 1; i++) {
    for (int j = 0; j < quantidadeAmostras - i - 1; j++) {
      if (leitura[j] > leitura[j + 1]) {
        float temp = leitura[j];
        leitura[j] = leitura[j + 1];
        leitura[j + 1] = temp;
      }
    }
  }

  float soma = 0;
  for (int i = 1; i < quantidadeAmostras - 1; i++) {
    soma += leitura[i];
  }
  return soma / 3;
}

// Reconexão ao Broker MQTT
void reconectarMQTT() {
  while (!mqttClient.connected()) {
    Serial.print("Tentando conexão MQTT com o broker...");
    // Tenta conectar usando o ID do Bueiro como identificador de cliente
    if (mqttClient.connect(ID_BUEIRO.c_str())) {
      Serial.println(" Conectado!");
    } else {
      Serial.print(" Falhou, rc=");
      Serial.print(mqttClient.state());
      Serial.println(". Tentando novamente em 5 segundos.");
      delay(5000);
    }
  }
}

// Função de publicação via PubSubClient
bool publicarMQTT(float distancia) {
  if (!mqttClient.connected()) {
    return false;
  }

  // Capacidade de 256 bytes por segurança
  StaticJsonDocument<256> jsonDoc;

  jsonDoc["id_bueiro"] = ID_BUEIRO;
  jsonDoc["distancia_cm"] = distancia;
  jsonDoc["latitude"] = 0.0;
  jsonDoc["longitude"] = 0.0;

  String payload;
  serializeJson(jsonDoc, payload);

  Serial.println("Publicando MQTT:");
  Serial.println(payload);

  // Publica a string serializada no tópico definido
  bool sucesso = mqttClient.publish(topico_telemetria, payload.c_str());
  
  if(sucesso) {
    Serial.println("Envio realizado com sucesso!");
  } else {
    Serial.println("Falha no envio MQTT.");
  }
  
  return sucesso;
}

void setup() {
  Serial.begin(115200);
  pinMode(trig, OUTPUT);
  pinMode(echo, INPUT);

  WiFi.begin(ssid, senha);
  Serial.print("Conectando WiFi");
  while (WiFi.status() != WL_CONNECTED) {
    delay(500);
    Serial.print(".");
  }
  Serial.println("\nWiFi conectado!");

  // Configura a porta e o IP do Mosquitto
  mqttClient.setServer(mqtt_broker, 1883);
}

void loop() {
  // Garante que o cliente MQTT esteja conectado antes de operar
  if (WiFi.status() == WL_CONNECTED && !mqttClient.connected()) {
    reconectarMQTT();
  }
  
  // Mantém os processos de background do MQTT rodando (heartbeats de rede)
  mqttClient.loop();

  float distancia = distanciaFiltrada();
  float nivel = ((alturaTotal - distancia) / alturaTotal) * 100;

  // Lógica de Logs de Nível
  if (nivel >= 60) {
    Serial.println("Nível Crítico!");
  } else if (nivel >= 40) {
    Serial.println("Alerta! Acima de 40%!");
  } else if (nivel >= 15) {
    Serial.println("Nível normal! Entre 15% e 40%!");
  } else {
    Serial.println("Nível baixo, possível falha no sensor ou bueiro vazio");
  }

  float delta = jaEnviouLeitura
                    ? (distancia >= ultimaDistancia ? distancia - ultimaDistancia : ultimaDistancia - distancia)
                    : DELTA_MINIMO + 1.0;

  unsigned long agora = millis();
  bool heartbeatVencido = jaEnviouLeitura && (agora - ultimoEnvioMillis >= HEARTBEAT_MS);
  bool deveEnviar = !jaEnviouLeitura || delta > DELTA_MINIMO || heartbeatVencido;

  if (deveEnviar) {
    if (publicarMQTT(distancia)) {
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
  Serial.println(" cm\n");

  delay(INTERVALO_LEITURA_MS);
}