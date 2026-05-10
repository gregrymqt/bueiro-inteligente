package br.edu.fatecpg.feature.profile.viewmodel

sealed class ProfileAction {
    object LoadProfile : ProfileAction()
    object OpenDashboardWeb : ProfileAction()
}