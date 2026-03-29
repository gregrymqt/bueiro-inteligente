package br.edu.fatecpg.feature.auth.viewmodel

import androidx.lifecycle.ViewModel
import androidx.lifecycle.ViewModelProvider
import br.edu.fatecpg.feature.auth.repository.AuthRepository

class LoginViewModelFactory(private val repository: AuthRepository) : ViewModelProvider.Factory {
    
    // 1. <T : ViewModel> -> Define que o método usa um tipo genérico "T" que herda de ViewModel.
    // 2. modelClass: Class<T> -> O que ele RECEBE (o molde/tipo da classe solicitada).
    // 3. : T -> O que ele RETORNA (uma instância real desse tipo).
    override fun <T : ViewModel> create(modelClass: Class<T>): T {
        
        // O Android pergunta: "Ei Factory, a classe que estão me pedindo (modelClass) 
        // é do tipo LoginViewModel?"
        if (modelClass.isAssignableFrom(LoginViewModel::class.java)) {
            
            // Se for, a Factory "fabrica" o ViewModel injetando o repositório 
            // e converte (cast) para o tipo genérico T que o Android espera receber.
            @Suppress("UNCHECKED_CAST")
            return LoginViewModel(repository) as T
        }
        
        // Se pedirem um ViewModel que essa Factory não sabe fabricar, ela lança erro.
        throw IllegalArgumentException("Unknown ViewModel class")
    }
}