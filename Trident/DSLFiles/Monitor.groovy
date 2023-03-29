folder('IntegrationPoints-Jobs') {
}

buildMonitorView('IntegrationPoints-Jobs/Nightly-Monitor') {
    description('All jobs for IntegrationPoints-Nightly')
	recurse(true)
	jobs {
		regex('*IntegrationPoints-Nightly*')
	}
	statusFilter(StatusFilter.ENABLED)
}