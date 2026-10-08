// Build "peer": vis-data resta una dipendenza esterna, così ne esiste UNA sola copia.
import { Network } from "vis-network";
import type { Options } from "vis-network";
import { DataSet } from "vis-data";
import "vis-network/styles/vis-network.css";
import type { GraphPayloadDto } from "../api";

// L'unico file che parla con vis-network: le pagine lo usano solo tramite <Graph />.

// Colore degli archi per tipo di relazione.
const EDGE_COLORS: Record<string, string> = {
  ACTED_IN: '#e67e22',
  DIRECTED: '#e74c3c',
  IN_GENRE: '#2ecc71',
};

// Aspetto del grafo (forme, colori, fisica): solo impostazioni, non serve toccarle.
const DEFAULT_OPTIONS: Options = {
  interaction: { hover: true }, // abilita gli eventi hoverNode e blurNode (tooltip)
  nodes: {
    shape: 'dot',
    size: 22,
    // Contorno del colore dello sfondo: stacca l'etichetta dagli archi che le passano sotto.
    font: { color: '#e0e0e0', size: 15, strokeWidth: 4, strokeColor: '#1a1a1a' },
    borderWidth: 2,
  },
  edges: {
    width: 2,
    color: { color: '#777', highlight: '#fff', opacity: 0.85 },
    // Senza align: 'horizontal' l'etichetta ruota lungo l'arco e diventa illeggibile.
    font: { color: '#e0e0e0', size: 12, face: 'monospace', align: 'horizontal', strokeWidth: 0, background: '#1a1a1a' },
    // Curva gli archi paralleli (ACTED_IN e DIRECTED tra la stessa coppia).
    smooth: { enabled: true, type: 'dynamic', roundness: 0.5 },
    arrows: { to: { enabled: true, scaleFactor: 0.7 } },
  },
  // Un gruppo per label: il colore dipende dalla prima label del nodo.
  groups: {
    Movie:    { color: { background: '#f1c40f', border: '#f39c12' }, shape: 'box' },
    Person:   { color: { background: '#3498db', border: '#2980b9' } },
    Actor:    { color: { background: '#3498db', border: '#2980b9' } },
    Director: { color: { background: '#9b59b6', border: '#8e44ad' } },
    Genre:    { color: { background: '#2ecc71', border: '#27ae60' }, shape: 'ellipse' },
  },
  physics: {
    solver: 'forceAtlas2Based',
    forceAtlas2Based: { gravitationalConstant: -100, centralGravity: 0.01, springLength: 100, springConstant: 0.08 },
    stabilization: { iterations: 100 },
  },
};

// Un nodo come lo vuole vis-network: mergeGraph traduce ogni NodeDto in questa forma.
export interface VisNode {
  id: string;                            // elementId: unico in tutto il database
  label: string;                         // il testo disegnato sul nodo
  group?: string;                        // la prima label Neo4j: sceglie colore e forma
  properties?: Record<string, unknown>;  // per il tooltip
  businessKey?: string;                  // tmdbId o name: quello che si manda alle API
}

// Un arco: from e to sono gli id (elementId) dei due nodi che collega.
export interface VisEdge {
  id: string;
  from: string;
  to: string;
  label?: string;
  color?: { color: string };
}

// Il wrapper espone solo gli eventi che ci servono.
export type GraphEvent = 'click' | 'doubleClick' | 'hoverNode' | 'blurNode';

export interface GraphEventParams {
  nodes?: string[];
  edges?: string[];
  node?: string; // presente in hoverNode e blurNode
  pointer?: { DOM: { x: number; y: number } };
}

// Il grafo: due archivi in memoria (DataSet) e il Network che li disegna.
export class GraphRenderer {
  private network: Network | null = null;
  // Quello che entra in questi due DataSet compare a schermo, e si aggiorna da solo.
  readonly nodes = new DataSet<VisNode>();
  readonly edges = new DataSet<VisEdge>();
  private container: HTMLElement;

  constructor(container: HTMLElement) {
    this.container = container;
  }

  // Crea il Network nel div, collegato ai due DataSet. Lo chiama Graph.tsx, una volta sola.
  init(overrides: Options = {}) {
    this.network = new Network(
      this.container,
      { nodes: this.nodes, edges: this.edges },
      { ...DEFAULT_OPTIONS, ...overrides },
    );
  }

  // Aggiunge al grafo disegnato i nodi e gli archi della risposta, senza cancellare quelli che ci sono.
  mergeGraph(payload: GraphPayloadDto) {
    // LIVE CODING 5.2: da NodeDto/EdgeDto a VisNode/VisEdge, poi update dei due DataSet
  }

  // Eventi (click, hover...): Graph.tsx li collega per conto della pagina.
  on(event: GraphEvent, callback: (params: GraphEventParams) => void) {
    this.network?.on(event, callback as any);
  }

  off(event: GraphEvent, callback: (params: GraphEventParams) => void) {
    this.network?.off(event, callback as any);
  }

  // vis-network registra un listener su window.resize e un loop di fisica:
  // senza destroy() ogni cambio di vista ne lascia indietro una copia.
  destroy() {
    this.network?.destroy();
    this.network = null;
    this.nodes.clear();
    this.edges.clear();
  }
}
