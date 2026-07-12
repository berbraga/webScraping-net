import { describe, expect, it } from 'vitest';
import {
  filterByName,
  isProcessingStatus,
  progressRatio,
  statusLabel,
  statusTone,
} from '../lib/homeView';

describe('homeView helpers', () => {
  const items = [
    { id: '1', name: 'Barbearia do Zé' },
    { id: '2', name: 'Café Central' },
  ];

  it('filters by name case-insensitively', () => {
    expect(filterByName(items, 'bar')).toHaveLength(1);
    expect(filterByName(items, 'CAFÉ')).toHaveLength(1);
    expect(filterByName(items, '')).toHaveLength(2);
  });

  it('maps processing statuses', () => {
    expect(isProcessingStatus('running')).toBe(true);
    expect(isProcessingStatus('pending')).toBe(true);
    expect(isProcessingStatus('completed')).toBe(false);
    expect(statusLabel('running')).toBe('processando');
    expect(statusLabel('completed')).toBe('completed');
    expect(statusTone('running')).toBe('processing');
    expect(statusTone('completed')).toBe('completed');
  });

  it('computes progress ratio', () => {
    expect(progressRatio(3, 8)).toBeCloseTo(0.375);
    expect(progressRatio(0, 0)).toBe(0);
    expect(progressRatio(10, 8)).toBe(1);
  });
});
