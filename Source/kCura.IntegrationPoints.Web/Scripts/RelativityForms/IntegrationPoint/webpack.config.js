const path = require('path');
const { CleanWebpackPlugin } = require("clean-webpack-plugin");
const FormsExtensionPlugin = require("./plugins/FormsPlugin");

module.exports = {
  entry: './src/index.ts',
  module: {
    rules: [
      {
        test: /\.tsx?$/,
        use: 'ts-loader',
        exclude: /node_modules/,
      },
    ],
  },
  resolve: {
    extensions: ['.ts', '.js'],
  },
  output: {
    filename: 'integration-point-event-handler.js',
    path: path.resolve(__dirname + "/dist"),
    library: 'extension',
    libraryTarget: 'var',
    libraryExport: 'default'
  },
  optimization: {
    minimize: true,
  },
  plugins: [new CleanWebpackPlugin(), new FormsExtensionPlugin()],
};