folder('IntegrationPoints-Jobs') {
}

buildMonitorView('IntegrationPoints-Jobs/Nightly-Monitor') {
    description('All jobs for IntegrationPoints-Nightly')
	recurse(true)
	jobs {
		regex('(IntegrationPoints-Nightly.*)((release(.*))|(develop))')	
	}
	statusFilter(StatusFilter.ENABLED)
}