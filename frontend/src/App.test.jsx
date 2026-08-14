import {
  render,
  screen,
} from '@testing-library/react';
import {
  beforeEach,
  describe,
  expect,
  it,
  vi,
} from 'vitest';
import App from './App';
import {
  getAmstar2Metadata,
  getHealth,
} from './api/amstarApi';

vi.mock('./api/amstarApi', () => ({
  getHealth: vi.fn(),
  getAmstar2Metadata: vi.fn(),
}));

const metadata = {
  instrumentName: 'AMSTAR 2',
  instrumentVersion: '2017',
  totalItems: 16,
  proposedDefaultCriticalDomains: [
    2, 4, 7, 9, 11, 13, 15,
  ],
  criticalDomainNotice:
    'Critical domains must be prespecified.',
  scoringNotice:
    'Responses must not be combined into a numerical total score.',
  currentCapabilities: [
    'Structural validation',
  ],
  unavailableCapabilities: [
    'Automatic overall-confidence calculation',
  ],
};

describe('App', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('shows verified AMSTAR 2 metadata', async () => {
    getHealth.mockResolvedValue({
      status: 'Healthy',
    });

    getAmstar2Metadata.mockResolvedValue(metadata);

    render(<App />);

    expect(
      await screen.findByText('AMSTAR 2'),
    ).toBeInTheDocument();

    expect(
      screen.getByText('(2017)'),
    ).toBeInTheDocument();

    expect(
      screen.getByText(/16 punkter/i),
    ).toBeInTheDocument();

    expect(
      screen.getByText('API: Healthy'),
    ).toBeInTheDocument();

    expect(
      screen.getByText(/must not be combined/i),
    ).toBeInTheDocument();
  });

  it('shows an accessible error when the API is unavailable', async () => {
    getHealth.mockRejectedValue(
      new Error('Connection failed'),
    );

    getAmstar2Metadata.mockRejectedValue(
      new Error('Connection failed'),
    );

    render(<App />);

    const alert = await screen.findByRole('alert');

    expect(alert).toHaveTextContent(
      /kunne ikke koble til API-et/i,
    );

    expect(
      screen.getByText('API: Unavailable'),
    ).toBeInTheDocument();
  });
});