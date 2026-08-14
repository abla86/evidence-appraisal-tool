import { useEffect, useMemo, useState } from 'react';
import {
  calculateAgree2,
  evaluateGrade,
  getInstruments,
  validateCasp,
} from '../api/researchApi';
import './ResearchModuleHub.css';

const emptyEvidence = (count, score = null) =>
  Array.from({ length: count }, (_, index) => ({
    itemNumber: index + 1,
    score,
    response: null,
    rationale: '',
    evidenceLocation: '',
  }));

const gradeDomains = [
  ['Risk of bias', -2, 0],
  ['Inconsistency', -2, 0],
  ['Indirectness', -2, 0],
  ['Imprecision', -2, 0],
  ['Publication bias', -1, 0],
  ['Large effect', 0, 2],
  ['Dose-response gradient', 0, 1],
  ['Plausible residual confounding', 0, 1],
];

function EvidenceFields({ items, setItems, mode }) {
  function update(number, field, value) {
    setItems((current) => current.map((item) =>
      item.itemNumber === number ? { ...item, [field]: value } : item));
  }

  return (
    <div className="research-items">
      {items.map((item) => (
        <fieldset className="research-item" key={item.itemNumber}>
          <legend>Punkt {item.itemNumber}</legend>
          {mode === 'agree2' ? (
            <label>Skår 1–7
              <select value={item.score ?? ''} onChange={(event) =>
                update(item.itemNumber, 'score', Number(event.target.value))}>
                <option value="">Velg</option>
                {[1, 2, 3, 4, 5, 6, 7].map((score) =>
                  <option key={score} value={score}>{score}</option>)}
              </select>
            </label>
          ) : (
            <label>Svar
              <select value={item.response ?? ''} onChange={(event) =>
                update(item.itemNumber, 'response', event.target.value || null)}>
                <option value="">Velg</option>
                <option value="Yes">Ja</option>
                <option value="No">Nei</option>
                <option value="CannotTell">Kan ikke avgjøres</option>
              </select>
            </label>
          )}
          <label>Dokumentasjonssted
            <input value={item.evidenceLocation} onChange={(event) =>
              update(item.itemNumber, 'evidenceLocation', event.target.value)}
              placeholder="Side, tabell eller vedlegg" />
          </label>
          <label>Faglig begrunnelse
            <textarea rows="2" value={item.rationale} onChange={(event) =>
              update(item.itemNumber, 'rationale', event.target.value)} />
          </label>
        </fieldset>
      ))}
    </div>
  );
}

function CaspForm() {
  const [count, setCount] = useState(10);
  const [items, setItems] = useState(() => emptyEvidence(10));
  const [result, setResult] = useState(null);
  const [meta, setMeta] = useState({
    checklistTitle: '', checklistVersion: '2024',
    officialChecklistUrl: 'https://casp-uk.net/casp-tools-checklists/',
    studyTitle: '', reviewerCode: '',
    overallJudgement: '', overallJudgementRationale: '',
  });

  function resize(next) {
    const value = Math.min(100, Math.max(1, Number(next)));
    setCount(value);
    setItems(emptyEvidence(value));
    setResult(null);
  }

  async function submit(event) {
    event.preventDefault();
    setResult(await validateCasp({
      ...meta, expectedItemCount: count, items,
      assessmentDateUtc: new Date().toISOString(),
    }));
  }

  return (
    <form className="research-form" onSubmit={submit}>
      <h3>CASP-vurdering</h3>
      <p className="module-warning">Bruk den autoriserte, studiespesifikke CASP-sjekklisten parallelt. Instrumentteksten gjengis ikke her.</p>
      <div className="research-grid">
        <label>Sjekklistens navn<input required value={meta.checklistTitle}
          onChange={(e) => setMeta({ ...meta, checklistTitle: e.target.value })} /></label>
        <label>Versjon<input required value={meta.checklistVersion}
          onChange={(e) => setMeta({ ...meta, checklistVersion: e.target.value })} /></label>
        <label>Offisiell lenke<input required type="url" value={meta.officialChecklistUrl}
          onChange={(e) => setMeta({ ...meta, officialChecklistUrl: e.target.value })} /></label>
        <label>Antall punkter<input required type="number" min="1" max="100" value={count}
          onChange={(e) => resize(e.target.value)} /></label>
        <label>Studietittel<input required value={meta.studyTitle}
          onChange={(e) => setMeta({ ...meta, studyTitle: e.target.value })} /></label>
        <label>Pseudonym vurdererkode<input required value={meta.reviewerCode}
          onChange={(e) => setMeta({ ...meta, reviewerCode: e.target.value })} /></label>
      </div>
      <EvidenceFields items={items} setItems={setItems} mode="casp" />
      <label>Samlet vurdering<input required value={meta.overallJudgement}
        onChange={(e) => setMeta({ ...meta, overallJudgement: e.target.value })} /></label>
      <label>Begrunnelse for samlet vurdering<textarea required rows="3"
        value={meta.overallJudgementRationale}
        onChange={(e) => setMeta({ ...meta, overallJudgementRationale: e.target.value })} /></label>
      <button className="primary-button">Valider CASP-vurdering</button>
      <Result result={result} />
    </form>
  );
}

