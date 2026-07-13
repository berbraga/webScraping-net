'use client';

import { useEffect, useMemo, useState } from 'react';
import { isRatingSortAllowed } from '../lib/homeView';
import { nextRatingSortDirection, sortByRating } from '../lib/sortByRating';

function FieldValue({ value }) {
  if (value === null || value === undefined || value === '') {
    return <span className="missing" title="Indisponível">✕</span>;
  }
  return <span>{String(value)}</span>;
}

function SortIcon({ direction }) {
  if (direction === 'desc') {
    return <span className="sort-icon" aria-hidden="true">↓</span>;
  }
  if (direction === 'asc') {
    return <span className="sort-icon" aria-hidden="true">↑</span>;
  }
  return null;
}

export default function BusinessList({
  items,
  totalCount,
  emptyMessage,
  searchStatus,
  searchId,
}) {
  const [ratingSort, setRatingSort] = useState(null);
  const rows = items || [];
  const total = totalCount ?? rows.length;
  const sortAllowed = isRatingSortAllowed(searchStatus);

  useEffect(() => {
    setRatingSort(null);
  }, [searchId]);

  const displayRows = useMemo(() => {
    if (!ratingSort || !sortAllowed) {
      return rows;
    }
    return sortByRating(rows, ratingSort);
  }, [rows, ratingSort, sortAllowed]);

  function handleRatingSortActivate() {
    if (!sortAllowed) {
      return;
    }
    setRatingSort((current) => nextRatingSortDirection(current));
  }

  function handleRatingKeyDown(event) {
    if (event.key === 'Enter' || event.key === ' ') {
      event.preventDefault();
      handleRatingSortActivate();
    }
  }

  if (rows.length === 0) {
    return (
      <div className="results-block">
        <p className="empty">{emptyMessage || 'Nenhum comércio encontrado.'}</p>
        {typeof totalCount === 'number' && (
          <p className="results-footer">0 de {total} resultados</p>
        )}
      </div>
    );
  }

  const ariaSort = ratingSort === 'desc'
    ? 'descending'
    : ratingSort === 'asc'
      ? 'ascending'
      : 'none';

  return (
    <div className="results-block">
      <div className="table-wrap">
        <table className="business-list">
          <thead>
            <tr>
              <th>Nome</th>
              <th>Telefone</th>
              <th>Site</th>
              <th
                className="th-sortable"
                aria-sort={ariaSort}
              >
                <button
                  type="button"
                  className="th-sort-button"
                  onClick={handleRatingSortActivate}
                  onKeyDown={handleRatingKeyDown}
                  aria-label="Ordenar por avaliação"
                >
                  Avaliação
                  <SortIcon direction={sortAllowed ? ratingSort : null} />
                </button>
              </th>
            </tr>
          </thead>
          <tbody>
            {displayRows.map((item) => (
              <tr key={item.id}>
                <td className="cell-name">{item.name}</td>
                <td><FieldValue value={item.phone} /></td>
                <td className="cell-site">
                  {item.website ? (
                    <a href={item.website} target="_blank" rel="noreferrer" title={item.website}>
                      {item.website}
                    </a>
                  ) : (
                    <FieldValue value={item.website} />
                  )}
                </td>
                <td><FieldValue value={item.rating} /></td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
      <p className="results-footer">{rows.length} de {total} resultados</p>
    </div>
  );
}
