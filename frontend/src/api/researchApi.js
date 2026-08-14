const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? '';

async function post(path, payload) {
  const response = await fetch(`${API_BASE_URL}${path}`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(payload),
  });

  const data = await response.json();
  if (!response.ok) throw new Error(data.error ?? 'API request failed.');
  return data;
}

export async function getInstruments() {
  const response = await fetch(`${API_BASE_URL}/api/instruments`);
  if (!response.ok) throw new Error('Could not load instruments.');
  return response.json();
}

export const validateCasp = (assessment) =>
  post('/api/casp/validate', assessment);

export const calculateAgree2 = (assessment) =>
  post('/api/agree2/calculate', assessment);

export const evaluateGrade = (assessment) =>
  post('/api/grade/evaluate', assessment);
