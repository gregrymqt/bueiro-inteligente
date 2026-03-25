package br.edu.fatecpg.feature.monitoring.ui

import android.os.Bundle
import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import android.widget.ProgressBar
import android.widget.TextView
import androidx.fragment.app.Fragment
import androidx.lifecycle.ViewModel
import androidx.lifecycle.ViewModelProvider
import androidx.lifecycle.lifecycleScope
import androidx.recyclerview.widget.LinearLayoutManager
import androidx.recyclerview.widget.RecyclerView
import br.edu.fatecpg.R
import br.edu.fatecpg.feature.monitoring.repository.MonitoringRepository
import br.edu.fatecpg.feature.monitoring.services.MonitoringService
import br.edu.fatecpg.feature.monitoring.viewmodel.MonitoringUiState
import br.edu.fatecpg.feature.monitoring.viewmodel.MonitoringViewModel
import kotlinx.coroutines.launch

class MonitoringFragment : Fragment() {

    private lateinit var viewModel: MonitoringViewModel
    private val adapter = MonitoringAdapter()

    override fun onCreateView(
        inflater: LayoutInflater, container: ViewGroup?,
        savedInstanceState: Bundle?
    ): View? {
        return inflater.inflate(R.layout.fragment_monitoring, container, false)
    }

    override fun onViewCreated(view: View, savedInstanceState: Bundle?) {
        super.onViewCreated(view, savedInstanceState)

        // Configura Injeção Manual Simple
        val factory = object : ViewModelProvider.Factory {
            override fun <T : ViewModel> create(modelClass: Class<T>): T {
                return MonitoringViewModel(MonitoringRepository(MonitoringService.create())) as T
            }
        }
        viewModel = ViewModelProvider(this, factory)[MonitoringViewModel::class.java]

        // UI
        val rv = view.findViewById<RecyclerView>(R.id.rv_bueiros)
        val progress = view.findViewById<ProgressBar>(R.id.progress_loading)
        val tvError = view.findViewById<TextView>(R.id.tv_error)

        rv.layoutManager = LinearLayoutManager(requireContext())
        rv.adapter = adapter

        // Observer do StateFlow
        viewLifecycleOwner.lifecycleScope.launch {
            viewModel.uiState.collect { state ->
                when (state) {
                    is MonitoringUiState.Loading -> {
                        progress.visibility = View.VISIBLE
                        tvError.visibility = View.GONE
                        rv.visibility = View.GONE
                    }
                    is MonitoringUiState.Success -> {
                        progress.visibility = View.GONE
                        tvError.visibility = View.GONE
                        rv.visibility = View.VISIBLE
                        // Como a API retorna 1 obj, colocamos em uma lista pro RecyclerView provisoriamente
                        adapter.submitList(listOf(state.data)) 
                    }
                    is MonitoringUiState.Error -> {
                        progress.visibility = View.GONE
                        rv.visibility = View.GONE
                        tvError.visibility = View.VISIBLE
                        tvError.text = state.message
                    }
                }
            }
        }

        // Chama a API ao abrir a tela (ID de exemplo)
        viewModel.fetchDrainStatus("1")
    }
}