folder('DataTransfer-Jobs') {
}

folder('DataTransfer-Jobs/RelativitySync') {
}

buildMonitorView('DataTransfer-Jobs/RelativitySync/Nightly/Monitor') {
    description('All nightly jobs')
    recurse(true)
    jobs {
        regex('/release-(.*)')
    }
}