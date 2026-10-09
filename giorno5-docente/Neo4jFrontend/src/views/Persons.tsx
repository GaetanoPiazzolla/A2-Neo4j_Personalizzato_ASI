import { useState } from 'react';
import {
  createActor, addDirectorLabel, updatePerson, deletePerson, addActedInRelation,
  type PersonDto, type ActedInDto,
} from '../api';
import { showToast } from '../components/toast';

export function Persons() {
  const [loading, setLoading] = useState(false);

  // Form della persona
  const [tmdbId, setTmdbId] = useState('');
  const [name, setName] = useState('');
  const [born, setBorn] = useState('');   // l'input date dà "yyyy-MM-dd", o "" se vuoto
  const [isDirector, setIsDirector] = useState(false);
  const [person, setPerson] = useState<PersonDto | null>(null);

  // Form della relazione
  const [relPersonId, setRelPersonId] = useState('');
  const [relMovieId, setRelMovieId] = useState('');
  const [relRoles, setRelRoles] = useState('');
  const [actedIn, setActedIn] = useState<ActedInDto | null>(null);

  // Stessa forma per ogni azione: esegui, poi un messaggio di successo o l'errore del backend.
  async function run(action: () => Promise<void>, successMessage: string) {
    setLoading(true);
    try {
      await action();
      showToast(successMessage);
    } catch (err: any) {
      // err è il ProblemDetails: detail se c'è (es. il 409), altrimenti title (es. il 400).
      showToast(err?.detail ?? err?.title ?? String(err), 'error');
    } finally {
      setLoading(false);
    }
  }

   const handleCreate = async () => {
    // LIVE CODING 5.3: createActor, poi addDirectorLabel se è anche regista; la risposta in person
    const { data } = await createActor({body : { tmdbId: tmdbId.trim(), name: name.trim(), born: born || null}})
    if(!data) { return }
    if (isDirector) {
      const { data :director } = await addDirectorLabel({ path: {tmdbId: tmdbId.trim()}});
      setPerson(director ?? data)
    }
    else{
      setPerson(data);
    }
  };

  const handleUpdate = () => run(async () => {
    const { data } = await updatePerson({
      path: { tmdbId: tmdbId.trim() },
      body: { name: name.trim(), born: born || null },
    });
    setPerson(data ?? null);
  }, 'Persona aggiornata');

  const handleRelate = () => {
    // TODO LAB 5.3: ruoli da "Neo, The One" a ["Neo", "The One"], poi addActedInRelation; la risposta in actedIn
  };

  const handleDelete = () => {
    // TODO LAB 5.3: conferma con confirm(), poi deletePerson; person torna null
  };

  return (
    <div className="cards">
      <div className="card">
        <h3>Persona</h3>
        <div className="col">
          <div className="field">
            <label>tmdbId</label>
            <input type="text" placeholder="999001" value={tmdbId} onChange={e => setTmdbId(e.target.value)} />
          </div>
          <div className="field">
            <label>Nome</label>
            <input type="text" placeholder="Ada Corsista" value={name} onChange={e => setName(e.target.value)} />
          </div>
          <div className="field">
            <label>Data di nascita</label>
            <input type="date" value={born} onChange={e => setBorn(e.target.value)} />
          </div>
          <label className="checkbox">
            <input type="checkbox" checked={isDirector} onChange={e => setIsDirector(e.target.checked)} />
            Anche regista (solo creazione)
          </label>
          <div className="form-actions">
            <button onClick={handleCreate} disabled={loading}>Crea</button>
            <button onClick={handleUpdate} disabled={loading}>Aggiorna</button>
            <button className="danger" onClick={handleDelete} disabled={loading}>Elimina</button>
          </div>
          {person && (
            <div className="result">
              <strong>{person.name}</strong> ({person.tmdbId}) {person.born && `· nata/o il ${person.born}`}
              <div>{(person.labels ?? []).map(l => <span key={l} className="badge">{l}</span>)}</div>
            </div>
          )}
        </div>
      </div>

      <div className="card">
        <h3>Relazione ACTED_IN</h3>
        <div className="col">
          <div className="field">
            <label>tmdbId della persona</label>
            <input type="text" placeholder="999001" value={relPersonId} onChange={e => setRelPersonId(e.target.value)} />
          </div>
          <div className="field">
            <label>tmdbId del film</label>
            <input type="text" placeholder="603" value={relMovieId} onChange={e => setRelMovieId(e.target.value)} />
          </div>
          <div className="field">
            <label>Ruoli (separati da virgola)</label>
            <input type="text" placeholder="Neo, The One" value={relRoles} onChange={e => setRelRoles(e.target.value)} />
          </div>
          <button onClick={handleRelate} disabled={loading}>Collega</button>
          {actedIn && (
            <div className="result">
              <strong>{actedIn.person}</strong> → {actedIn.movie}
              <div>{(actedIn.roles ?? []).map(r => <span key={r} className="badge">{r}</span>)}</div>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
