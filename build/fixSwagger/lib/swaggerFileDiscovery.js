const { existsSync, writeFileSync, readFileSync } = require("fs");
const { parse } = require("yaml");
const { swaggerPath } = require("./swaggerPath");

/**
 * Searches for a `swagger.yaml` file. If found that is used for the openApi spec.
 * If not found then it must have an `original-swagger.json` file to use.
 */
function discoverOriginalSwaggerFile() {
    const yamlVersion = swaggerPath("swagger.yaml");
    const jsonVersion = swaggerPath("original-swagger.json");

    if (existsSync(yamlVersion))
    {
        var result = readFileSync(yamlVersion, "utf8");
        writeFileSync(swaggerPath("original-swagger.json"), result);
        return parse(result);
    }

    return JSON.parse(readFileSync(jsonVersion, "utf8"));
}

exports.discoverOriginalSwaggerFile = discoverOriginalSwaggerFile;