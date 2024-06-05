const WebpackRequireFrom = require("webpack-require-from");

module.exports = {
  configureWebpack: {
    plugins: [
      new WebpackRequireFrom({
        variableName: "resourceBasePath",
      }),
    ],
  },
  devServer: {
    //proxy: "https://lerchen.net/korga",
    proxy: "http://localhost:10501",
  },
};
