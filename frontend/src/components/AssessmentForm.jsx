import { useState } from 'react';
import FinalAssessment from './FinalAssessment';
import {
  createAssessment,
  createEmptyAmstar2Items,
  NO_META_ANALYSIS_ITEMS,
  validateItemDrafts,
} from '../domain/amstar2Assessment';
import {
  validateAmstar2Assessment,
} from '../api/amstarApi';

const RESPONSE_OPTIONS = [
  {
    value: 'Yes',
    label: 'Ja',
  },
  {
    value: 'PartialYes',
    label: 'Delvis ja',
  },
  {
    value: 'No',
    label: 'Nei',
  },
];

function toNullableBoolean(value) {
  if (value === 'true') {
    return true;
  }

  if (value === 'false') {
    return false;
  }

  return null;
}

export default function AssessmentForm({
  setup,
}) {
  const [items, setItems] = useState(
    createEmptyAmstar2Items,
  );
  const [clientErrors, setClientErrors] =
    useState({});
  const [serverErrors, setServerErrors] =
    useState([]);
  const [result, setResult] = useState(null);
  const [
    validatedAssessment,
    setValidatedAssessment,
  ] = useState(null);
  const [submitting, setSubmitting] =
    useState(false);

  const criticalNumbers = new Set(
    setup.criticalDomains.map(
      (domain) => domain.itemNumber,
    ),
  );

  function updateItem(
    itemNumber,
    field,
    value,
  ) {
    setResult(null);
    setValidatedAssessment(null);
    setServerErrors([]);

    setItems((current) =>
      current.map((item) =>
        item.itemNumber === itemNumber
          ? {
              ...item,
              [field]: value,
            }
          : item,
      ),
    );
  }

  async function handleSubmit(event) {
    event.preventDefault();
    setResult(null);
    setValidatedAssessment(null);
    setServerErrors([]);

    const nextErrors = validateItemDrafts(
      items,
      setup.criticalDomains,
    );

    setClientErrors(nextErrors);

    if (Object.keys(nextErrors).length > 0) {
      document
        .getElementById('assessment-errors')
        ?.focus();

      return;
    }

    const assessment = createAssessment(
      setup,
      items,
    );

    setSubmitting(true);

    try {
      const validation =
        await validateAmstar2Assessment(
          assessment,
        );

      if (validation.isValid) {
        setResult(validation);
        setValidatedAssessment(assessment);
      } else {
        setServerErrors(
          validation.errors ?? [
            'API-et avviste vurderingen.',
          ],
        );
      }
    } catch {
      setServerErrors([
        'Kunne ikke validere vurderingen mot API-et. Kontroller at backend kjører.',
      ]);
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <section className="assessment-card">
      <div className="assessment-heading">
        <div>
          <p className="eyebrow">
            AMSTAR 2-vurdering
          </p>

          <h2>{setup.reviewTitle}</h2>

          <p>
            Vurderer: <strong>{setup.reviewer}</strong>
          </p>
        </div>

        <span className="local-only">
          Ikke lagret
        </span>
      </div>

      <div className="notice-inline">
        Bruk det autoriserte AMSTAR 2-instrumentet
        og tilhørende veiledning når hvert punkt
        vurderes. Denne prototypen gjengir ikke
        instrumentteksten og foretar ingen
        automatisk faglig vurdering.
      </div>

      {Object.keys(clientErrors).length > 0 && (
        <div
          id="assessment-errors"
          className="message message-error"
          role="alert"
          tabIndex="-1"
        >
          <h3>Vurderingen er ikke komplett</h3>
          <p>
            {Object.keys(clientErrors).length} av
            16 punkter har manglende eller
            inkonsistente opplysninger.
          </p>
        </div>
      )}

      {serverErrors.length > 0 && (
        <div
          className="message message-error"
          role="alert"
        >
          <h3>API-valideringen avviste dataene</h3>

          <ul>
            {serverErrors.map((error) => (
              <li key={error}>{error}</li>
            ))}
          </ul>
        </div>
      )}

      <form onSubmit={handleSubmit} noValidate>
        <div className="assessment-items">
          {items.map((item) => {
            const itemErrors =
              clientErrors[item.itemNumber] ?? [];

            const responseOptions = [
              ...RESPONSE_OPTIONS,
              ...(NO_META_ANALYSIS_ITEMS.has(
                item.itemNumber,
              )
                ? [
                    {
                      value:
                        'NoMetaAnalysisConducted',
                      label:
                        'Ingen metaanalyse gjennomført',
                    },
                  ]
                : []),
            ];

            return (
              <fieldset
                className={
                  itemErrors.length > 0
                    ? 'item-card item-card-error'
                    : 'item-card'
                }
                key={item.itemNumber}
                aria-label={
                  `Vurdering av punkt ${item.itemNumber}`
                }
              >
                <legend>
                  Punkt {item.itemNumber}

                  {criticalNumbers.has(
                    item.itemNumber,
                  ) && (
                    <span className="critical-badge">
                      Forhåndsdefinert kritisk
                    </span>
                  )}
                </legend>

                <p className="item-guidance">
                  Dokumenter vurderingen mot punkt{' '}
                  {item.itemNumber} i det autoriserte
                  AMSTAR 2-instrumentet.
                </p>

                <div className="item-field-grid">
                  <div className="form-field">
                    <label
                      htmlFor={
                        `response-${item.itemNumber}`
                      }
                    >
                      Svar
                    </label>

                    <select
                      id={
                        `response-${item.itemNumber}`
                      }
                      value={item.response ?? ''}
                      onChange={(event) =>
                        updateItem(
                          item.itemNumber,
                          'response',
                          event.target.value ||
                            null,
                        )
                      }
                    >
                      <option value="">
                        Velg svar
                      </option>

                      {responseOptions.map(
                        (option) => (
                          <option
                            key={option.value}
                            value={option.value}
                          >
                            {option.label}
                          </option>
                        ),
                      )}
                    </select>
                  </div>

                  <div className="form-field">
                    <label
                      htmlFor={
                        `evidence-${item.itemNumber}`
                      }
                    >
                      Dokumentasjonssted
                    </label>

                    <input
                      id={
                        `evidence-${item.itemNumber}`
                      }
                      type="text"
                      value={item.evidenceLocation}
                      placeholder={
                        'For eksempel side, tabell eller vedlegg'
                      }
                      onChange={(event) =>
                        updateItem(
                          item.itemNumber,
                          'evidenceLocation',
                          event.target.value,
                        )
                      }
                    />
                  </div>
                </div>

                <div className="form-field">
                  <label
                    htmlFor={
                      `rationale-${item.itemNumber}`
                    }
                  >
                    Faglig begrunnelse
                  </label>

                  <textarea
                    id={
                      `rationale-${item.itemNumber}`
                    }
                    rows="3"
                    value={item.rationale}
                    onChange={(event) =>
                      updateItem(
                        item.itemNumber,
                        'rationale',
                        event.target.value,
                      )
                    }
                  />
                </div>

                <div className="item-field-grid judgement-grid">
                  <div className="form-field">
                    <label
                      htmlFor={
                        `weakness-${item.itemNumber}`
                      }
                    >
                      Metodisk svakhet?
                    </label>

                    <select
                      id={
                        `weakness-${item.itemNumber}`
                      }
                      value={
                        item.isWeakness === null
                          ? ''
                          : String(item.isWeakness)
                      }
                      onChange={(event) =>
                        updateItem(
                          item.itemNumber,
                          'isWeakness',
                          toNullableBoolean(
                            event.target.value,
                          ),
                        )
                      }
                    >
                      <option value="">
                        Ikke vurdert
                      </option>
                      <option value="false">
                        Nei
                      </option>
                      <option value="true">
                        Ja
                      </option>
                    </select>
                  </div>

                  <div className="form-field">
                    <label
                      htmlFor={
                        `critical-${item.itemNumber}`
                      }
                    >
                      Kritisk svakhet?
                    </label>

                    <select
                      id={
                        `critical-${item.itemNumber}`
                      }
                      value={
                        item.isCriticalFlaw === null
                          ? ''
                          : String(
                              item.isCriticalFlaw,
                            )
                      }
                      onChange={(event) =>
                        updateItem(
                          item.itemNumber,
                          'isCriticalFlaw',
                          toNullableBoolean(
                            event.target.value,
                          ),
                        )
                      }
                    >
                      <option value="">
                        Ikke vurdert
                      </option>
                      <option value="false">
                        Nei
                      </option>
                      <option value="true">
                        Ja
                      </option>
                    </select>
                  </div>
                </div>

                {itemErrors.length > 0 && (
                  <div
                    className="item-errors"
                    role="alert"
                  >
                    <strong>
                      Punktet må korrigeres:
                    </strong>

                    <ul>
                      {itemErrors.map((error) => (
                        <li key={error}>
                          {error}
                        </li>
                      ))}
                    </ul>
                  </div>
                )}
              </fieldset>
            );
          })}
        </div>

        <button
          className="primary-button assessment-submit"
          type="submit"
          disabled={submitting}
        >
          {submitting
            ? 'Validerer …'
            : 'Valider komplett vurdering'}
        </button>
      </form>

      {result?.isValid && (
        <div
          className="confirmation"
          role="status"
          aria-live="polite"
        >
          <strong>
            Vurderingsdatasettet er komplett.
          </strong>

          <span>
            API-et har validert strukturen og de
            obligatoriske dokumentasjonsfeltene.
            Dette er ikke en automatisk
            kvalitetskonklusjon.
          </span>
        </div>
      )}

      {validatedAssessment && (
        <FinalAssessment
          assessment={validatedAssessment}
        />
      )}
    </section>
  );
}