package br.edu.fatecpg.feature.home.ui

import android.os.Bundle
import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import android.view.animation.AnimationUtils
import androidx.fragment.app.Fragment
import androidx.lifecycle.ViewModel
import androidx.lifecycle.ViewModelProvider
import androidx.lifecycle.lifecycleScope
import br.edu.fatecpg.R
import br.edu.fatecpg.core.network.TokenManager
import br.edu.fatecpg.feature.home.components.AlertCard
import br.edu.fatecpg.feature.home.viewmodel.HomeViewModel
import br.edu.fatecpg.feature.realtime.repository.RealtimeRepository
import br.edu.fatecpg.feature.realtime.services.RealtimeService
import kotlinx.coroutines.launch

class HomeFragment : Fragment() {

    private lateinit var viewModel: HomeViewModel
    private lateinit var tokenManager: TokenManager

    override fun onCreateView(
        inflater: LayoutInflater, container: ViewGroup?,
        savedInstanceState: Bundle?
    ): View? {
        return inflater.inflate(R.layout.fragment_home, container, false)
    }

    override fun onViewCreated(view: View, savedInstanceState: Bundle?) {
        super.onViewCreated(view, savedInstanceState)

        tokenManager = TokenManager(requireContext())

        // Injeção de dependência manual do repositório RT na HomeViewModel
        val factory = object : ViewModelProvider.Factory {
            override fun <T : ViewModel> create(modelClass: Class<T>): T {
                val service = RealtimeService()
                val repository = RealtimeRepository(service)
                return HomeViewModel(repository) as T
            }
        }
        viewModel = ViewModelProvider(this, factory)[HomeViewModel::class.java]

        val alertCardView = view.findViewById<AlertCard>(R.id.alert_card_view)
        val pulseAnim = AnimationUtils.loadAnimation(requireContext(), R.anim.pulse)

        alertCardView.setOnDismissListener {
            viewModel.dismissAlert()
        }

        viewLifecycleOwner.lifecycleScope.launch {
            viewModel.activeAlert.collect { alert ->
                if (alert != null) {
                    alertCardView.visibility = View.VISIBLE
                    alertCardView.bind(alert)
                    alertCardView.startAnimation(pulseAnim)
                } else {
                    alertCardView.visibility = View.GONE
                    alertCardView.clearAnimation()
                }
            }
        }
    }

    // Regra de Economia de Bateria: Conecta só quando tiver no foreground
    override fun onStart() {
        super.onStart()
        viewModel.startRealtimeUpdates(tokenManager.getToken())
    }

    // Desconecta assim que sai da visão do usuário
    override fun onStop() {
        super.onStop()
        viewModel.stopRealtimeUpdates()
    }
}