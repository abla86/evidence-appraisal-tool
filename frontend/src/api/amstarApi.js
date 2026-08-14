const API_BASE_URL =
  import.meta.env.VITE_API_URL ??
  'http://localhost:5237';

async function request(path) {
  const response = await fetch(`${API_BASE_URL}${path}`, {
    headers: {
      Accept: 'application/json',
    },
  });

  if (!response.ok) {
    throw new Error(
      `API request failed with status ${response.status}.`,
    );
  }

  return response.json();
}

export function getHealth() {
  return request('/health');
}

export function getAmstar2Metadata() {
  return request('/api/amstar2/metadata');
}
export async function validateAmstar2Assessment(
  assessment,
) {
  const response = await fetch(
    `${API_BASE_URL}/api/amstar2/validate`,
    {
      method: 'POST',
      headers: {
        Accept: 'application/json',
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(assessment),
    },
  );

  if (!response.ok) {
    throw new Error(
      `API validation failed with status ${response.status}.`,
    );
  }

  return response.json();
}
export async function exportAmstar2Assessment(
  assessment,
  format,
) {
  const response = await fetch(
    `${API_BASE_URL}/api/amstar2/export/${format}`,
    {
      method: 'POST',
      headers: {
        Accept: '*/*',
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(assessment),
    },
  );

  if (!response.ok) {
    throw new Error(
      `Export failed with status ${response.status}.`,
    );
  }

  return response.blob();
}