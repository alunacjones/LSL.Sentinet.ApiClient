const fs = require("fs");
const path = "../../src/LSL.Sentinet.ApiClient/ApiClient.g.cs";
var content = fs.readFileSync(path, { encoding: "utf-8" });
content = content.replaceAll("[Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]", "");
fs.writeFileSync(path, content, { encoding: "utf-8" });