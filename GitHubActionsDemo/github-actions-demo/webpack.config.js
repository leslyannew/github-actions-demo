const MiniCssExtractPlugin = require('mini-css-extract-plugin');

module.exports = {
    entry: {
        'site': './Scripts/main.js',
        'sidenav': './Scripts/sidenav.js'
    },
    module: {
        rules: [
            {
                test: /\.css$/,
                use: [MiniCssExtractPlugin.loader, 'css-loader'],
            },
            {
                test: /\.woff(2)?$/,
                generator: {
                    filename: 'fonts/[name].[ext]'
                }
            },
            {
                test: /\.(ttf|eot|svg)?$/,
                generator: {
                    filename: 'fonts/[name].[ext]'
                }
            }
        ]
    }
};
