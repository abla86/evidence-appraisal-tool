import { useEffect, useState } from 'react';
import {
  getAmstar2Metadata,
  getHealth,
} from './api/amstarApi';
import './App.css';

function App() {
  const [metadata, setMetadata] = useState(null);
  const [apiStatus, setApiStatus] = useState('Checking');
  const [error, setError] = useState('');

  useEffect(() => {
    let active = true;

    async function loadApplicationData() {
      try {
        const [health, instrumentMetadata] =
          await Promise.all([
            getHealth(),
            getAmstar2Metadata(),
          ]);

        if (!active) {
          return;
        }

        setApiStatus(health.status);
        setMetadata(instrumentMetadata);
      } catch {
        if (!active) {
          return;
        }

        setApiStatus('Unavailable');
        setError(
          'Kunne ikke koble til API-et. Kontroller at backend kjører på port 5237.',
        );
      }
    }

    loadApplicationData();

    return () => {
      active = false;
    };
  }, []);

  return (
    <main className="app-shell">
      <header className="hero">
        <div>
          <p className="eyebrow">
            Forskningsprototype
          </p>

          <h1>Evidence Appraisal Tool</h1>

          <p className="hero-text">
            Transparent og etterprøvbar støtte for
            strukturert kritisk vurdering.
          </p>
        </div>

        <div
          className={`status status-${apiStatus.toLowerCase()}`}
          role="status"
          aria-live="polite"
        >
          <span aria-hidden="true" />
          API: {apiStatus}
        </div>
      </header>

      {error && (
        <section
          className="message message-error"
          role="alert"
        >
          <h2>Tilkoblingsfeil</h2>
          <p>{error}</p>
        </section>
      )}

      {!metadata && !error && (
        <section
          className="message"
          aria-live="polite"
        >
          <p>Laster metodeinformasjon …</p>
        </section>
      )}

      {metadata && (
        <>
          <section className="instrument-card">
            <div>
              <p className="eyebrow">
                Aktiv modul
              </p>

              <h2>
                {metadata.instrumentName}{' '}
                <span>
                  ({metadata.instrumentVersion})
                </span>
              </h2>

              <p>
                Instrumentet inneholder{' '}
                <strong>
                  {metadata.totalItems} punkter
                </strong>
                .
              </p>
            </div>

            <div className="critical-domains">
              <h3>
                Foreslåtte kritiske standarddomener
              </h3>

              <ul aria-label="Foreslåtte kritiske domener">
                {metadata.proposedDefaultCriticalDomains.map(
                  (item) => (
                    <li key={item}>
                      Punkt {item}
                    </li>
                  ),
                )}
              </ul>
            </div>
          </section>

          <section className="notice notice-warning">
            <h2>Metodisk avgrensning</h2>
            <p>{metadata.criticalDomainNotice}</p>
            <p>
              <strong>Viktig:</strong>{' '}
              {metadata.scoringNotice}
            </p>
          </section>

          <div className="capability-grid">
            <section className="capability-card">
              <h2>Tilgjengelig nå</h2>

              <ul>
                {metadata.currentCapabilities.map(
                  (capability) => (
                    <li key={capability}>
                      <span
                        className="icon icon-available"
                        aria-hidden="true"
                      >
                        ✓
                      </span>
                      {capability}
                    </li>
                  ),
                )}
              </ul>
            </section>

            <section className="capability-card">
              <h2>Ikke tilgjengelig ennå</h2>

              <ul>
                {metadata.unavailableCapabilities.map(
                  (capability) => (
                    <li key={capability}>
                      <span
                        className="icon icon-unavailable"
                        aria-hidden="true"
                      >
                        —
                      </span>
                      {capability}
                    </li>
                  ),
                )}
              </ul>
            </section>
          </div>

          <section className="notice">
            <h2>Forskningsmessig sikkerhet</h2>
            <p>
              Denne versjonen beregner ingen
              kvalitetskonklusjon, lagrer ingen
              forskningsdata og erstatter ikke
              forskerens metodiske vurdering.
            </p>
          </section>
        </>
      )}
    </main>
  );
}

export default App;