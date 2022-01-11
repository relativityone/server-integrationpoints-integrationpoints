const { Compilation, sources, NormalModule } = require("webpack");
const EXTENSION_NAME = "FormsExtensionPlugin"

/**
 * This plugin makes sure that output module will be in correct shape to be consumed by page interaction event handler.
 * Correct webpack setup is necessary.
 */
class FormsExtensionPlugin {
	_filename = "";
	_library = {};
    _entry = {};

	apply(compiler) {
		// Hooks on compilation time
		compiler.hooks.compilation.tap(EXTENSION_NAME, (compilation) => {
			this.validateOptions(compilation.options);
			this._filename = compilation.options.output.filename;
			this._library = compilation.options.output.library;
            this._entry = compilation.options.entry;

            compilation.hooks.afterProcessAssets.tap({
                name: EXTENSION_NAME
            }, (assets) => { this.processAsset(compilation, assets); })
		});
	}

	// Wraps output file with self-called function. This shape is expected to properly handle page interaction event handler.
	// The function accepts event names and convenienceApi as agruments.
	processAsset(compilation, assets) {
        Object.keys(assets).forEach((key) => {
            const asset = assets[key];

            let wrappedWithForm = `
                (function(eventNames, convenienceApi) {
                    ${asset.source()}
                    return ${this._library.name} (eventNames, convenienceApi); }
                (eventNames, convenienceApi))
            `;

            compilation.updateAsset(
                key,
                new sources.RawSource(wrappedWithForm)
            );
        });
	}

	// Validates webpack configuration
	validateOptions(options) {
        const { output } = options;

        if (!output.library) {
			throw new Error(`"output.library" option must be specified.`);
		}

		if (output.library.type !== "var") {
			throw new Error(`"output.libraryTarget" option must be "var".`);
		}

		if (!output.library.name) {
			throw new Error(`"output.library" option must be defined.`);
		}
	}
}

module.exports = FormsExtensionPlugin;