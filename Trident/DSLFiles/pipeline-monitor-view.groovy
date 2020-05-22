buildMonitorView('DataTransfer-Jobs/RelativitySync/Nightly/Build monitor') {
    description('All nightly jobs')
    recurse(true)
    jobs {
        regex('/release-(.*)')
    }
}