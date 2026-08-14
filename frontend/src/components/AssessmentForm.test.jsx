import {
  fireEvent,
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
import AssessmentForm from './AssessmentForm';
import {
  validateAmstar2Assessment,
} from '../api/amstarApi';

vi.mock('../api/amstarApi', () => ({
  validateAmstar2Assessment: vi.fn(),
}));

const setup = {
  reviewTitle: 'Testoversikt',
  reviewer: 'Forsker 01',
  criticalDomains: [
    {
      itemNumber: 2,
      rationale:
        'Forhåndsdefinert i protokollen.',
    },
  ],
};

describe('AssessmentForm', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('renders all 16 assessment items', () => {
    render(<AssessmentForm setup={setup} />);

    expect(
      screen.getAllByRole('group', {
        name: /vurdering av punkt/i,
      }),
    ).toHaveLength(16);

    expect(
      screen.getByRole('group', {
        name: 'Vurdering av punkt 1',
      }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('group', {
        name: 'Vurdering av punkt 16',
      }),
    ).toBeInTheDocument();
  });

  it('does not call the API for an incomplete assessment', () => {
    render(<AssessmentForm setup={setup} />);

    fireEvent.click(
      screen.getByRole('button', {
        name: /valider komplett vurdering/i,
      }),
    );

    expect(
      screen.getByText(
        /16 punkter har manglende eller inkonsistente opplysninger/i,
      ),
    ).toBeInTheDocument();

    expect(
      validateAmstar2Assessment,
    ).not.toHaveBeenCalled();
  });

  it('only offers no-meta-analysis for eligible items', () => {
    render(<AssessmentForm setup={setup} />);

    const itemOne = screen.getByRole(
      'group',
      {
        name: 'Vurdering av punkt 1',
      },
    );

    const itemEleven = screen.getByRole(
      'group',
      {
        name: 'Vurdering av punkt 11',
      },
    );

    expect(
      itemOne.querySelector(
        'option[value="NoMetaAnalysisConducted"]',
      ),
    ).toBeNull();

    expect(
      itemEleven.querySelector(
        'option[value="NoMetaAnalysisConducted"]',
      ),
    ).not.toBeNull();
  });
});