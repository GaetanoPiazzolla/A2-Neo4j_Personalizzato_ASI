import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { getMovies, type MovieDto } from '../api';

const PAGE_SIZE = 10;

export function Movies() {
  const [skip, setSkip] = useState(0);
  const [search, setSearch] = useState('');            // testo della ricerca eseguita
  const [searchInput, setSearchInput] = useState('');  // testo che si sta scrivendo

  const [items, setItems] = useState<MovieDto[]>([]);
  const [total, setTotal] = useState(0);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Riparte a ogni cambio di skip o search: è qui che il frontend chiama il backend.
  useEffect(() => {
    let ignore = false;   // risposta arrivata dopo un cambio di pagina: va scartata
    async function load() {
      setLoading(true);
      setError(null);
      try {
        const { data } = await getMovies({ query : { search, skip, take: PAGE_SIZE}});
        if(!ignore && data && data.items) {
          setItems(data.items);
          setTotal(Number(data.totalCount));
        }
      } catch (err: any) {
        if (!ignore) setError(err?.detail ?? err?.title ?? String(err));
      } finally {
        if (!ignore) setLoading(false);
      }
    }
    load();
    return () => { ignore = true; };
  }, [skip, search]);

  function doSearch() {
    // TODO LAB 5.1: salvare searchInput in search e tornare alla prima pagina
    setSearch(searchInput);
    setSkip(0);
  }

  return (
    <>
      <div className="toolbar">
        <h2>Film</h2>
        <input
          type="text"
          className="grow"
          placeholder="Cerca per titolo (es. matrix)..."
          value={searchInput}
          onChange={e => setSearchInput(e.target.value)}
          onKeyDown={e => { if (e.key === 'Enter') doSearch(); }}
        />
        <button onClick={doSearch}>Cerca</button>
      </div>

      <div className="scroll-area">
        <table>
          <thead>
            <tr>
              <th>Poster</th>
              <th>Titolo</th>
              <th>Anno</th>
              <th>Rating IMDb</th>
              <th>Generi</th>
            </tr>
          </thead>
          <tbody>
            {loading ? (
              <tr><td colSpan={5} className="status">Caricamento...</td></tr>
            ) : error ? (
              <tr><td colSpan={5} className="status"><span className="status--error">{error}</span></td></tr>
            ) : items.length === 0 ? (
              <tr><td colSpan={5} className="status">Nessun film trovato</td></tr>
            ) : (
              items.map(m => (
                <tr key={m.tmdbId}>
                  <td>{m.poster && <img src={m.poster} alt="" className="poster" />}</td>
                  <td>
                    <Link to={`/explorer?movieId=${encodeURIComponent(m.tmdbId)}`}>
                      <strong>{m.title}</strong>
                    </Link>
                  </td>
                  <td>{m.year}</td>
                  <td>{m.imdbRating}</td>
                  <td>
                    {(m.genres ?? []).map(g => <span key={g} className="badge">{g}</span>)}
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>

      <div className="toolbar toolbar--between">
        <button disabled={skip === 0} onClick={() => setSkip(s => s - PAGE_SIZE)}>
          ← Precedente
        </button>
        <span>
          {total > 0 ? `${skip + 1}–${Math.min(skip + PAGE_SIZE, total)} di ${total}` : '0 risultati'}
        </span>
        <button disabled={total < skip + PAGE_SIZE} onClick={() => setSkip(s => s + PAGE_SIZE)}>
          Successivo →
        </button>
      </div>
    </>
  );
}
