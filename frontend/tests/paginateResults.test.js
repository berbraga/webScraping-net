import { describe, expect, it } from 'vitest';
import {
  PAGE_SIZE,
  clampPage,
  formatResultsFooter,
  pageRange,
  shouldShowPagination,
  slicePage,
} from '../lib/paginateResults';

function makeItems(count) {
  return Array.from({ length: count }, (_, i) => ({ id: String(i + 1), name: `N${i + 1}` }));
}

describe('paginateResults', () => {
  it('exposes PAGE_SIZE of 60', () => {
    expect(PAGE_SIZE).toBe(60);
  });

  it('shouldShowPagination only above 60', () => {
    expect(shouldShowPagination(0)).toBe(false);
    expect(shouldShowPagination(60)).toBe(false);
    expect(shouldShowPagination(61)).toBe(true);
    expect(shouldShowPagination(125)).toBe(true);
  });

  it('slicePage returns full-size and remainder pages for 125 items', () => {
    const items = makeItems(125);
    expect(slicePage(items, 1)).toHaveLength(60);
    expect(slicePage(items, 1)[0].id).toBe('1');
    expect(slicePage(items, 1)[59].id).toBe('60');
    expect(slicePage(items, 2)[0].id).toBe('61');
    expect(slicePage(items, 2)).toHaveLength(60);
    expect(slicePage(items, 3)).toHaveLength(5);
    expect(slicePage(items, 3)[0].id).toBe('121');
    expect(slicePage(items, 3)[4].id).toBe('125');
  });

  it('clampPage keeps page within bounds', () => {
    expect(clampPage(0, 125)).toBe(1);
    expect(clampPage(-2, 125)).toBe(1);
    expect(clampPage(99, 125)).toBe(3);
    expect(clampPage(2, 60)).toBe(1);
    expect(clampPage(2, 61)).toBe(2);
  });

  it('pageRange and footer for paginated 125 items', () => {
    expect(pageRange(1, 60, 125)).toEqual({ start: 1, end: 60 });
    expect(pageRange(2, 60, 125)).toEqual({ start: 61, end: 120 });
    expect(pageRange(3, 60, 125)).toEqual({ start: 121, end: 125 });

    expect(formatResultsFooter({
      total: 125,
      page: 1,
      pageSize: 60,
      paginationActive: true,
    })).toBe('Mostrando 1–60 de 125');

    expect(formatResultsFooter({
      total: 125,
      page: 3,
      pageSize: 60,
      paginationActive: true,
    })).toBe('Mostrando 121–125 de 125');
  });

  it('footer without pagination uses simple N de Y', () => {
    expect(formatResultsFooter({
      total: 60,
      page: 1,
      paginationActive: false,
      listTotal: 60,
    })).toBe('60 de 60 resultados');

    expect(formatResultsFooter({
      total: 1,
      page: 1,
      paginationActive: false,
      listTotal: 8,
    })).toBe('1 de 8 resultados');
  });

  it('handles exactly 61 items as two pages', () => {
    const items = makeItems(61);
    expect(shouldShowPagination(61)).toBe(true);
    expect(slicePage(items, 1)).toHaveLength(60);
    expect(slicePage(items, 2)).toHaveLength(1);
    expect(formatResultsFooter({
      total: 61,
      page: 2,
      paginationActive: true,
    })).toBe('Mostrando 61–61 de 61');
  });

  it('handles empty list', () => {
    expect(slicePage([], 1)).toEqual([]);
    expect(pageRange(1, 60, 0)).toEqual({ start: 0, end: 0 });
  });
});
