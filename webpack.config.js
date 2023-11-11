const path = require('path');

module.exports = {
    entry: './src/extension.ts',
    target: 'node',
    output: {
        path: path.resolve(__dirname, 'dist'),
        filename: 'extension.js',
        libraryTarget: 'commonjs2',
        devtoolModuleFilenameTemplate: '../[resource-path]'
    },
    devtool: 'source-map',
    externals: {
        vscode: 'commonjs vscode'
    },
    resolve: {
        extensions: ['.ts', '.js']
    },
    node: {
        __dirname: false
    },
    module: {
        rules: [
            {
               test: /\.ts$/,
               exclude: /node_modules/,
               use: [
                   {
                       loader: 'ts-loader'
                   }
               ]
            }
        ]
    }
};
