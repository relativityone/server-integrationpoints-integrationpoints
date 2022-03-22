export function getFilter(filter) {
    if (filter) {
        return filter;
    } else {
        return "(objectClass=*)";
    }
}

export function getConnectionAuthenticationType(connectionAuthenticationType: number) {
    if (connectionAuthenticationType === 16) {
        return "Anonymous"
    } else if (connectionAuthenticationType === 32) {
        return "FastBind"
    } else if (connectionAuthenticationType === 2) {
        return "Secure Socket Layer"
    }
    return "";
}