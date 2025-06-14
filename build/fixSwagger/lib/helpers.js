const jp = require("jsonpath");
const { swaggerPath } = require("./swaggerPath")
const { writeFileSync, readFileSync } = require("fs")
const { discoverOriginalSwaggerFile } = require("./swaggerFileDiscovery");
const deepmerge = require("deepmerge");
const { runPathAction } = require("./pathActions");

function makeJsonPathFilter(path) { 
    return path === "*" ? ".*" : `['${path}']`
}

function makePathFilter(path, method) {
    return `$.paths${makeJsonPathFilter(path)}${makeJsonPathFilter(method)}`;    
}

class Fixer {
    /**
     * Creates an instance of the `Fixer` class
     * @param {*} openApiSpec the openApi specification as an object
     * @param {*} fixes the fixes to apply
     */
    constructor(openApiSpec, fixes) {
        this.openApiSpec = openApiSpec;
        this.fixes = fixes;
    }

    get componentBasePath() { return `${this.fixes.components?.componentJsonPath?.replace(/\.$/, "") ?? "$.components.schemas"}.` }
    get componentPath() { return this.componentBasePath.substring(0, this.componentBasePath.length - 1) };
    
    /**
     * Perform a JSON path query against the `openApiSpec`
     * @param {string} jsonPath The JSON path to query against the `openApiSpec`
     * @returns the matched nodes
     */
    query(jsonPath) {
        return jp.nodes(this.openApiSpec, jsonPath);
    }

    generateJsonPath(component, property) {
        if (!component) throw "'component' is a required value";
    
        return property
            ? property === true ? `${this.componentBasePath}${component}.properties` : `${this.componentBasePath}${component}.properties.${property}`
            : `${this.componentBasePath}${component}`
    }

    getComponentsNode = () => { 
        this.componentPath.split(".").filter(v => v != "$")
            .reduce(
                (currentValue, v) => currentValue[v] = currentValue[v] ?? {},
                this.openApiSpec);

        return this.query(this.componentPath)[0].value;
    }
    
    /**
     * Scans for all `object` types in the api spec
     * and sets `additionalProperties` to false
     * @returns this
     */
    setAdditionalPropertiesToFalse() {
        this.query("$..*[?(@.type == 'object')]")
            .forEach(o => {
                o.value.additionalProperties = false
            });

        return this;
    }

    /**
     * Removes `maxLength`, `minLength` and `pattern`
     * properties from any property definition
     * of type string that either has a `format`
     * or an `enum` list
     * @returns this
     */
    removeInvalidStringConstraints() {
        this.query("$..*[?(@.type == 'string' && (@.format || @.enum))]")
            .forEach(c => {
                delete c.value.maxLength;
                delete c.value.minLength;
                delete c.value.pattern;
            })

        return this;
    }