function Agree2Form() {
  const [items, setItems] = useState(() => emptyEvidence(23, null));
  const [result, setResult] = useState(null);
  const [meta, setMeta] = useState({
    guidelineTitle: '', guidelineCitation: '', appraiserCode: '',
    overallQualityScore: '', recommendation: '', recommendationRationale: '',
  });

  async function submit(event) {
    event.preventDefault();
    setResult(await calculateAgree2({
      instrumentName: 'AGREE II', instrumentVersion: '2017',
      guidelineTitle: meta.guidelineTitle,
      guidelineCitation: meta.guidelineCitation,
      assessmentDateUtc: new Date().toISOString(),
      appraisers: [{
        appraiserCode: meta.appraiserCode,
        items: items.map(({ itemNumber, score, rationale, evidenceLocation }) =>
          ({ itemNumber, score, rationale, evidenceLocation })),
        overallQualityScore: Number(meta.overallQualityScore),
        recommendation: meta.recommendation,
        recommendationRationale: meta.recommendationRationale,
      }],
    }));
  }

  return (
    <form className="research-form" onSubmit={submit}>
      <h3>AGREE II-vurdering</h3>
      <p className="module-warning">Alle 23 punkter skåres 1–7. Domeneskår beregnes etter AGREE II-formelen; domenene skal ikke summeres til én automatisk totalskår.</p>
      <div className="research-grid">
        <label>Retningslinjetittel<input required value={meta.guidelineTitle}
          onChange={(e) => setMeta({ ...meta, guidelineTitle: e.target.value })} /></label>
        <label>Full referanse<input required value={meta.guidelineCitation}
          onChange={(e) => setMeta({ ...meta, guidelineCitation: e.target.value })} /></label>
        <label>Pseudonym vurdererkode<input required value={meta.appraiserCode}
          onChange={(e) => setMeta({ ...meta, appraiserCode: e.target.value })} /></label>
      </div>
      <EvidenceFields items={items} setItems={setItems} mode="agree2" />
      <div className="research-grid">
        <label>Samlet kvalitet 1–7<input required type="number" min="1" max="7"
          value={meta.overallQualityScore}
          onChange={(e) => setMeta({ ...meta, overallQualityScore: e.target.value })} /></label>
        <label>Anbefaling<input required value={meta.recommendation}
          placeholder="Ja, nei eller med endringer"
          onChange={(e) => setMeta({ ...meta, recommendation: e.target.value })} /></label>
      </div>
      <label>Begrunnelse for anbefaling<textarea required rows="3"
        value={meta.recommendationRationale}
        onChange={(e) => setMeta({ ...meta, recommendationRationale: e.target.value })} /></label>
      <button className="primary-button">Beregn AGREE II-domeneskår</button>
      {result?.isValid && (
        <div className="result-panel"><h4>Domeneskår</h4><ul>
          {result.domainScores.map((domain) => <li key={domain.domainNumber}>
            {domain.domainNumber}. {domain.domainName}: <strong>{domain.standardizedScorePercent} %</strong>
          </li>)}
        </ul>{!result.independentAppraisalMinimumMet &&
          <p>Foreløpig én vurderer. Legg til uavhengige vurderinger før forskningsmessig konsensus.</p>}</div>
      )}
      <Result result={result?.isValid ? null : result} />
    </form>
  );
}

