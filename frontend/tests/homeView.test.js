import { describe, expect, it } from 'vitest';
import {
  filterByName,
  isProcessingStatus,
  isRatingSortAllowed,
  progressRatio,
  shouldShowResultsArea,
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

  it('allows rating sort only when not processing', () => {
    expect(isRatingSortAllowed('running')).toBe(false);
    expect(isRatingSortAllowed('pending')).toBe(false);
    expect(isRatingSortAllowed('completed')).toBe(true);
    expect(isRatingSortAllowed('cancelled')).toBe(true);
    expect(isRatingSortAllowed('failed')).toBe(true);
    expect(isRatingSortAllowed(undefined)).toBe(false);
  });

  it('hides results area while processing even if items would exist', () => {
    expect(shouldShowResultsArea({ status: 'running' })).toBe(false);
    expect(shouldShowResultsArea({ status: 'pending' })).toBe(false);
  });

  it('shows results area for terminal statuses', () => {
    expect(shouldShowResultsArea({ status: 'completed' })).toBe(true);
    expect(shouldShowResultsArea({ status: 'cancelled' })).toBe(true);
    expect(shouldShowResultsArea({ status: 'failed' })).toBe(true);
  });

  it('hides results area while submit loading', () => {
    expect(shouldShowResultsArea({ status: 'completed', loading: true })).toBe(false);
    expect(shouldShowResultsArea({ status: undefined, loading: true })).toBe(false);
  });

  it('gates results chrome across a two-search sequence', () => {
    expect(shouldShowResultsArea({ status: 'completed' })).toBe(true);
    expect(shouldShowResultsArea({ status: 'running' })).toBe(false);
    expect(shouldShowResultsArea({ status: 'completed' })).toBe(true);
  });

  it('computes progress ratio', () => {
    expect(progressRatio(3, 8)).toBeCloseTo(0.375);
    expect(progressRatio(0, 0)).toBe(0);
    expect(progressRatio(10, 8)).toBe(1);
  });
});
