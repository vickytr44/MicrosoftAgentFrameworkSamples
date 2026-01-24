import React, { useState } from "react";

export type Parameter = {
  name: string;
  description: string;
  required: boolean;
};

export function DynamicForm({ parameters, onSubmit }: { parameters: Parameter[]; onSubmit: (values: Record<string, string>) => void }) {
  const [form, setForm] = useState<Record<string, string>>({});

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setForm({ ...form, [e.target.name]: e.target.value });
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    onSubmit(form);
  };

  return (
    <form
      onSubmit={handleSubmit}
      style={{
        margin: "2rem auto",
        maxWidth: 400,
        padding: "2rem 2rem 1.5rem 2rem",
        border: "1px solid #e5e7eb",
        borderRadius: 12,
        background: "#fff",
        boxShadow: "0 2px 8px rgba(0,0,0,0.06)",
        display: "flex",
        flexDirection: "column",
        gap: "1.2rem",
      }}
    >
      {parameters.map((param) => (
        <div key={param.name} style={{ display: "flex", flexDirection: "column", gap: "0.4rem" }}>
          <label htmlFor={param.name} style={{ fontWeight: 500, marginBottom: 2 }}>{param.description}:</label>
          <input
            id={param.name}
            type="text"
            name={param.name}
            required={param.required}
            value={form[param.name] || ""}
            onChange={handleChange}
            style={{
              padding: "0.5rem 0.75rem",
              border: "1px solid #cbd5e1",
              borderRadius: 6,
              fontSize: 16,
              outline: "none",
              background: "#f9fafb",
              transition: "border 0.2s",
            }}
          />
        </div>
      ))}
      <button
        type="submit"
        style={{
          marginTop: "0.5rem",
          padding: "0.6rem 1.2rem",
          background: "#6366f1",
          color: "#fff",
          border: "none",
          borderRadius: 6,
          fontWeight: 600,
          fontSize: 16,
          cursor: "pointer",
          boxShadow: "0 1px 4px rgba(99,102,241,0.08)",
          transition: "background 0.2s",
        }}
        onMouseOver={e => (e.currentTarget.style.background = '#4f46e5')}
        onMouseOut={e => (e.currentTarget.style.background = '#6366f1')}
      >
        Submit
      </button>
    </form>
  );
}