function GradeForm() {
  const [result, setResult] = useState(null);
  const [meta, setMeta] = useState({
    outcomeName: '', importance: 'Critical', population: '',
    intervention: '', comparator: '', effectMeasure: '',
    relativeEffect: '', absoluteEffect: '', participants: 0, studies: 1,
    initialCertainty: 'High', initialCertaintyRationale: '',
    reviewerConfirmedCertainty: '', finalCertaintyRationale: '',
  });
  const [judgements, setJudgements] = useState(() => gradeDomains.map(
    ([domain]) => ({ domain, levelChange: 0, rationale: '', evidenceLocation: '' })));

  function updateDomain(domain, field, value) {
    setJudgements((current) => current.map((item) =>
      item.domain === domain ? { ...item, [field]: value } : item));
  }

  async function submit(event) {
    event.preventDefault();
    setResult(await evaluateGrade({
      ...meta,
      participants: Number(meta.participants),
      studies: Number(meta.studies),
      reviewerConfirmedCertainty: meta.reviewerConfirmedCertainty || null,
      domainJudgements: judgements,
    }));
  }

  return (
    <form className="research-form" onSubmit={submit}>
      <h3>GRADE – sikkerhet i dokumentasjonen per utfall</h3>
      <div className="research-grid">
        {['outcomeName', 'population', 'intervention', 'comparator', 'effectMeasure',
          'relativeEffect', 'absoluteEffect'].map((field) =>
          <label key={field}>{field}<input required value={meta[field]}
            onChange={(e) => setMeta({ ...meta, [field]: e.target.value })} /></label>)}
        <label>Betydning<select value={meta.importance}
          onChange={(e) => setMeta({ ...meta, importance: e.target.value })}>
          <option value="Critical">Kritisk</option><option value="Important">Viktig</option>
        </select></label>
        <label>Deltakere<input type="number" min="0" value={meta.participants}
          onChange={(e) => setMeta({ ...meta, participants: e.target.value })} /></label>
        <label>Studier<input type="number" min="1" value={meta.studies}
          onChange={(e) => setMeta({ ...meta, studies: e.target.value })} /></label>
        <label>Utgangsnivå<select value={meta.initialCertainty}
          onChange={(e) => setMeta({ ...meta, initialCertainty: e.target.value })}>
          {['High', 'Moderate', 'Low', 'VeryLow'].map((value) =>
            <option key={value} value={value}>{value}</option>)}
        </select></label>
      </div>
      <label>Begrunnelse for utgangsnivå<textarea required rows="3"
        value={meta.initialCertaintyRationale}
        onChange={(e) => setMeta({ ...meta, initialCertaintyRationale: e.target.value })} /></label>
      <div className="research-items">
        {gradeDomains.map(([domain, min, max]) => {
          const item = judgements.find((entry) => entry.domain === domain);
          return <fieldset className="research-item" key={domain}><legend>{domain}</legend>
            <label>Nivåendring<select value={item.levelChange}
              onChange={(e) => updateDomain(domain, 'levelChange', Number(e.target.value))}>
              {Array.from({ length: max - min + 1 }, (_, i) => min + i).map((value) =>
                <option key={value} value={value}>{value > 0 ? `+${value}` : value}</option>)}
            </select></label>
            <label>Dokumentasjonssted<input required value={item.evidenceLocation}
              onChange={(e) => updateDomain(domain, 'evidenceLocation', e.target.value)} /></label>
            <label>Begrunnelse<textarea required rows="2" value={item.rationale}
              onChange={(e) => updateDomain(domain, 'rationale', e.target.value)} /></label>
          </fieldset>;
        })}
      </div>
      <div className="research-grid">
        <label>Forskerbekreftet sikkerhet<select value={meta.reviewerConfirmedCertainty}
          onChange={(e) => setMeta({ ...meta, reviewerConfirmedCertainty: e.target.value })}>
          <option value="">Ikke bekreftet</option>
          {['High', 'Moderate', 'Low', 'VeryLow'].map((value) =>
            <option key={value} value={value}>{value}</option>)}
        </select></label>
      </div>
      <label>Endelig begrunnelse<textarea required rows="3"
        value={meta.finalCertaintyRationale}
        onChange={(e) => setMeta({ ...meta, finalCertaintyRationale: e.target.value })} /></label>
      <button className="primary-button">Vurder GRADE-utfall</button>
      {result?.isValid && <div className="result-panel">
        Foreløpig sikkerhet: <strong>{result.provisionalCertainty}</strong>.
        Netto nivåendring: {result.netLevelChange}.
      </div>}
      <Result result={result?.isValid ? null : result} />
    </form>
  );
}

function Result({ result }) {
  if (!result) return null;
  return <div className={result.isValid ? 'result-panel' : 'message message-error'} role="status">
    <strong>{result.isValid ? 'Datasettet er validert.' : 'Må korrigeres'}</strong>
    {result.errors?.length > 0 && <ul>{result.errors.map((error) =>
      <li key={error}>{error}</li>)}</ul>}
  </div>;
}

export default function ResearchModuleHub() {
  const [instruments, setInstruments] = useState([]);
  const [selected, setSelected] = useState('');

  useEffect(() => { getInstruments().then(setInstruments).catch(() => setInstruments([])); }, []);
  const current = useMemo(() => instruments.find((item) => item.id === selected), [instruments, selected]);

  return (
    <section className="research-hub">
      <p className="eyebrow">Forskningsmoduler</p>
      <h2>Velg vurderingsmetode</h2>
      <div className="instrument-picker">
        {instruments.filter((item) => item.id !== 'amstar2').map((item) =>
          <button type="button" className={selected === item.id ? 'instrument-option selected' : 'instrument-option'}
            key={item.id} onClick={() => setSelected(item.id)}>
            <strong>{item.name}</strong><span>{item.purpose}</span>
          </button>)}
      </div>
      {current && <p className="selected-purpose"><strong>{current.name}:</strong> {current.scoring}</p>}
      {selected === 'casp' && <CaspForm />}
      {selected === 'agree2' && <Agree2Form />}
      {selected === 'grade' && <GradeForm />}
    </section>
  );
}
