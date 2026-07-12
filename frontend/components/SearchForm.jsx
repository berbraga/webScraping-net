'use client';

import { useState } from 'react';

export default function SearchForm({ onSubmit, disabled, busy }) {
  const [region, setRegion] = useState('');
  const [query, setQuery] = useState('');
  const [maxResults, setMaxResults] = useState(50);

  function handleSubmit(event) {
    event.preventDefault();
    onSubmit?.({ region, query, maxResults });
  }

  return (
    <form onSubmit={handleSubmit} className="search-card">
      <div className="search-fields">
        <label className="field">
          <span className="field-label">Região</span>
          <input
            name="region"
            value={region}
            onChange={(e) => setRegion(e.target.value)}
            placeholder="barreiros, são josé"
            required
          />
        </label>
        <label className="field">
          <span className="field-label">Termo / categoria</span>
          <input
            name="query"
            value={query}
            onChange={(e) => setQuery(e.target.value)}
            placeholder="barbearia"
            required
          />
        </label>
        <label className="field field-limit">
          <span className="field-label">Limite máximo</span>
          <input
            name="maxResults"
            type="number"
            min={1}
            max={200}
            value={maxResults}
            onChange={(e) => setMaxResults(e.target.value)}
          />
        </label>
      </div>
      <button type="submit" className="btn-primary btn-block" disabled={disabled || busy}>
        {busy ? 'Buscando...' : 'Buscar comércios'}
      </button>
    </form>
  );
}
