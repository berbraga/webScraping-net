import { render, screen, fireEvent } from '@testing-library/react';
import { describe, expect, it, beforeEach, afterEach } from 'vitest';
import ExportButton from '../components/ExportButton';

describe('ExportButton', () => {
  const original = window.location;

  beforeEach(() => {
    delete window.location;
    window.location = { href: '' };
  });

  afterEach(() => {
    window.location = original;
  });

  it('sets download url on click', () => {
    render(<ExportButton searchId="abc123" />);
    fireEvent.click(screen.getByRole('button', { name: /exportar csv/i }));
    expect(window.location.href).toContain('/api/searches/abc123/export');
  });
});
