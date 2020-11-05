const WebpackRequireFrom = require('webpack-require-from')

module.exports = {
  configureWebpack: {
    plugins: [
      new WebpackRequireFrom({
        variableName: 'resourceBasePath'
      })
    ]
  }
};
