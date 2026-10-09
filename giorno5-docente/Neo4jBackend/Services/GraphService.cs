using Neo4j.Driver;
using Neo4jBackend.Models;

namespace Neo4jBackend.Services;

public class GraphService(IGraphSessionFactory sessionFactory)
{
    // LIVE CODING 4.4b: ExpandAsync, il nodo e i suoi vicini

    public async Task<GraphPayloadDto?> ExpandAsyc(string label, string id)
    {
        await using var session = sessionFactory.CreateReadSession();
        return await session.ExecuteReadAsync(async tx =>
        {
            var cursor = await tx.RunAsync(@"
                MATCH (anchor:$($label) {tmdbId: $id})
                OPTIONAL MATCH (anchor)-[r:ACTED_IN|DIRECTED|IN_GENRE]-(n)
                RETURN anchor, collect(DISTINCT n) AS neighbors, collect(DISTINCT r) AS rels
            ", new { label, id});
            
            var records = await cursor.ToListAsync();
            if (records.Count == 0) return null;   // l'àncora non esiste: l'endpoint risponde 404

            var record = records[0];
            // Colonne con nodi e relazioni interi: As<INode> e As<List<...>>, non AsObject (non c'è un DTO).
            var nodes = new List<INode> { record["anchor"].As<INode>() };
            nodes.AddRange(record["neighbors"].As<List<INode>>());
            var rels = record["rels"].As<List<IRelationship>>();

            // Select(GraphMapping.ToNodeDto) è come Select(n => GraphMapping.ToNodeDto(n)).
            return new GraphPayloadDto(
                nodes.Select(GraphMapping.ToNodeDto).ToList(),
                rels.Select(GraphMapping.ToEdgeDto).ToList());
        });
    }
}
