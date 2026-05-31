plugins {
    id("com.android.application")
    id("org.jetbrains.kotlin.android")
    id("org.jetbrains.kotlin.plugin.compose")
    id("org.jetbrains.kotlin.kapt")
    id("com.google.gms.google-services")
}

    android {
        namespace = "br.edu.fatecpg"
        compileSdk = 36

        defaultConfig {
            applicationId = "br.edu.fatecpg"
            minSdk = 24

            // Mantemos o target em 35 para preservar o comportamento de runtime atual
            targetSdk = 35

            versionCode = 1
            versionName = "1.0"

            testInstrumentationRunner = "androidx.test.runner.AndroidJUnitRunner"
            vectorDrawables {
                useSupportLibrary = true
            }
        }

        buildTypes {
            release {
                isMinifyEnabled = false
                proguardFiles(
                    getDefaultProguardFile("proguard-android-optimize.txt"),
                    "proguard-rules.pro"
                )
                buildConfigField("String", "APP_ID_SECRET", "\"\"")
                buildConfigField("String", "BASE_URL", "\"\"")
                buildConfigField("String", "DASHBOARD_WEB_URL", "\"\"")
            }
            debug {
                // 1. Verifique se este valor é exatamente igual ao do seu .env do backend!
                buildConfigField("String", "APP_ID_SECRET", "\"jZ3frultyGb5G84zX4en0naZDwIal2HuJi83fqgyPmAWjyXtoI5WzywycGvepQew7aEDtLCwZ0MBZy07tuvY8zcRif8iA5M5CcUTCrHgRMx8Hde8oATev72TGmNO0mnR\"")

                // 2. Trocamos o IP do emulador pela sua URL ativa do ngrok com o sufixo da API
                buildConfigField("String", "BASE_URL", "\"https://7bb5-2804-14d-8e90-5783-b8be-bd16-18b8-979e.ngrok-free.app/\"")

                buildConfigField("String", "DASHBOARD_WEB_URL", "\"http://10.0.2.2:5173/\"")
            }
        }

        buildFeatures {
            compose = true
            buildConfig = true
        }
        compileOptions {
            sourceCompatibility = JavaVersion.VERSION_21
            targetCompatibility = JavaVersion.VERSION_21
        }
    }
    kotlin {
        jvmToolchain(21)
    }

    dependencies {
        // BOM do Compose para alinhar versões automaticamente
        implementation(platform("androidx.compose:compose-bom:2024.02.00"))
        implementation("androidx.compose.ui:ui")
        implementation("androidx.compose.ui:ui-graphics")
        implementation("androidx.compose.ui:ui-tooling-preview")
        implementation("androidx.compose.material3:material3:1.3.0")
        implementation("androidx.compose.material:material-icons-extended")
        implementation("androidx.activity:activity-compose:1.8.2")
        implementation("androidx.navigation:navigation-compose:2.7.7")
        implementation("io.coil-kt:coil-compose:2.6.0")

        // Bibliotecas Core do Catálogo
        implementation(libs.androidx.core.ktx)
        implementation(libs.androidx.appcompat)
        implementation(libs.material)
        implementation(libs.androidx.activity)
        implementation(libs.androidx.constraintlayout)

        // Rede (Retrofit) e Reatividade (Lifecycle)
        implementation("com.squareup.retrofit2:retrofit:2.9.0")
        implementation("com.squareup.retrofit2:converter-gson:2.9.0")
        implementation("com.squareup.okhttp3:okhttp:4.12.0")
        implementation("com.squareup.okhttp3:logging-interceptor:4.12.0")
        implementation("org.jetbrains.kotlinx:kotlinx-coroutines-android:1.7.3")
        implementation("androidx.lifecycle:lifecycle-viewmodel-ktx:2.7.0")
        implementation("androidx.lifecycle:lifecycle-runtime-ktx:2.7.0")
        implementation("androidx.lifecycle:lifecycle-runtime-compose:2.8.0") // Versão estável
        implementation("androidx.room:room-runtime:2.8.4")
        kapt("androidx.room:room-compiler:2.8.4")

        implementation(platform(libs.firebase.bom))
        implementation(libs.firebase.messaging) 

        testImplementation(libs.junit)
        testImplementation(libs.mockk)
        testImplementation(libs.kotlinx.coroutines.test)
        androidTestImplementation(libs.androidx.junit)
        androidTestImplementation(libs.androidx.espresso.core)
    }