import { useMemo, useState } from 'react';

export default function PreAppraisalSetup({
  defaultCriticalDomains,
}) {
  const availableItems = useMemo(
    () => Array.from({ length: 16 }, (_, index) => index + 1),
    [],
  );

  const [reviewTitle, setReviewTitle] = useState('');
  const [reviewer, setReviewer] = useState('');
  const [criticalDomains, setCriticalDomains] = useState(
    () => [...defaultCriticalDomains],
  );
  const [rationales, setRationales] = useState({});
  const [errors, setErrors] = useState({});
  const [confirmed, setConfirmed] = useState(false);

  function toggleCriticalDomain(itemNumber) {
    setConfirmed(false);

    setCriticalDomains((current) =>
      current.includes(itemNumber)
        ? current.filter((item) => item !== itemNumber)
        : [...current, itemNumber].sort((a, b) => a - b),
    );
  }

  function updateRationale(itemNumber, value) {
    setConfirmed(false);

    setRationales((current) => ({
      ...current,
      [itemNumber]: value,
    }));
  }

  function validate() {
    const nextErrors = {};

    if (!reviewTitle.trim()) {
      nextErrors.reviewTitle =
        'Tittel på den systematiske oversikten er obligatorisk.';
    }

    if (!reviewer.trim()) {
      nextErrors.reviewer =
        'Navn eller identifikator for vurderer er obligatorisk.';
    }

    if (criticalDomains.length === 0) {
      nextErrors.criticalDomains =
        'Minst ett kritisk domene må forhåndsdefineres.';
    }

    criticalDomains.forEach((itemNumber) => {
      if (!rationales[itemNumber]?.trim()) {
        nextErrors[`rationale-${itemNumber}`] =
          `Begrunnelse for punkt ${itemNumber} er obligatorisk.`;
      }
    });

    setErrors(nextErrors);
    return Object.keys(nextErrors).length === 0;
  }

  function handleSubmit(event) {
    event.preventDefault();
    setConfirmed(false);

    if (validate()) {
      setConfirmed(true);
    }
  }

  return (
    <section className="setup-card">
      <div className="setup-heading">
        <div>
          <p className="eyebrow">Før vurderingen starter</p>
          <h2>Forhåndsdefiner vurderingsoppsettet</h2>
        </div>

        <span className="local-only">
          Ikke lagret
        </span>
      </div>

      <p className="setup-introduction">
        Registrer hvilken systematisk oversikt som skal vurderes,
        hvem som utfører vurderingen, og hvilke AMSTAR 2-punkter
        som på forhånd skal behandles som kritiske domener.
      </p>

      <div className="notice-inline">
        Dette steget beregner ingen totalskår eller
        kvalitetskonklusjon. Opplysningene finnes bare i
        nettleseren mens siden er åpen.
      </div>

      <form onSubmit={handleSubmit} noValidate>
        <div className="field-grid">
          <div className="form-field">
            <label htmlFor="review-title">
              Tittel på systematisk oversikt
              <span aria-hidden="true"> *</span>
            </label>

            <input
              id="review-title"
              type="text"
              value={reviewTitle}
              onChange={(event) => {
                setReviewTitle(event.target.value);
                setConfirmed(false);
              }}
              aria-describedby={
                errors.reviewTitle
                  ? 'review-title-error'
                  : undefined
              }
              aria-invalid={Boolean(errors.reviewTitle)}
            />

            {errors.reviewTitle && (
              <p
                id="review-title-error"
                className="field-error"
                role="alert"
              >
                {errors.reviewTitle}
              </p>
            )}
          </div>

          <div className="form-field">
            <label htmlFor="reviewer">
              Vurderer
              <span aria-hidden="true"> *</span>
            </label>

            <input
              id="reviewer"
              type="text"
              value={reviewer}
              onChange={(event) => {
                setReviewer(event.target.value);
                setConfirmed(false);
              }}
              aria-describedby={
                errors.reviewer
                  ? 'reviewer-error'
                  : undefined
              }
              aria-invalid={Boolean(errors.reviewer)}
            />

            {errors.reviewer && (
              <p
                id="reviewer-error"
                className="field-error"
                role="alert"
              >
                {errors.reviewer}
              </p>
            )}
          </div>
        </div>

        <fieldset className="critical-fieldset">
          <legend>
            Forhåndsdefinerte kritiske domener
          </legend>

          <p className="fieldset-help">
            Standardforslaget er forhåndsvalgt. Endringer må
            gjøres før selve vurderingen og begrunnes eksplisitt.
          </p>

          {errors.criticalDomains && (
            <p className="field-error" role="alert">
              {errors.criticalDomains}
            </p>
          )}

          <div className="domain-list">
            {availableItems.map((itemNumber) => {
              const selected =
                criticalDomains.includes(itemNumber);

              return (
                <div
                  className={
                    selected
                      ? 'domain-row domain-row-selected'
                      : 'domain-row'
                  }
                  key={itemNumber}
                >
                  <label className="domain-choice">
                    <input
                      type="checkbox"
                      checked={selected}
                      onChange={() =>
                        toggleCriticalDomain(itemNumber)
                      }
                    />

                    <span>
                      Punkt {itemNumber}
                      {defaultCriticalDomains.includes(
                        itemNumber,
                      ) && (
                        <small>
                          Foreslått standarddomene
                        </small>
                      )}
                    </span>
                  </label>

                  {selected && (
                    <div className="rationale-field">
                      <label
                        htmlFor={`rationale-${itemNumber}`}
                      >
                        Forhåndsbegrunnelse for punkt{' '}
                        {itemNumber}
                        <span aria-hidden="true"> *</span>
                      </label>

                      <textarea
                        id={`rationale-${itemNumber}`}
                        rows="3"
                        value={
                          rationales[itemNumber] ?? ''
                        }
                        onChange={(event) =>
                          updateRationale(
                            itemNumber,
                            event.target.value,
                          )
                        }
                        aria-invalid={Boolean(
                          errors[
                            `rationale-${itemNumber}`
                          ],
                        )}
                      />

                      {errors[
                        `rationale-${itemNumber}`
                      ] && (
                        <p
                          className="field-error"
                          role="alert"
                        >
                          {
                            errors[
                              `rationale-${itemNumber}`
                            ]
                          }
                        </p>
                      )}
                    </div>
                  )}
                </div>
              );
            })}
          </div>
        </fieldset>

        <button className="primary-button" type="submit">
          Kontroller vurderingsoppsettet
        </button>
      </form>

      {confirmed && (
        <div
          className="confirmation"
          role="status"
          aria-live="polite"
        >
          <strong>Oppsettet er kontrollert.</strong>
          <span>
            {criticalDomains.length} kritiske domener er
            forhåndsdefinert og begrunnet. Ingen data er lagret.
          </span>
        </div>
      )}
    </section>
  );
}