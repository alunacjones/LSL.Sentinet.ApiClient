/**
 * Node.js app to fix a provided 3rd party swagger file
 */
const { fix } = require("./lib")

fix("./fixes.json");