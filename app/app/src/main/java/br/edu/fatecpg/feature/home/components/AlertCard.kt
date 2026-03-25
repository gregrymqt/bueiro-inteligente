package br.edu.fatecpg.feature.home.components

import android.content.Context
import android.content.Intent
import android.graphics.Color
import android.net.Uri
import android.util.AttributeSet
import android.view.LayoutInflater
import android.widget.Button
import android.widget.FrameLayout
import android.widget.ImageButton
import android.widget.LinearLayout
import android.widget.ProgressBar
import android.widget.TextView
import br.edu.fatecpg.R
import br.edu.fatecpg.feature.monitoring.dto.DrainStatusDTO

class AlertCard @JvmOverloads constructor(
    context: Context,
    attrs: AttributeSet? = null,
    defStyleAttr: Int = 0
) : FrameLayout(context, attrs, defStyleAttr) {

    private var tvDrainId: TextView
    private var tvLastUpdate: TextView
    private var tvObstruction: TextView
    private var progressBar: ProgressBar
    private var btnClose: ImageButton
    private var btnMap: Button
    private var llBg: LinearLayout

    private var onDismissCallback: (() -> Unit)? = null

    init {
        LayoutInflater.from(context).inflate(R.layout.view_alert_card, this, true)

        tvDrainId = findViewById(R.id.tv_drain_id)
        tvLastUpdate = findViewById(R.id.tv_last_update)
        tvObstruction = findViewById(R.id.tv_obstruction)
        progressBar = findViewById(R.id.progress_obstruction)
        btnClose = findViewById(R.id.btn_close)
        btnMap = findViewById(R.id.btn_map)
        llBg = findViewById(R.id.ll_bg)

        btnClose.setOnClickListener {
            onDismissCallback?.invoke()
        }
    }

    fun setOnDismissListener(callback: () -> Unit) {
        this.onDismissCallback = callback
    }

    fun bind(alert: DrainStatusDTO) {
        tvDrainId.text = "Bueiro ID: ${alert.idBueiro}"
        val level = alert.nivelObstrucao.toInt()
        tvObstruction.text = "Obstrução: $level%"
        progressBar.progress = level

        tvLastUpdate.text = "Atualizado às: ${alert.ultimaAtualizacao}"

        val statusLower = alert.status.lowercase()
        if (statusLower == "crítico" || statusLower == "critico") {
            llBg.setBackgroundColor(Color.parseColor("#FFCDD2")) // Vermelho Suave
        } else if (statusLower == "alerta") {
            llBg.setBackgroundColor(Color.parseColor("#FFE082")) // Amarelo/Laranja Suave
        }

        btnMap.setOnClickListener {
            val lat = alert.latitude ?: 0.0
            val lng = alert.longitude ?: 0.0
            val uri = "geo:$lat,$lng?q=$lat,$lng(Bueiro+${alert.idBueiro})"
            val intent = Intent(Intent.ACTION_VIEW, Uri.parse(uri))
            intent.setPackage("com.google.android.apps.maps")
            context.startActivity(intent)
        }
    }
}
