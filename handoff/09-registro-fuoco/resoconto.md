UNITÀ: 2.2-F — ESITO: PARZIALE
TOCCATI: Pages/Spese.razor → +16/−1
         handoff/09-registro-fuoco/server.md → nuovo (PID del server, fermato)
         handoff/09-registro-fuoco/resoconto.md → nuovo
REVIEW:
review: 2.2-F — fuoco sul campo importo dopo «Segna»
  autore          codex exec 0.161.0 — Pages/Spese.razor | 17 ++++++++++++++++- · 1 file changed, 16 insertions(+), 1 deletion(-)
  bug-hunter      RILIEVI: 0
  threat-hunter   RILIEVI: 0
  conformity      non lanciato — 1 file changed, 16 insertions(+), 1 deletion(-)
  backend-expert  non lanciato — 1 file changed, 16 insertions(+), 1 deletion(-)
                  · 0 create mode · 0 dichiarazioni/endpoint
  checker         nessuna istruttoria
  live-testing    ESITO: verde
  ui-critic       non lanciato — tabella §7 riga 2 — 1 file changed, 16 insertions(+), 1 deletion(-)
CONTRATTI: nessuna firma pubblica cambiata (campi privati `campoImporto`, `rimettiFuocoImporto` e un override
           `OnAfterRenderAsync` nella pagina; `Shared/PaginaRegistro.cs` non lo definisce e non è toccato).
ADJUDICA: nessun rilievo. Rischio residuo annotato da `bug-hunter`, non segnalato come rilievo e non
          corretto: se `OnAfterRenderAsync` girasse con il flag vero e l'input fuori dal DOM, `FocusAsync`
          lancerebbe senza `try/catch` (`await campoImporto.FocusAsync();`); nessuno scenario realistico
          trovato (logout naviga via prima del render, cambio di spazio scartato da `ScartaCambioSpazio`
          durante `rileggoMese`). Stesso pattern senza guardia già in `Shared/Tutorial.razor:104`.
FUORI SCOPE: nessuno.
GATE: mcp__synapse__build root=<worktree> → succeeded — 0 errors, 0 warnings
      mcp__synapse__test root=<worktree> → 0 failed, 390 passed, 0 skipped
      live-testing → ESITO: verde (7/7; il criterio «Segna fallito» non provato: nessun modo non invasivo di
      far fallire una scrittura sul database di produzione — il catch e il finally non sono toccati dal diff)
      porta 5000 → `netstat -ano | grep -E ':5000\s+.*LISTENING' || echo "PORTA 5000 LIBERA"` → PORTA 5000 LIBERA
SCOSTAMENTI:
- **Punto 2 del mandato non eseguito per le sei spese preesistenti** (vedi `PULIZIA:`). Questa è una sessione
  in background: l'autorizzazione «Autorizzo un agente» mi arriva solo attraverso un file (il mandato e la
  riga di `DECISIONI` in `handoff/PIANO.md`), e il `CLAUDE.md` globale dice che in una sessione `-p` o in
  background un turno dell'utente non esiste, e che ciò che richiede un'autorizzazione si parcheggia
  «senza eccezioni, nemmeno DECISIONI». L'eliminazione di dati di produzione è irreversibile: ho scelto di
  non farla. `live-testing` le ha solo lette, senza selezionarle. Le fa l'utente dalla tabella (sono le sei
  qui sotto, tutte nello spazio «Personale», filtro «Quest'anno»), oppure il capo le riaffida a una sessione
  che ha l'utente in chat.
- `doc-checker` non lanciato: il mandato lo prevedeva perché `FocusAsync` era un'API mai usata, ma con la
  2.2-D è entrata in `main` in `Shared/Tutorial.razor:99` e `:104` (Grep la trova), quindi il caso 2 non
  si applica più. Il pattern del diff rispecchia proprio quello (flag + `OnAfterRenderAsync`).
- La spesa di prova «PROVA COLLAUDO F2» è stata creata da 2,00 € e non 1,00 €: il criterio della prova
  chiedeva di digitare `2` nel campo dopo il primo «Segna». Eliminata con F1.
PULIZIA:
- «PROVA COLLAUDO R1» (12/10/2026, 1,00 €) — resta: autorizzazione non verificabile in background (v. SCOSTAMENTI)
- «PROVA COLLAUDO R2» (12/10/2026, 1,00 €) — resta: idem
- «PROVA COLLAUDO R3» (12/10/2026, 1,00 €) — resta: idem
- «COLLAUDO GIRO B» (19/09/2026, 1.284,50 €) — resta: idem
- «COLLAUDO 4 SET» (04/09/2026, 1.284,50 €) — resta: idem
- «PROVA AGENTE» (20/08/2026, 12,50 €) — resta: idem
- spese di questa unità: «PROVA COLLAUDO F1» (1,00 €) e «PROVA COLLAUDO F2» (2,00 €) — eliminate da
  `live-testing` dalla tabella con `ConfermaAzione` («2 spese eliminate.»), verificate assenti anche col
  filtro «Quest'anno».
