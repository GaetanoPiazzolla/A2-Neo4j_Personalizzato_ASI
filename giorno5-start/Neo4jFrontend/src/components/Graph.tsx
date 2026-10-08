import { useEffect, useRef, useState } from 'react';
import type { Options } from 'vis-network';
import { GraphRenderer, type GraphEvent, type GraphEventParams } from './graph-renderer';
import type { GraphPayloadDto } from '../api';

// Quello che la pagina passa a <Graph />: si usa così, il resto del file è già pronto.
interface GraphProps {
  options?: Options;                 // opzioni vis-network in più (facoltative)
  payload?: GraphPayloadDto | null;  // la risposta del backend da aggiungere al grafo
  // Gli eventi da ascoltare; il gestore riceve cosa è stato cliccato e il renderer (per i DataSet).
  onEvent?: Array<{ event: GraphEvent; handler: (params: GraphEventParams, renderer: GraphRenderer) => void }>;
}

// Collega vis-network, che si comanda a mano, a React, che rifà il render da solo.
export function Graph({ options, payload, onEvent }: GraphProps) {
  // useRef conserva un valore tra un render e l'altro, senza provocarne uno nuovo.
  const containerRef = useRef<HTMLDivElement>(null);           // il <div> dove vis-network disegna
  const rendererRef = useRef<GraphRenderer | null>(null);      // il grafo, creato una volta sola

  // 1. All'apertura della pagina ([] = una volta sola): crea il grafo nel div. Alla chiusura lo distrugge.
  useEffect(() => {
    if (!containerRef.current) return;

    const renderer = new GraphRenderer(containerRef.current);
    renderer.init(options);
    rendererRef.current = renderer;

    return () => {
      renderer.destroy();
      rendererRef.current = null;
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  // 2. Collega gli eventi chiesti dalla pagina: ogni gestore riceve (params, renderer).
  useEffect(() => {
    const renderer = rendererRef.current;
    if (!renderer || !onEvent) return;

    const boundHandlers = onEvent.map(({ event, handler }) => {
      const boundHandler = (params: GraphEventParams) => handler(params, renderer);
      renderer.on(event, boundHandler);
      return { event, boundHandler };
    });

    return () => {
      boundHandlers.forEach(({ event, boundHandler }) => renderer.off(event, boundHandler));
    };
  }, [onEvent]);

  // 3. Ogni nuova risposta del backend si aggiunge al grafo con mergeGraph (il live 5.2).
  useEffect(() => {
    if (rendererRef.current && payload) {
      rendererRef.current.mergeGraph(payload);
    }
  }, [payload]);

  const [tooltip, setTooltip] = useState<{ x: number; y: number; props: Record<string, unknown>; label: string } | null>(null);

  // 4. Il tooltip: al passaggio del mouse mostra le proprietà del nodo. Il LAB 5.2 fa lo stesso sul click.
  useEffect(() => {
    const renderer = rendererRef.current;
    if (!renderer) return;

    const handleHover = (params: GraphEventParams) => {
      if (!params.node || !params.pointer) return;
      const node = renderer.nodes.get(params.node);   // dall'id al nodo completo, con proprietà e group
      if (!node || !node.properties) return;

      let x = params.pointer.DOM.x + 15;
      let y = params.pointer.DOM.y + 15;

      // Il tooltip è largo 250px e alto fino a ~400px (poster): non deve uscire dal riquadro.
      if (containerRef.current) {
        if (x + 250 > containerRef.current.offsetWidth) x = params.pointer.DOM.x - 265;
        if (y + 400 > containerRef.current.offsetHeight) y = Math.max(10, params.pointer.DOM.y - 415);
      }

      setTooltip({ x, y, props: node.properties, label: node.group || 'Node' });
    };

    const handleBlur = () => setTooltip(null);

    renderer.on('hoverNode', handleHover);
    renderer.on('blurNode', handleBlur);

    return () => {
      renderer.off('hoverNode', handleHover);
      renderer.off('blurNode', handleBlur);
    };
  }, []);

  // Il div collegato a containerRef, dove vis-network disegna, e sopra il tooltip se c'è.
  return (
    <div className="graph-container-wrapper" style={{ position: 'relative', flex: 1, display: 'flex', minHeight: 0 }}>
      <div ref={containerRef} className="graph-container"></div>
      {tooltip && (
        <div className="custom-tooltip" style={{ position: 'absolute', left: tooltip.x, top: tooltip.y, pointerEvents: 'none' }}>
          <div className="tooltip-header">{tooltip.label}</div>
          {typeof tooltip.props.poster === 'string' && (
            <img src={tooltip.props.poster} alt="" className="tooltip-poster" />
          )}
          <div className="tooltip-body">
            {Object.entries(tooltip.props)
              .filter(([key]) => key !== 'poster')
              .map(([key, value]) => (
                <div key={key}><strong>{key}:</strong> {String(value)}</div>
              ))}
          </div>
        </div>
      )}
    </div>
  );
}
