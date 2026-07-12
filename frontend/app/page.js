'use client';

import { useEffect, useMemo, useState } from 'react';
import SearchForm from '../components/SearchForm';
import BusinessList from '../components/BusinessList';
import SearchProgress from '../components/SearchProgress';
import ExportButton from '../components/ExportButton';
import NameFilter from '../components/NameFilter';
import { cancelSearch, createSearch, getSearch, listBusinesses } from '../lib/searchesApi';
import { filterByName, isProcessingStatus } from '../lib/homeView';

export default function HomePage() {
  const [search, setSearch] = useState(null);
  const [businesses, setBusinesses] = useState([]);
  const [nameFilter, setNameFilter] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const [cancelling, setCancelling] = useState(false);

  const processing = loading || isProcessingStatus(search?.status);
  const showResultsChrome = Boolean(search) && !processing;
  const filteredBusinesses = useMemo(
    () => filterByName(businesses, nameFilter),
    [businesses, nameFilter],
  );

  useEffect(() => {
    if (!search?.id) return undefined;
    const terminal = ['completed', 'cancelled', 'failed'].includes(search.status);
    if (terminal) return undefined;

    const timer = setInterval(async () => {
      try {
        const [nextSearch, list] = await Promise.all([
          getSearch(search.id),
          listBusinesses(search.id),
        ]);
        setSearch(nextSearch);
        setBusinesses(list.items || []);
      } catch (err) {
        setError(err.message);
      }
    }, 2000);

    return () => clearInterval(timer);
  }, [search?.id, search?.status]);

  async function handleSubmit(values) {
    setError('');
    setNameFilter('');
    setLoading(true);
    try {
      const created = await createSearch(values);
      setSearch(created);
      const list = await listBusinesses(created.id);
      setBusinesses(list.items || []);
    } catch (err) {
      setError(err.message);
      setSearch(null);
      setBusinesses([]);
    } finally {
      setLoading(false);
    }
  }

  async function handleCancel() {
    if (!search?.id) return;
    setCancelling(true);
    try {
      const cancelled = await cancelSearch(search.id);
      setSearch(cancelled);
      const list = await listBusinesses(search.id);
      setBusinesses(list.items || []);
    } catch (err) {
      setError(err.message);
    } finally {
      setCancelling(false);
    }
  }

  const canExport = search && ['completed', 'cancelled', 'running', 'failed'].includes(search.status);
  const emptyMessage = search
    ? (nameFilter
      ? 'Nenhum comércio corresponde ao filtro.'
      : 'Nenhum comércio encontrado para esta busca.')
    : 'Inicie uma busca para ver resultados.';

  return (
    <main className="container">
      <header className="hero">
        <h1>Busca de Comércios no Google Maps</h1>
        <p className="lead">
          Informe a região e o termo para coletar nome, telefone, site e avaliação.
        </p>
      </header>

      <SearchForm onSubmit={handleSubmit} disabled={loading} busy={processing} />
      {error && <p className="error" role="alert">{error}</p>}

      {search && (
        <SearchProgress
          search={search}
          onCancel={handleCancel}
          cancelling={cancelling}
          showCancel={processing}
        />
      )}

      {showResultsChrome && (
        <>
          <div className="results-toolbar">
            <ExportButton searchId={search?.id} disabled={!canExport} />
            <NameFilter value={nameFilter} onChange={setNameFilter} />
          </div>
          <BusinessList
            items={filteredBusinesses}
            totalCount={businesses.length}
            emptyMessage={emptyMessage}
          />
        </>
      )}

      {!search && !error && (
        <p className="hint">Preencha o formulário acima para iniciar a coleta.</p>
      )}
    </main>
  );
}
