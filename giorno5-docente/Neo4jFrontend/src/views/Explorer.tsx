import { useEffect, useState, useCallback, useMemo } from 'react';
import { useSearchParams } from 'react-router-dom';
import { expandGraphNode, type GraphPayloadDto } from '../api';
import { Graph } from '../components/Graph';
import { showToast } from '../components/toast';
import type { GraphEventParams, GraphRenderer } from '../components/graph-renderer';

export function Explorer() {
  const [searchParams] = useSearchParams();
  const [id, setId] = useState(searchParams.get('movieId') ?? '');
  const [label, setLabel] = useState('Movie');

  const [payload, setPayload] = useState<GraphPayloadDto | null>(null);
  const [graphKey, setGraphKey] = useState(0);   // cambiarlo ricrea il grafo da zero
  const [loading, setLoading] = useState(false);

  // Chiede al backend i vicini di un nodo: il componente Graph li aggiunge a quelli già disegnati.
  const expand = useCallback(async (targetLabel: string, targetId: string) => {
    setLoading(true);
    try {
      const { data } = await expandGraphNode({ path : { label: targetLabel, id: targetId}});
      if(data) {
        setPayload(data)
      }
    } catch (err: any) {
      showToast(err?.detail ?? err?.title ?? String(err), 'error');
    } finally {
      setLoading(false);
    }
  }, []);

  // Arrivo dalla tabella dei film: /explorer?movieId=603
  useEffect(() => {
    const movieId = searchParams.get('movieId');
    if (movieId) expand('Movie', movieId);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const handleLoad = () => {
    const trimmedId = id.trim();
    if (!trimmedId) return;
    setGraphKey(k => k + 1);
    setPayload(null);
    expand(label, trimmedId);
  };

  const handleClick = useCallback((params: GraphEventParams, renderer: GraphRenderer) => {
    const nodeId = params.nodes?.[0];
    if(!nodeId) {
      return;
    }
    var node = renderer.nodes.get(nodeId);
    if(node?.group && node?.businessKey) {
      expand(node?.group, node?.businessKey)
    }
  }, [expand]);

  const events = useMemo(() => [{ event: 'click' as const, handler: handleClick }], [handleClick]);

  return (
    <>
      <div className="toolbar">
        <h2>Graph Explorer</h2>
        <input
          type="text"
          className="input-sm"
          placeholder="tmdbId (es. 603)"
          value={id}
          onChange={e => setId(e.target.value)}
          onKeyDown={e => { if (e.key === 'Enter') handleLoad(); }}
        />
        <select value={label} onChange={e => setLabel(e.target.value)}>
          <option value="Movie">Movie</option>
          <option value="Person">Person</option>
        </select>
        <button onClick={handleLoad} disabled={loading}>Esplora</button>
        <span className="muted push-right">Click su un nodo per espandere il vicinato</span>
      </div>
      <Graph key={graphKey} payload={payload} onEvent={events} />
    </>
  );
}
