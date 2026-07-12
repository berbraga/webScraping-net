'use client';

import { exportSearchCsv } from '../lib/searchesApi';

export default function ExportButton({ searchId, disabled }) {
  function handleClick() {
    if (!searchId) return;
    window.location.href = exportSearchCsv(searchId);
  }

  return (
    <button
      type="button"
      className="btn-primary"
      onClick={handleClick}
      disabled={disabled || !searchId}
    >
      Exportar CSV
    </button>
  );
}
