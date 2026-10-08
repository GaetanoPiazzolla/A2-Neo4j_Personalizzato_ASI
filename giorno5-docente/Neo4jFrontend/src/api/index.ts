import { client } from "./client/client.gen";
export * from "./client";

// Il client in ./client è generato da Neo4jBackend.json (npm run generate-api): non si modifica a mano.
client.setConfig({
  baseUrl: "http://localhost:5234",
  // Una risposta 4xx/5xx diventa un'eccezione con il JSON del ProblemDetails.
  throwOnError: true,
});
