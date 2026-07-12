import { render, screen, fireEvent } from '@testing-library/react';
import { describe, expect, it, vi } from 'vitest';
import SearchForm from '../components/SearchForm';

describe('SearchForm', () => {
  it('renders three labeled fields and submit button', () => {
    render(<SearchForm onSubmit={vi.fn()} />);

    expect(screen.getByLabelText(/região/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/termo \/ categoria/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/limite máximo/i)).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /buscar comércios/i })).toBeInTheDocument();
  });

  it('submits region, query and maxResults', () => {
    const onSubmit = vi.fn();
    render(<SearchForm onSubmit={onSubmit} />);

    fireEvent.change(screen.getByLabelText(/região/i), { target: { value: 'Pinheiros' } });
    fireEvent.change(screen.getByLabelText(/termo \/ categoria/i), { target: { value: 'cafeterias' } });
    fireEvent.change(screen.getByLabelText(/limite máximo/i), { target: { value: '12' } });
    fireEvent.click(screen.getByRole('button', { name: /buscar comércios/i }));

    expect(onSubmit).toHaveBeenCalledWith({
      region: 'Pinheiros',
      query: 'cafeterias',
      maxResults: '12',
    });
  });

  it('shows Buscando... when busy', () => {
    render(<SearchForm onSubmit={vi.fn()} busy />);
    expect(screen.getByRole('button', { name: /buscando\.\.\./i })).toBeDisabled();
  });
});
