const { join } = require("path");

const swaggerPath = (file) => join("../../src/LSL.Sentinet.ApiClient", file);

exports.swaggerPath = swaggerPath;