    /**
     * Fixes any regex patterns that effectively only 
     * include one character
     * @returns this
     */
    fixBrokenPatterns() {
        this.query("$..*[?(@.pattern)]")
            .forEach(c => {
                c.value.pattern = /\*|\{/.exec(c.value.pattern)
                    ? c.value.pattern
                    : `(${c.value.pattern})*`
            })

        return this;
    }

    /**
     * Ensures that any non-required properties are set to `nullable: true`
     * @returns this
     */
    fixNonRequiredProperties() {
        this.query(this.generateJsonPath("*"))
            .forEach(c => {
                if (!c.value.properties) return;
                var requiredFields = c.value.required || [];

                for (const [key, value] of Object.entries(c.value.properties)) {
                    var isNotRequired = requiredFields.indexOf(key) === -1;
                    
                    const avoid = ["string", "number", "integer", "boolean"];
                    if (
                        (isNotRequired && value.type === "string" && !value.format && !value.enum) || 
                        (isNotRequired && !avoid.includes(value.type) && !value.$ref)
                    ) {
                        value.nullable = true;
                    }
                }
            })

        return this;
    }

    /**
     * Ensure all request bodies are required
     * @returns this
     */
    fixUpRequestBodies() {
        this.query("$..requestBody")
            .forEach(rb => rb.value.required = true);

        return this;
    }

    /**
     * Fixes `operationId`'s with invalid characters
     * @returns this
     */
    fixOperationIds() {
        this.query("$..*[?(@.operationId)]")
            .forEach(c => c.value.operationId = c.value.operationId.replace(/'/, ""));

        return this;
    }

    /**
     * Updates or creates a component based on name.
     * It will do nothing if a wildcard is used for the name and nothing matches
     * @param {*} component The fixes file definition of a component adjustment
     * @returns this
     */
    updateOrCreateComponent(component) {
        const nodes = this.query(this.generateJsonPath(component.name));
        if (nodes.length === 0) {
            if (component.name === "*") return;

            this.getComponentsNode()[component.name] = component.merge;
            return this;
        }

        nodes.forEach(c => this.getComponentsNode()[c.path[c.path.length - 1]] = deepmerge(c.value, component.merge))
        return this;
    }

    /**
     * Updates or creates components based on the `fixes.components` section
     * @returns this
     */
    fixComponents() {
        this.fixes.components.items.forEach(c => this.updateOrCreateComponent(c));

        return this;
    }

    /**
     * Fixes responses in the `openApi` specification
     * @returns this
     */
    fixResponses() {
        this.fixes.responses.items.forEach(f =>
            this.query(makePathFilter(f.path, f.method))            
                .forEach(r => f.actions.forEach(a => runPathAction(a, r.value)))
        );

        return this;
    }

    /**
     * Fixes parameters in the `openApi` specification
     * @returns this
     */
    fixParameters() {
        this.fixes.parameters.items.forEach(f =>
        {
            const paths = this.query(makePathFilter(f.path, f.method));

            paths.forEach(p => {
                if (f.create) {
                    p.value.parameters = p.value.parameters ?? [];
                    p.value.parameters.push(f.create);
                    return;
                }
                
                if (!p.value.parameters) return;

                const parameters = p.value.parameters.filter(p => f.name === "*" || (f.name !== "*" && f.name === p.name))
                parameters.forEach((n, i) => p.value.parameters[i] = deepmerge(n, f.merge));
            });
        })

        return this;
    }

    /**
     * Ensure the source spec doesn't repeat enums
     * @returns this
     */
    normaliseEnums() {
        const enumNodes = jp.nodes(this.openApiSpec, "$..*[?(@.enum)]");
        const result = enumNodes.reduce((agg, n) =>
        {
            const group = n.value.enum.reduce((v1, v2) => `${v1}:${v2}`)
            const value = agg[group] ?? (agg[group] = { items: [] });
            value.items.push(n);

            return agg;
        },
        {});
        
        const toEnumName = name => `${name.substring(0, 1).toUpperCase()}${name.substring(1)}`;
        const componentSchemas = jp.query(this.openApiSpec, "$.components.schemas")[0];
        const generateComponentName = name => {
            let generatedName = name;
            let index = 2;
            
            while (componentSchemas[generatedName]) {
                generatedName = `${name}${index++}`;
            }

            return generatedName;
        };

        Object.getOwnPropertyNames(result).forEach(n =>
        {
            const setName = (name) => result[n].name = toEnumName(name);
            const value = result[n];

            if (value.items.length < 2) { 
                delete result[n];
                return;
            }
            
            const firstOne = value.items[0];
            if (typeof firstOne.path[firstOne.path.length - 2] === "number") {
                setName(jp.parent(this.openApiSpec, jp.stringify(firstOne.path)).name);
            }
            else {
                setName(firstOne.path[firstOne.path.length - 1]);
            }
            const name = generateComponentName(value.name);
            const schemaName = `#/components/schemas/${name}`;

            componentSchemas[name] = JSON.parse(JSON.stringify(firstOne.value));

            value.items.map(i => i.value).forEach(i =>
            {
                Object.getOwnPropertyNames(i).forEach(n => delete i[n]);
                i["$ref"] = schemaName;
            });
        });

        writeFileSync("c:\\temp\\enums.json", JSON.stringify(result, null, 2));
        return this;
    }
    /**
     * Fixes the `openApi` specification  using the `fixes` that were passed into the constructor
     */
    fix() {
        this.executeIf(this.fixes.components, () => this.fixComponents())
            .executeIf(this.fixes.normaliseEnums, () => this.normaliseEnums())
            .executeIf(this.fixes.responses, () => this.fixResponses())
            .executeIf(this.fixes.parameters, () => this.fixParameters())
            .executeIf(this.fixes.ensureRequestBodiesAreRequired, () => this.fixUpRequestBodies())
            .executeIf(this.fixes.setAdditionalPropertiesToFalse, () => this.setAdditionalPropertiesToFalse())
            .executeIf(this.fixes.fixNonRequiredProperties, () => this.fixNonRequiredProperties())
            .executeIf(this.fixes.fixOperationIds, () => this.fixOperationIds())
            .executeIf(this.fixes.fixUpWithJsonPath, () => this.fixUpWithJsonPath())

        this.save();
    }

    /**
     * Conditionally execite the provided lambda
     * @param {boolean} condition If this is `true` then `toExecute` is run
     * @param {*} toExecute A parameterless function to run if `condition` is `true`
     * @returns 
     */
    executeIf(condition, toExecute) {
        if (condition) toExecute()
        return this;
    }

    fixUpWithJsonPath() {
        this.fixes.fixUpWithJsonPath.forEach(j =>
        {
            var toFix = jp.nodes(this.openApiSpec, j.path);
            toFix.forEach(f =>
            {
                if (j.clear) {
                    Object.getOwnPropertyNames(f.value).forEach(p => delete f.value[p]);
                }
                
                if (j.remove) {
                    j.remove.forEach(r =>
                    {
                        delete f.value[r];
                    })
                }

                if (j.merge) {
                    Object.assign(f.value, j.merge);
                    return;
                }

                if (j.rename) {                    
                    const { parent, oldName } = this.getParentAndOldName(f)

                    parent[j.rename] = parent[oldName];
                    delete parent[oldName]
                    return;
                }
            });
        });

        return this;
    }

    /**
     * Saves out the `swagger.json` file that `NSwag` 
     * will use to generate the `ApiClient.g.cs` file
     */
    save() {
        writeFileSync(swaggerPath("swagger.json"), JSON.stringify(this.openApiSpec, null, 2), "utf-8");
    }
}

/**
 * Applies fixes from the given `fileName`
 */
function fix(fileName) {
    const schema = discoverOriginalSwaggerFile();
    const fixes = JSON.parse(readFileSync(fileName, "utf8"));
    new Fixer(schema, fixes).fix();
}

exports.fix = fix;