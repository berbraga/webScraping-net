'use client';

import { useEffect, useMemo, useState } from 'react';
import { isRatingSortAllowed } from '../lib/homeView';
import {
  PAGE_SIZE,
  clampPage,
  formatResultsFooter,
  shouldShowPagination,
  slicePage,
} from '../lib/paginateResults';
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
  const [currentPage, setCurrentPage] = useState(1);
  const rows = items || [];
  const listTotal = totalCount ?? rows.length;
  const sortAllowed = isRatingSortAllowed(searchStatus);

  useEffect(() => {
    setRatingSort(null);
    setCurrentPage(1);
  }, [searchId]);

  useEffect(() => {
    setCurrentPage(1);
  }, [items]);

  useEffect(() => {
    setCurrentPage(1);
  }, [ratingSort]);

  const orderedRows = useMemo(() => {
    if (!ratingSort || !sortAllowed) {
      return rows;
    }
    return sortByRating(rows, ratingSort);
  }, [rows, ratingSort, sortAllowed]);

  const paginationActive = shouldShowPagination(orderedRows.length);
  const safePage = clampPage(currentPage, orderedRows.length, PAGE_SIZE);
  const displayRows = paginationActive
    ? slicePage(orderedRows, safePage, PAGE_SIZE)
    : orderedRows;

  const lastPage = Math.max(1, Math.ceil(orderedRows.length / PAGE_SIZE) || 1);
  const canGoPrev = paginationActive && safePage > 1;
  const canGoNext = paginationActive && safePage < lastPage;

  const footerText = formatResultsFooter({
    total: orderedRows.length,
    page: safePage,
    pageSize: PAGE_SIZE,
    paginationActive,
    listTotal,
  });

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
          <p className="results-footer">0 de {listTotal} resultados</p>
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
              <th>Criação do site</th>
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
                <td><FieldValue value={item.siteCreationYear} /></td>
                <td><FieldValue value={item.rating} /></td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
      {paginationActive && (
        <div className="pagination-controls" role="navigation" aria-label="Paginação de resultados">
          <button
            type="button"
            className="pagination-button"
            disabled={!canGoPrev}
            onClick={() => setCurrentPage((page) => Math.max(1, page - 1))}
          >
            Anterior
          </button>
          <button
            type="button"
            className="pagination-button"
            disabled={!canGoNext}
            onClick={() => setCurrentPage((page) => page + 1)}
          >
            Próxima
          </button>
        </div>
      )}
      <p className="results-footer">{footerText}</p>
    </div>
  );
}
