export const AMSTAR2_ITEM_NUMBERS =
  Array.from(
    { length: 16 },
    (_, index) => index + 1,
  );

export const NO_META_ANALYSIS_ITEMS =
  new Set([11, 12, 15]);

export function createEmptyAmstar2Items() {
  return AMSTAR2_ITEM_NUMBERS.map(
    (itemNumber) => ({
      itemNumber,
      response: null,
      rationale: '',
      evidenceLocation: '',
      isWeakness: null,
      isCriticalFlaw: null,
    }),
  );
}

export function createAssessment(
  setup,
  items,
) {
  return {
    instrumentName: 'AMSTAR 2',
    instrumentVersion: '2017',
    reviewTitle: setup.reviewTitle,
    reviewer: setup.reviewer,
    assessmentDateUtc:
      new Date().toISOString(),
    criticalDomains:
      setup.criticalDomains.map(
        (domain) => ({
          itemNumber: domain.itemNumber,
          rationale: domain.rationale,
        }),
      ),
    items: items.map((item) => ({
      itemNumber: item.itemNumber,
      response: item.response,
      rationale: item.rationale.trim(),
      evidenceLocation:
        item.evidenceLocation.trim(),
      isWeakness: item.isWeakness,
      isCriticalFlaw:
        item.isCriticalFlaw,
    })),
    finalConfidence: null,
    finalConfidenceRationale: null,
  };
}

export function validateItemDrafts(
  items,
  criticalDomains,
) {
  const errors = {};
  const criticalNumbers = new Set(
    criticalDomains.map(
      (domain) => domain.itemNumber,
    ),
  );

  items.forEach((item) => {
    const itemErrors = [];

    if (!item.response) {
      itemErrors.push('Svar mangler.');
    }

    if (!item.rationale.trim()) {
      itemErrors.push('Begrunnelse mangler.');
    }

    if (!item.evidenceLocation.trim()) {
      itemErrors.push(
        'Dokumentasjonssted eller eksplisitt opplysning om manglende rapportering mangler.',
      );
    }

    if (item.isWeakness === null) {
      itemErrors.push(
        'Vurdering av metodisk svakhet mangler.',
      );
    }

    if (item.isCriticalFlaw === null) {
      itemErrors.push(
        'Vurdering av kritisk svakhet mangler.',
      );
    }

    if (
      item.response ===
        'NoMetaAnalysisConducted' &&
      !NO_META_ANALYSIS_ITEMS.has(
        item.itemNumber,
      )
    ) {
      itemErrors.push(
        'Svaret «ingen metaanalyse gjennomført» er ikke gyldig for dette punktet.',
      );
    }

    if (
      item.isCriticalFlaw === true &&
      !criticalNumbers.has(item.itemNumber)
    ) {
      itemErrors.push(
        'Punktet var ikke forhåndsdefinert som et kritisk domene.',
      );
    }

    if (
      item.isCriticalFlaw === true &&
      item.isWeakness !== true
    ) {
      itemErrors.push(
        'En kritisk svakhet må også registreres som en metodisk svakhet.',
      );
    }

    if (itemErrors.length > 0) {
      errors[item.itemNumber] = itemErrors;
    }
  });

  return errors;
}