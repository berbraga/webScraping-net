import { render, screen } from '@testing-library/react';
import { describe, expect, it, vi } from 'vitest';
import SearchProgress from '../components/SearchProgress';

describe('SearchProgress', () => {
  it('shows processed/total and processing label', () => {
    render(
      <SearchProgress
        search={{ status: 'running', processedCount: 3, totalFound: 8, failedCount: 0 }}
        onCancel={vi.fn()}
      />,
    );

    expect(screen.getByText(/processando/i)).toBeInTheDocument();
    expect(screen.getByText(/3\/8 processados/i)).toBeInTheDocument();
  });

  it('exposes progressbar with ratio', () => {
    render(
      <SearchProgress
        search={{ status: 'running', processedCount: 3, totalFound: 8, failedCount: 0 }}
        onCancel={vi.fn()}
      />,
    );

    const bar = screen.getByRole('progressbar');
    expect(bar).toHaveAttribute('aria-valuenow', '38');
  });
});
