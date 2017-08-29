var FlatSavedSearches = function(tree) {
    var _searches = [];
    var _iterate = function(node, depth) {
        if (self.IsSavedSearchTreeNode(node)) {
            _searches.push({
                value: node.id,
                displayName: node.text
            });
        }

        for (var i = 0, len = node.children.length; i < len; i++) {
            _iterate(node.children[i], depth + 1);
        }
    };

    _iterate(tree, 0);
    return _searches;
};
