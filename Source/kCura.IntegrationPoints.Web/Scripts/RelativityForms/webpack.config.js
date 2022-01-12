const path = require('path');
const { CleanWebpackPlugin } = require("clean-webpack-plugin");
const FormsExtensionPlugin = require("./plugins/FormsPlugin");

module.exports = {
  entry: {
      "integration-point-event-handler": './src/integration-point-event-handler.ts',
      "integration-point-profile-event-handler": './src/integration-point-profile-event-handler.ts'
  },
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
    filename: '[name].js',
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