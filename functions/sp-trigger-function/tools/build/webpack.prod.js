const $ = require('./helpers');
const uglifyJSPlugin = require('uglifyjs-webpack-plugin');
const copyWebpackPlugin = require('copy-webpack-plugin');
const cleanWebpackPlugin = require('clean-webpack-plugin');
const webpack = require('webpack');

module.exports = {
  target: 'node',
  entry: {
    'webhookHandler': $.root('./src/functions/webhookHandler/webhookHandler.ts')
    /* 'anotherFunctionEntryPoint': $.root('./src/functions/anotherFunctionEntryPoint/anotherFunctionEntryPoint.ts'),*/
  },
  output: {
    path: $.root('dist'),
    filename: '[name]/[name].js',
    libraryTarget: 'commonjs2'
  },
  module: {
    rules: [
      {
        test: /\.ts$/,
        use: 'awesome-typescript-loader?declaration=false',
        exclude: [/\.(spec|e2e)\.ts$/]
      }
    ]
  },
  resolve: {
    extensions: ['.ts', '.js', '.json'],
    modules: [
      'node_modules',
      'src'
    ]
  },
  plugins: [
    new uglifyJSPlugin({
      uglifyOptions: {
        ecma: 6
      }
    }),
    new copyWebpackPlugin([
      {
        from: 'src/host.json',
        to: 'host.json'
      },
      {
        from: 'src/proxies.json',
        to: 'proxies.json'
      },
      {
        context: 'src/functions',
        from: '**/function.json',
        to: ''
      },
      {
        from: 'src/local.settings.json',
        to: 'local.settings.json'
      },
      {
        context: 'src/functions',
        from: '**/config/*.json',
        to: ''
      },
      {
        context: 'src/functions',
        from: '**/config/*.pem',
        to: ''
      }
    ]),
    new cleanWebpackPlugin(['dist/**/*'], {
      allowExternal: true,
      root: $.root('.'),
      verbose: false
    }),
    new webpack.IgnorePlugin(/^encoding$/, /node-fetch/)
  ],
  node: {
    __filename: false,
    __dirname: false,
  }
};
