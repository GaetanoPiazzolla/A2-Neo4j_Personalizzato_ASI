import { defineConfig } from '@hey-api/openapi-ts';

export default defineConfig({
  input: '../Neo4jBackend/Neo4jBackend.json',
  output: 'src/api/client',
  client: 'fetch',
});
