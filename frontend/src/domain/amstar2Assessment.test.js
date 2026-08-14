import {
  describe,
  expect,
  it,
} from 'vitest';
import {
  createAssessment,
  createEmptyAmstar2Items,
  validateItemDrafts,
} from './amstar2Assessment';

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

describe('AMSTAR 2 assessment domain', () => {
  it('creates exactly 16 empty item assessments', () => {
    const items =
      createEmptyAmstar2Items();

    expect(items).toHaveLength(16);
    expect(items[0].itemNumber).toBe(1);
    expect(items[15].itemNumber).toBe(16);

    expect(items.every(
      (item) =>
        item.response === null &&
        item.isWeakness === null &&
        item.isCriticalFlaw === null,
    )).toBe(true);
  });

  it('does not infer researcher judgements', () => {
    const items =
      createEmptyAmstar2Items();

    const errors = validateItemDrafts(
      items,
      setup.criticalDomains,
    );

    expect(Object.keys(errors)).toHaveLength(16);
    expect(errors[1]).toContain(
      'Svar mangler.',
    );
    expect(errors[1]).toContain(
      'Vurdering av metodisk svakhet mangler.',
    );
  });

  it('rejects a critical flaw outside prespecified domains', () => {
    const items =
      createEmptyAmstar2Items();

    items[0] = {
      itemNumber: 1,
      response: 'No',
      rationale: 'Metodisk krav ikke oppfylt.',
      evidenceLocation: 'Metodedelen, side 4',
      isWeakness: true,
      isCriticalFlaw: true,
    };

    const errors = validateItemDrafts(
      items,
      setup.criticalDomains,
    );

    expect(errors[1]).toContain(
      'Punktet var ikke forhåndsdefinert som et kritisk domene.',
    );
  });

  it('creates an API-compatible assessment without a score', () => {
    const items =
      createEmptyAmstar2Items().map(
        (item) => ({
          ...item,
          response: 'Yes',
          rationale:
            'Dokumentert forskervurdering.',
          evidenceLocation:
            'Metodedelen, relevant avsnitt',
          isWeakness: false,
          isCriticalFlaw: false,
        }),
      );

    const assessment = createAssessment(
      setup,
      items,
    );

    expect(assessment.items).toHaveLength(16);
    expect(assessment.reviewTitle)
      .toBe('Testoversikt');

    expect(assessment)
      .not.toHaveProperty('score');

    expect(assessment.finalConfidence)
      .toBeNull();
  });
});