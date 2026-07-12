import { render, screen, fireEvent } from '@testing-library/react';
import { describe, expect, it, vi } from 'vitest';
import NameFilter from '../components/NameFilter';

describe('NameFilter', () => {
  it('renders placeholder and calls onChange', () => {
    const onChange = vi.fn();
    render(<NameFilter value="" onChange={onChange} />);

    const input = screen.getByPlaceholderText(/filtrar por nome/i);
    expect(input).toBeInTheDocument();
    fireEvent.change(input, { target: { value: 'bar' } });
    expect(onChange).toHaveBeenCalledWith('bar');
  });
});
