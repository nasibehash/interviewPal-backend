import type { ComponentType } from 'react';

type Field =
  | { type: 'text'; name: string; label: string }
  | { type: 'number'; name: string; label: string; min?: number }
  | { type: 'select'; name: string; label: string; options: string[] };

function TextField({ field }: { field: Extract<Field, { type: 'text' }> }) {
  return <label>{field.label}<input name={field.name} /></label>;
}
function NumberField({ field }: { field: Extract<Field, { type: 'number' }> }) {
  return <label>{field.label}<input type="number" name={field.name} min={field.min} /></label>;
}
function SelectField({ field }: { field: Extract<Field, { type: 'select' }> }) {
  return (
    <label>
      {field.label}
      <select name={field.name}>{field.options.map((o) => <option key={o}>{o}</option>)}</select>
    </label>
  );
}

// کارخانهٔ کامپوننت: نوع فیلد ← کامپوننت مناسب
const fieldComponents: { [T in Field['type']]: ComponentType<{ field: Extract<Field, { type: T }> }> } = {
  text: TextField,
  number: NumberField,
  select: SelectField,
};

function FormField({ field }: { field: Field }) {
  const Component = fieldComponents[field.type] as ComponentType<{ field: Field }>;
  return <Component field={field} />;
}

// فرم از روی داده ساخته می‌شود (مثلاً JSON از API)؛ افزودن نوع فیلد جدید فقط یک کامپوننت و یک ردیف در نگاشت است
export function DynamicForm({ fields }: { fields: Field[] }) {
  return (
    <form>
      {fields.map((field) => (
        <FormField key={field.name} field={field} />
      ))}
    </form>
  );
}
