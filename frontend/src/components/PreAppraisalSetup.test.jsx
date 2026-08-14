import {
  fireEvent,
  render,
  screen,
} from '@testing-library/react';
import {
  describe,
  expect,
  it,
} from 'vitest';
import PreAppraisalSetup from './PreAppraisalSetup';

const defaults = [2, 4, 7, 9, 11, 13, 15];

describe('PreAppraisalSetup', () => {
  it('requires identifying information and rationales', () => {
    render(
      <PreAppraisalSetup
        defaultCriticalDomains={defaults}
      />,
    );

    fireEvent.click(
      screen.getByRole('button', {
        name: /kontroller vurderingsoppsettet/i,
      }),
    );

    expect(
      screen.getByText(
        /tittel på den systematiske oversikten er obligatorisk/i,
      ),
    ).toBeInTheDocument();

    expect(
      screen.getByText(
        /navn eller identifikator for vurderer er obligatorisk/i,
      ),
    ).toBeInTheDocument();

    expect(
      screen.getByText(
        /begrunnelse for punkt 2 er obligatorisk/i,
      ),
    ).toBeInTheDocument();

    expect(
      screen.queryByText(/oppsettet er kontrollert/i),
    ).not.toBeInTheDocument();
  });

  it('confirms a completely documented setup', () => {
    render(
      <PreAppraisalSetup
        defaultCriticalDomains={[2]}
      />,
    );

    fireEvent.change(
      screen.getByLabelText(
        /tittel på systematisk oversikt/i,
      ),
      {
        target: {
          value: 'Eksempeloversikt',
        },
      },
    );

    fireEvent.change(
      screen.getByLabelText(/^vurderer/i),
      {
        target: {
          value: 'Forsker 01',
        },
      },
    );

    fireEvent.change(
      screen.getByLabelText(
        /forhåndsbegrunnelse for punkt 2/i,
      ),
      {
        target: {
          value:
            'Domenet er forhåndsdefinert i protokollen.',
        },
      },
    );

    fireEvent.click(
      screen.getByRole('button', {
        name: /kontroller vurderingsoppsettet/i,
      }),
    );

    expect(
      screen.getByText(/oppsettet er kontrollert/i),
    ).toBeInTheDocument();

    expect(
      screen.getByText(/1 kritiske domener/i),
    ).toBeInTheDocument();
  });
});