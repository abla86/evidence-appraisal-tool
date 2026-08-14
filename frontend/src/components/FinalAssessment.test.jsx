import {
  fireEvent,
  render,
  screen,
  waitFor,
} from '@testing-library/react';
import {
  beforeEach,
  describe,
  expect,
  it,
  vi,
} from 'vitest';
import FinalAssessment from './FinalAssessment';
import {
  validateAmstar2Assessment,
} from '../api/amstarApi';

vi.mock('../api/amstarApi', () => ({
  validateAmstar2Assessment: vi.fn(),
  exportAmstar2Assessment: vi.fn(),
}));

const assessment = {
  instrumentName: 'AMSTAR 2',
  instrumentVersion: '2017',
  reviewTitle: 'Testoversikt',
  reviewer: 'Forsker 01',
  criticalDomains: [
    {
      itemNumber: 2,
      rationale:
        'Forhåndsdefinert i protokollen.',
    },
  ],
  items: Array.from(
    { length: 16 },
    (_, index) => ({
      itemNumber: index + 1,
      response: 'Yes',
      rationale:
        'Dokumentert vurdering.',
      evidenceLocation:
        'Metodedelen',
      isWeakness: index === 0,
      isCriticalFlaw: false,
    }),
  ),
  finalConfidence: null,
  finalConfidenceRationale: null,
};

describe('FinalAssessment', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('summarises recorded judgements without inferring confidence', () => {
    render(
      <FinalAssessment
        assessment={assessment}
      />,
    );

    expect(
      screen.getByText('Registrerte svakheter')
        .parentElement,
    ).toHaveTextContent('1');

    expect(
      screen.getByLabelText(
        /forskerens samlede tillitsvurdering/i,
      ),
    ).toHaveValue('');
  });

  it('requires confidence and rationale before API validation', () => {
    render(
      <FinalAssessment
        assessment={assessment}
      />,
    );

    fireEvent.click(
      screen.getByRole('button', {
        name: /valider sluttvurdering/i,
      }),
    );

    expect(
      screen.getByText(
        /samlet tillitsvurdering må velges/i,
      ),
    ).toBeInTheDocument();

    expect(
      screen.getByText(
        /faglig begrunnelse.*obligatorisk/i,
      ),
    ).toBeInTheDocument();

    expect(
      validateAmstar2Assessment,
    ).not.toHaveBeenCalled();
  });

  it('sends the researcher judgement to the API', async () => {
    validateAmstar2Assessment.mockResolvedValue({
      isValid: true,
      errors: [],
    });

    render(
      <FinalAssessment
        assessment={assessment}
      />,
    );

    fireEvent.change(
      screen.getByLabelText(
        /forskerens samlede tillitsvurdering/i,
      ),
      {
        target: {
          value: 'Moderate',
        },
      },
    );

    fireEvent.change(
      screen.getByLabelText(
        /faglig begrunnelse for samlet tillit/i,
      ),
      {
        target: {
          value:
            'Samlet vurdering basert på dokumenterte svakheter.',
        },
      },
    );

    fireEvent.click(
      screen.getByRole('button', {
        name: /valider sluttvurdering/i,
      }),
    );

    await waitFor(() => {
      expect(
        validateAmstar2Assessment,
      ).toHaveBeenCalledWith(
        expect.objectContaining({
          finalConfidence: 'Moderate',
          finalConfidenceRationale:
            'Samlet vurdering basert på dokumenterte svakheter.',
        }),
      );
    });

    expect(
      await screen.findByText(
        /sluttvurderingen er validert/i,
      ),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('button', {
        name: /last ned Word-rapport/i,
      }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('button', {
        name: /last ned PDF/i,
      }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('button', {
        name: /last ned Excel/i,
      }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('button', {
        name: /last ned vurdering som JSON/i,
      }),
    ).toBeInTheDocument();
  });
});