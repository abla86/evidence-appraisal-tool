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