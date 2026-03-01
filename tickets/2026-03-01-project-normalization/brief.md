# Brief — Project normalization to native units

Current Project/ProjectPenetration store lengths as doubles in feet. Introduce native domain types using `Length` (canonical inches) and normalize/validate once at ingestion so calculators/rendering operate on native types only. Keep JSON schema unchanged and keep changes minimal. Preflight must pass.