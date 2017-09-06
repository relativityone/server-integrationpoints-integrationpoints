var IsSavedSearchTreeNode = function (node) {
    return !!node && (node.icon === "jstree-search" || node.icon === "jstree-search-personal");
}

var FlatSavedSearches = function(tree) {
    var _searches = [];
    var _iterate = function(node, depth) {
        if (IsSavedSearchTreeNode(node)) {
            _searches.push({
                value: node.id,
                displayName: node.text
            });
        }

        for (var i = 0, len = node.children.length; i < len; i++) {
            _iterate(node.children[i], depth + 1);
        }
    };

    var _sort = function(a, b) {
        var nameA = a.displayName.toUpperCase(); // ignore upper and lowercase
        var nameB = b.displayName.toUpperCase(); // ignore upper and lowercase
        if (nameA < nameB) {
            return -1;
        }
        if (nameA > nameB) {
            return 1;
        }

        // names must be equal
        return 0;
    };

    _iterate(tree, 0);
    return _searches.sort(_sort);
}
