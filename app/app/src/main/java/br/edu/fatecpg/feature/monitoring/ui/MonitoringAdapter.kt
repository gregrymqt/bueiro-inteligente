package br.edu.fatecpg.feature.monitoring.ui

import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import android.widget.ProgressBar
import android.widget.TextView
import androidx.recyclerview.widget.RecyclerView
import br.edu.fatecpg.R
import br.edu.fatecpg.feature.monitoring.dto.DrainStatusDTO

class MonitoringAdapter : RecyclerView.Adapter<MonitoringAdapter.ViewHolder>() {

    private val items = mutableListOf<DrainStatusDTO>()

    fun submitList(newItems: List<DrainStatusDTO>) {
        items.clear()
        items.addAll(newItems)
        notifyDataSetChanged()
    }

    override fun onCreateViewHolder(parent: ViewGroup, viewType: Int): ViewHolder {
        val view = LayoutInflater.from(parent.context).inflate(R.layout.item_bueiro, parent, false)
        return ViewHolder(view)
    }

    override fun onBindViewHolder(holder: ViewHolder, position: Int) {
        holder.bind(items[position])
    }

    override fun getItemCount(): Int = items.size

    class ViewHolder(itemView: View) : RecyclerView.ViewHolder(itemView) {
        private val tvId: TextView = itemView.findViewById(R.id.tv_bueiro_id)
        private val tvStatus: TextView = itemView.findViewById(R.id.tv_status)
        private val pbObstrucao: ProgressBar = itemView.findViewById(R.id.pb_obstrucao)

        fun bind(bueiro: DrainStatusDTO) {
            tvId.text = "ID: ${bueiro.idBueiro}"
            tvStatus.text = "Status: ${bueiro.status}"
            pbObstrucao.progress = bueiro.nivelObstrucao.toInt()

            // Lógica de Cores baseada na regra ("Verde Ok", "Amarelo Alerta", "Vermelho Crítico")
            val colorRes = when (bueiro.status.lowercase()) {
                "ok" -> android.R.color.holo_green_dark
                "alerta" -> android.R.color.holo_orange_dark
                "crítico", "critico" -> android.R.color.holo_red_dark
                else -> android.R.color.darker_gray
            }
            tvStatus.setTextColor(itemView.context.getColor(colorRes))
        }
    }
}