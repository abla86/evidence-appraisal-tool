import { useMemo, useState } from 'react';
import {
  exportAmstar2Assessment,
  validateAmstar2Assessment,
} from '../api/amstarApi';

const CONFIDENCE_OPTIONS = [
  {
    value: 'High',
    label: 'Høy tillit',
  },
  {
    value: 'Moderate',
    label: 'Moderat tillit',
  },
  {
    value: 'Low',
    label: 'Lav tillit',
  },
  {
    value: 'CriticallyLow',
    label: 'Kritisk lav tillit',
  },
];

function safeFileName(value) {
  const normalized = value
    .trim()
    .toLowerCase()
    .replace(/[^a-z0-9æøå]+/gi, '-')
    .replace(/^-|-$/g, '');

  return normalized || 'amstar2-vurdering';
}

export default function FinalAssessment({
  assessment,
}) {
  const [confidence, setConfidence] =
    useState('');
  const [rationale, setRationale] =
    useState('');
  const [errors, setErrors] = useState([]);
  const [validatedAssessment, setValidatedAssessment] =
    useState(null);
  const [submitting, setSubmitting] =
    useState(false);
  const [exporting, setExporting] =
    useState('');
  const [exportError, setExportError] =
    useState('');

  const summary = useMemo(() => {
    const weaknesses = assessment.items
      .filter((item) => item.isWeakness)
      .map((item) => item.itemNumber);

    const criticalFlaws = assessment.items
      .filter((item) => item.isCriticalFlaw)
      .map((item) => item.itemNumber);

    return {
      weaknesses,
      criticalFlaws,
    };
  }, [assessment]);

  function updateConfidence(value) {
    setConfidence(value);
    setValidatedAssessment(null);
    setErrors([]);
  }

  function updateRationale(value) {
    setRationale(value);
    setValidatedAssessment(null);
    setErrors([]);
  }

  async function handleValidation(event) {
    event.preventDefault();

    const nextErrors = [];

    if (!confidence) {
      nextErrors.push(
        'Samlet tillitsvurdering må velges av forskeren.',
      );
    }

    if (!rationale.trim()) {
      nextErrors.push(
        'Faglig begrunnelse for samlet tillit er obligatorisk.',
      );
    }

    if (nextErrors.length > 0) {
      setErrors(nextErrors);
      setValidatedAssessment(null);
      return;
    }

    const completedAssessment = {
      ...assessment,
      finalConfidence: confidence,
      finalConfidenceRationale:
        rationale.trim(),
    };

    setSubmitting(true);
    setErrors([]);

    try {
      const validation =
        await validateAmstar2Assessment(
          completedAssessment,
        );

      if (!validation.isValid) {
        setErrors(
          validation.errors ?? [
            'API-et avviste sluttvurderingen.',
          ],
        );
        setValidatedAssessment(null);
        return;
      }

      setValidatedAssessment(
        completedAssessment,
      );
    } catch {
      setErrors([
        'Kunne ikke validere sluttvurderingen mot API-et.',
      ]);
      setValidatedAssessment(null);
    } finally {
      setSubmitting(false);
    }
  }

  async function exportDocument(
    format,
    extension,
  ) {
    if (!validatedAssessment) {
      return;
    }

    setExporting(format);
    setExportError('');

    try {
      const blob =
        await exportAmstar2Assessment(
          validatedAssessment,
          format,
        );

      downloadBlob(blob, extension);
    } catch {
      setExportError(
        'Rapporten kunne ikke lastes ned. Kontroller at API-et kjører og prøv igjen.',
      );
    } finally {
      setExporting('');
    }
  }

  function downloadBlob(blob, extension) {
    const url = URL.createObjectURL(blob);
    const anchor = document.createElement('a');

    anchor.href = url;
    anchor.download =
      `${safeFileName(
        validatedAssessment.reviewTitle,
      )}-amstar2.${extension}`;

    document.body.appendChild(anchor);
    anchor.click();
    anchor.remove();
    URL.revokeObjectURL(url);
  }

  function exportJson() {
    if (!validatedAssessment) {
      return;
    }

    const content = JSON.stringify(
      validatedAssessment,
      null,
      2,
    );

    const blob = new Blob(
      [content],
      {
        type: 'application/json;charset=utf-8',
      },
    );

    const url = URL.createObjectURL(blob);
    const anchor = document.createElement('a');

    anchor.href = url;
    anchor.download =
      `${safeFileName(
        validatedAssessment.reviewTitle,
      )}-amstar2.json`;

    document.body.appendChild(anchor);
    anchor.click();
    anchor.remove();
    URL.revokeObjectURL(url);
  }

  return (
    <section className="final-card">
      <p className="eyebrow">
        Forskerens sluttvurdering
      </p>

      <h2>Samlet tillit og eksport</h2>

      <div className="summary-grid">
        <div className="summary-box">
          <span>Registrerte svakheter</span>
          <strong>
            {summary.weaknesses.length}
          </strong>

          <small>
            {summary.weaknesses.length > 0
              ? `Punkt ${summary.weaknesses.join(', ')}`
              : 'Ingen registrert'}
          </small>
        </div>

        <div className="summary-box summary-critical">
          <span>Registrerte kritiske svakheter</span>
          <strong>
            {summary.criticalFlaws.length}
          </strong>

          <small>
            {summary.criticalFlaws.length > 0
              ? `Punkt ${summary.criticalFlaws.join(', ')}`
              : 'Ingen registrert'}
          </small>
        </div>
      </div>

      <div className="notice-inline">
        Oppsummeringen over beskriver bare
        forskerens registreringer. Verktøyet velger
        ikke samlet tillitsnivå og erstatter ikke
        metodisk vurdering etter AMSTAR 2.
      </div>

      {errors.length > 0 && (
        <div
          className="message message-error"
          role="alert"
        >
          <h3>Sluttvurderingen må korrigeres</h3>

          <ul>
            {errors.map((error) => (
              <li key={error}>{error}</li>
            ))}
          </ul>
        </div>
      )}

      <form onSubmit={handleValidation} noValidate>
        <div className="form-field">
          <label htmlFor="final-confidence">
            Forskerens samlede tillitsvurdering
          </label>

          <select
            id="final-confidence"
            value={confidence}
            onChange={(event) =>
              updateConfidence(
                event.target.value,
              )
            }
          >
            <option value="">
              Velg tillitsnivå
            </option>

            {CONFIDENCE_OPTIONS.map(
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

        <div className="form-field final-rationale">
          <label htmlFor="final-rationale">
            Faglig begrunnelse for samlet tillit
          </label>

          <textarea
            id="final-rationale"
            rows="6"
            value={rationale}
            onChange={(event) =>
              updateRationale(
                event.target.value,
              )
            }
            placeholder="Begrunn konklusjonen med utgangspunkt i de registrerte kritiske og ikke-kritiske svakhetene."
          />
        </div>

        <button
          className="primary-button"
          type="submit"
          disabled={submitting}
        >
          {submitting
            ? 'Validerer sluttvurdering …'
            : 'Valider sluttvurdering'}
        </button>
      </form>

      {validatedAssessment && (
        <section
          className="confirmation export-section"
          aria-labelledby="export-heading"
        >
          <div>
            <strong id="export-heading">
              Sluttvurderingen er validert
            </strong>

            <span>
              Velg ønsket rapportformat. Alle
              rapportene bygger på den samme
              validerte vurderingen.
            </span>
          </div>

          {exportError && (
            <p
              className="field-error"
              role="alert"
            >
              {exportError}
            </p>
          )}

          <div className="export-actions">
            <button
              className="primary-button export-choice"
              type="button"
              disabled={Boolean(exporting)}
              onClick={() =>
                exportDocument('word', 'docx')
              }
            >
              <strong>
                {exporting === 'word'
                  ? 'Lager Word-rapport …'
                  : 'Last ned Word-rapport'}
              </strong>
              <span>
                Anbefalt for videre akademisk arbeid
              </span>
            </button>

            <button
              className="secondary-button export-choice"
              type="button"
              disabled={Boolean(exporting)}
              onClick={() =>
                exportDocument('pdf', 'pdf')
              }
            >
              <strong>
                {exporting === 'pdf'
                  ? 'Lager PDF …'
                  : 'Last ned PDF'}
              </strong>
              <span>
                Ferdig rapport for deling og arkivering
              </span>
            </button>

            <button
              className="secondary-button export-choice"
              type="button"
              disabled={Boolean(exporting)}
              onClick={() =>
                exportDocument('excel', 'xlsx')
              }
            >
              <strong>
                {exporting === 'excel'
                  ? 'Lager Excel-fil …'
                  : 'Last ned Excel'}
              </strong>
              <span>
                Strukturert oversikt over alle 16 punkter
              </span>
            </button>
          </div>

          <details className="additional-export">
            <summary>Flere eksportvalg</summary>

            <button
              className="text-button"
              type="button"
              onClick={exportJson}
            >
              Last ned vurdering som JSON
            </button>
          </details>
        </section>
      )}
    </section>
  );
}