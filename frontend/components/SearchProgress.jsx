'use client';

import { isProcessingStatus, progressRatio, statusLabel, statusTone } from '../lib/homeView';

export default function SearchProgress({ search, onCancel, cancelling, showCancel = true }) {
  if (!search) {
    return null;
  }

  const total = search.totalFound || 0;
  const processed = search.processedCount || 0;
  const ratio = progressRatio(processed, total);
  const label = statusLabel(search.status);
  const tone = statusTone(search.status);
  const isRunning = isProcessingStatus(search.status);
  const percent = Math.round(ratio * 100);

  return (
    <div className="search-progress">
      <div className="search-progress-row">
        <p className="status-line">
          Status:{' '}
          <strong className={`status-pill status-${tone}`}>{label}</strong>
          {' — '}
          {processed}/{total || '…'} processados
          {search.failedCount ? ` (${search.failedCount} falhas)` : ''}
        </p>
        {showCancel && isRunning && (
          <button type="button" className="btn-ghost" onClick={onCancel} disabled={cancelling}>
            {cancelling ? 'Cancelando…' : 'Cancelar'}
          </button>
        )}
      </div>
      {(isRunning || total > 0) && isRunning && (
        <div
          className="progress-track"
          role="progressbar"
          aria-valuemin={0}
          aria-valuemax={100}
          aria-valuenow={percent}
          aria-label="Progresso da coleta"
        >
          <div className="progress-fill" style={{ width: `${percent}%` }} />
        </div>
      )}
    </div>
  );
}
