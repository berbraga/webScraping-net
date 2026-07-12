'use client';

export default function NameFilter({ value, onChange }) {
  return (
    <label className="name-filter">
      <span className="sr-only">Filtrar por nome</span>
      <input
        type="search"
        value={value}
        onChange={(e) => onChange?.(e.target.value)}
        placeholder="Filtrar por nome..."
        aria-label="Filtrar por nome"
      />
    </label>
  );
}
