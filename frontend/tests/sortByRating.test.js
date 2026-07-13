import { describe, expect, it } from 'vitest';
import {
  hasRating,
  nextRatingSortDirection,
  sortByRating,
} from '../lib/sortByRating';

const sample = [
  { id: '1', name: 'Low', rating: 1 },
  { id: '2', name: 'High', rating: 5 },
  { id: '3', name: 'Mid', rating: 3 },
];

describe('sortByRating', () => {
  it('sorts descending by rating', () => {
    const sorted = sortByRating(sample, 'desc');
    expect(sorted.map((x) => x.rating)).toEqual([5, 3, 1]);
  });

  it('sorts ascending by rating', () => {
    const sorted = sortByRating(sample, 'asc');
    expect(sorted.map((x) => x.rating)).toEqual([1, 3, 5]);
  });

  it('keeps missing ratings last for both directions', () => {
    const mixed = [
      { id: 'a', rating: 4 },
      { id: 'b', rating: null },
      { id: 'c', rating: 2 },
      { id: 'd', rating: '' },
      { id: 'e', rating: 'N/A' },
      { id: 'f', rating: 5 },
    ];

    expect(sortByRating(mixed, 'desc').map((x) => x.id)).toEqual([
      'f', 'a', 'c', 'b', 'd', 'e',
    ]);
    expect(sortByRating(mixed, 'asc').map((x) => x.id)).toEqual([
      'c', 'a', 'f', 'b', 'd', 'e',
    ]);
  });

  it('returns copy of original order when direction is null', () => {
    const sorted = sortByRating(sample, null);
    expect(sorted.map((x) => x.id)).toEqual(['1', '2', '3']);
    expect(sorted).not.toBe(sample);
  });
});

describe('nextRatingSortDirection', () => {
  it('cycles null → desc → asc → desc', () => {
    expect(nextRatingSortDirection(null)).toBe('desc');
    expect(nextRatingSortDirection('desc')).toBe('asc');
    expect(nextRatingSortDirection('asc')).toBe('desc');
  });
});

describe('hasRating', () => {
  it('rejects empty and N/A', () => {
    expect(hasRating(null)).toBe(false);
    expect(hasRating('')).toBe(false);
    expect(hasRating('N/A')).toBe(false);
    expect(hasRating(4.5)).toBe(true);
  });
});
