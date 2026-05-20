package br.edu.fatecpg.feature.profile.viewmodel

sealed class ProfileAction {
    data object LoadProfile : ProfileAction()
    data object OpenDashboardWeb : ProfileAction()
}
