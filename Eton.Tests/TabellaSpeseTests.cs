using Eton.Models;
using Eton.Services;

namespace Eton.Tests;

public class TabellaSpeseTests
{
    private static readonly Guid Io = Guid.NewGuid();
    private static readonly Guid Altro = Guid.NewGuid();
    private static readonly Guid SpazioId = Guid.NewGuid();
    private static readonly DateTime Oggi = new(2026, 10, 7);

    private static Expense Spesa(decimal importo = 10m, string descrizione = "Pane", string categoria = "Spesa",
        int giorno = 5, Guid? pagante = null, Guid? regola = null) => new()
    {
        Id = Guid.NewGuid(),
        SpaceId = SpazioId,
        PaidBy = pagante ?? Io,
        Amount = importo,
        Description = descrizione,
        Category = categoria,
        SpentOn = new DateTime(2026, 9, giorno),
        Version = 3,
        RecurringId = regola,
        RecurringPeriod = regola is null ? null : new DateTime(2026, 9, 1)
    };

    private static RigaSpesa Reg(Expense s) => new(s, StatoSpesa.Registrata);

    private static (IReadOnlyList<RigaSpesa> Righe, Expense Prevista) TreStati()
    {
        var vera = Spesa();
        var prevista = Spesa(regola: Guid.NewGuid());
        var futura = Spesa(giorno: 28, regola: Guid.NewGuid());
        var periodo = new SpeseDelPeriodo([vera, prevista], new HashSet<Guid> { prevista.Id }, [futura]);
        return (TabellaSpese.Righe(periodo), prevista);
    }

    // --- righe e stati ---

    [Fact]
    public void Ogni_riga_porta_il_suo_stato()
    {
        var (righe, _) = TreStati();

        Assert.Equal([StatoSpesa.Registrata, StatoSpesa.Prevista, StatoSpesa.InArrivo], righe.Select(r => r.Stato));
    }

    [Fact]
    public void Le_previste_contano_nei_totali_e_le_in_arrivo_no()
    {
        var (righe, _) = TreStati();

        Assert.Equal([true, true, false], righe.Select(r => r.ContaNeiTotali));
        Assert.Equal(1, TabellaSpese.QuantePreviste(righe));
    }

    [Fact]
    public void La_chiave_di_una_prevista_non_cambia_fra_due_letture()
    {
        // CalcoliRicorrenti.Occorrenza dà un Guid nuovo a ogni fusione: la chiave non può essere l'Id.
        var regola = Guid.NewGuid();
        var primaLettura = new RigaSpesa(Spesa(regola: regola), StatoSpesa.Prevista);
        var secondaLettura = new RigaSpesa(Spesa(regola: regola), StatoSpesa.Prevista);

        Assert.NotEqual(primaLettura.Spesa.Id, secondaLettura.Spesa.Id);
        Assert.Equal(primaLettura.Chiave, secondaLettura.Chiave);
    }

    [Fact]
    public void La_chiave_di_una_registrata_e_il_suo_id()
    {
        var vera = Spesa(regola: Guid.NewGuid());   // anche se nata da una ricorrente

        Assert.Equal(vera.Id.ToString(), Reg(vera).Chiave);
    }

    // --- periodi ---

    [Fact]
    public void I_periodi_visti_dal_sette_ottobre()
    {
        Assert.Equal((new DateTime(2026, 10, 1), new DateTime(2026, 10, 31)), TabellaSpese.Intervallo(TipoPeriodo.QuestoMese, Oggi, null, null));
        Assert.Equal((new DateTime(2026, 9, 1), new DateTime(2026, 9, 30)), TabellaSpese.Intervallo(TipoPeriodo.MeseScorso, Oggi, null, null));
        Assert.Equal((new DateTime(2026, 8, 1), new DateTime(2026, 10, 31)), TabellaSpese.Intervallo(TipoPeriodo.UltimiTreMesi, Oggi, null, null));
        Assert.Equal((new DateTime(2026, 1, 1), new DateTime(2026, 12, 31)), TabellaSpese.Intervallo(TipoPeriodo.QuestAnno, Oggi, null, null));
        Assert.Equal((new DateTime(2026, 3, 2), new DateTime(2026, 4, 9)),
            TabellaSpese.Intervallo(TipoPeriodo.Libero, Oggi, new DateTime(2026, 3, 2), new DateTime(2026, 4, 9)));
    }

    [Fact]
    public void Il_mese_scorso_di_gennaio_e_dicembre_dell_anno_prima()
    {
        Assert.Equal((new DateTime(2025, 12, 1), new DateTime(2025, 12, 31)),
            TabellaSpese.Intervallo(TipoPeriodo.MeseScorso, new DateTime(2026, 1, 15), null, null));
    }

    [Fact]
    public void Un_periodo_libero_invertito_o_incompleto_torna_a_questo_mese()
    {
        var questoMese = (new DateTime(2026, 10, 1), new DateTime(2026, 10, 31));

        Assert.Equal(questoMese, TabellaSpese.Intervallo(TipoPeriodo.Libero, Oggi, new DateTime(2026, 9, 30), new DateTime(2026, 9, 1)));
        Assert.Equal(questoMese, TabellaSpese.Intervallo(TipoPeriodo.Libero, Oggi, null, new DateTime(2026, 9, 1)));
    }

    [Fact]
    public void Un_periodo_libero_fuori_dalla_guardia_delle_date_torna_a_questo_mese()
    {
        // Un link con a=9999-12-31 farebbe generare un'occorrenza al mese per ogni ricorrente senza fine.
        var questoMese = (new DateTime(2026, 10, 1), new DateTime(2026, 10, 31));

        Assert.Equal(questoMese, TabellaSpese.Intervallo(TipoPeriodo.Libero, Oggi, new DateTime(2026, 1, 1), new DateTime(9999, 12, 31)));
        Assert.Equal(questoMese, TabellaSpese.Intervallo(TipoPeriodo.Libero, Oggi, new DateTime(1999, 12, 31), new DateTime(2026, 9, 1)));
    }

    // --- filtri ---

    private static IReadOnlyList<RigaSpesa> TreRighe() =>
    [
        Reg(Spesa(descrizione: "Pane", categoria: "Spesa")),
        Reg(Spesa(descrizione: "Cena da Mario", categoria: "Ristoranti", pagante: Altro)),
        new RigaSpesa(Spesa(descrizione: "Caffè al bar", categoria: "Svago", regola: Guid.NewGuid()), StatoSpesa.Prevista),
    ];

    [Fact]
    public void I_filtri_si_combinano()
    {
        var filtri = FiltriSpese.Nessuno with { Categorie = new HashSet<string> { "Spesa", "Ristoranti" }, Testo = "MARIO" };

        var filtrate = TabellaSpese.Filtra(TreRighe(), filtri);

        Assert.Equal(["Cena da Mario"], filtrate.Select(r => r.Spesa.Description));
    }

    [Fact]
    public void Il_testo_si_cerca_senza_accenti_e_senza_maiuscole()
    {
        var filtrate = TabellaSpese.Filtra(TreRighe(), FiltriSpese.Nessuno with { Testo = "  caffe " });

        Assert.Equal(["Caffè al bar"], filtrate.Select(r => r.Spesa.Description));
    }

    [Fact]
    public void Pagante_e_stato_filtrano_ciascuno_per_conto_proprio()
    {
        Assert.Equal(["Cena da Mario"],
            TabellaSpese.Filtra(TreRighe(), FiltriSpese.Nessuno with { Pagante = Altro }).Select(r => r.Spesa.Description));
        Assert.Equal(["Caffè al bar"],
            TabellaSpese.Filtra(TreRighe(), FiltriSpese.Nessuno with { Stati = new HashSet<StatoSpesa> { StatoSpesa.Prevista } })
                .Select(r => r.Spesa.Description));
    }

    [Fact]
    public void Senza_filtri_passano_tutte()
    {
        Assert.Equal(3, TabellaSpese.Filtra(TreRighe(), FiltriSpese.Nessuno).Count);
    }

    // --- chi modifica ---

    [Fact]
    public void Modificabile_per_riga_pagante_proprietario_altro_membro_prevista()
    {
        Space[] spazi = [new() { Id = SpazioId, OwnerId = Altro }];   // lo spazio lo possiede Altro

        Assert.True(TabellaSpese.Modificabile(Reg(Spesa(pagante: Io)), Io, spazi));        // l'ho pagata io
        Assert.True(TabellaSpese.Modificabile(Reg(Spesa(pagante: Io)), Altro, spazi));     // possiede lo spazio
        Assert.False(TabellaSpese.Modificabile(Reg(Spesa(pagante: Altro)), Io, spazi));    // un altro membro
        Assert.False(TabellaSpese.Modificabile(new RigaSpesa(Spesa(pagante: Io), StatoSpesa.Prevista), Io, spazi));
        Assert.False(TabellaSpese.Modificabile(Reg(Spesa(pagante: Io)), null, spazi));     // fallisce chiuso
    }

    // --- la modifica di una cella ---

    [Fact]
    public void Un_importo_valido_cambia_solo_l_importo_e_tiene_la_versione()
    {
        var spesa = Spesa(importo: 10m);

        var esito = TabellaSpese.Applica(spesa, TabellaSpese.Colonne.Importo, "1284,5", Oggi);

        Assert.Null(esito.Errore);
        Assert.Equal(1284.5m, esito.Nuova!.Amount);
        Assert.Equal(spesa.Version, esito.Nuova.Version);
        Assert.Equal(spesa.Id, esito.Nuova.Id);
        Assert.Equal(spesa.Description, esito.Nuova.Description);
        Assert.Equal(spesa.SpentOn, esito.Nuova.SpentOn);
    }

    [Fact]
    public void Un_importo_non_valido_non_parte_e_dice_perche()
    {
        Assert.Equal(Testi.MessaggioImporto(EsitoImporto.NonNumerico),
            TabellaSpese.Applica(Spesa(), TabellaSpese.Colonne.Importo, "abc", Oggi).Errore);
        Assert.Equal(Testi.MessaggioImporto(EsitoImporto.TroppiDecimali),
            TabellaSpese.Applica(Spesa(), TabellaSpese.Colonne.Importo, "1,234", Oggi).Errore);
        // MessaggioImporto(Vuoto) è null di proposito (Services/Testi.cs:93-97): in una cella
        // svuotata e confermata invece è un errore, e serve una frase sua.
        Assert.Equal("Scrivi un importo.", TabellaSpese.Applica(Spesa(), TabellaSpese.Colonne.Importo, "", Oggi).Errore);
        Assert.Null(TabellaSpese.Applica(Spesa(), TabellaSpese.Colonne.Importo, "abc", Oggi).Nuova);
    }

    [Fact]
    public void La_descrizione_si_ripulisce_e_non_resta_vuota()
    {
        Assert.Equal("Latte", TabellaSpese.Applica(Spesa(), TabellaSpese.Colonne.Descrizione, "  Latte ", Oggi).Nuova!.Description);
        Assert.Equal("La descrizione non può restare vuota.",
            TabellaSpese.Applica(Spesa(), TabellaSpese.Colonne.Descrizione, "   ", Oggi).Errore);
    }

    [Fact]
    public void La_descrizione_non_supera_il_limite_del_database()
    {
        Assert.Equal("La descrizione può avere al massimo 200 caratteri.",
            TabellaSpese.Applica(Spesa(), TabellaSpese.Colonne.Descrizione, new string('a', 201), Oggi).Errore);
        Assert.Null(TabellaSpese.Applica(Spesa(), TabellaSpese.Colonne.Descrizione, new string('a', 200), Oggi).Errore);
    }

    [Fact]
    public void La_data_ha_la_stessa_guardia_del_registro()
    {
        Assert.Equal(new DateTime(2026, 9, 10),
            TabellaSpese.Applica(Spesa(), TabellaSpese.Colonne.Data, "2026-09-10", Oggi).Nuova!.SpentOn);
        Assert.Equal("L'anno non è verosimile: dev'essere fra il 2000 e l'anno prossimo.",
            TabellaSpese.Applica(Spesa(), TabellaSpese.Colonne.Data, "1999-12-31", Oggi).Errore);
        Assert.Equal("Scrivi una data valida.",
            TabellaSpese.Applica(Spesa(), TabellaSpese.Colonne.Data, "", Oggi).Errore);
    }

    [Fact]
    public void La_categoria_viene_dall_elenco_o_resta_quella_che_era()
    {
        Assert.Equal("Casa", TabellaSpese.Applica(Spesa(), TabellaSpese.Colonne.Categoria, "Casa", Oggi).Nuova!.Category);
        Assert.Equal("Scegli una categoria dall'elenco.",
            TabellaSpese.Applica(Spesa(), TabellaSpese.Colonne.Categoria, "Inventata", Oggi).Errore);
        // Una categoria uscita dall'elenco dopo che la spesa è stata segnata (v. Pages/SpesaEdit.razor:140):
        // riconfermarla non è un errore.
        Assert.Null(TabellaSpese.Applica(Spesa(categoria: "Vecchia"), TabellaSpese.Colonne.Categoria, "Vecchia", Oggi).Errore);
    }

    [Fact]
    public void La_modifica_non_tocca_la_riga_originale_e_rifiuta_le_colonne_di_sola_lettura()
    {
        var spesa = Spesa(importo: 10m);

        TabellaSpese.Applica(spesa, TabellaSpese.Colonne.Importo, "20", Oggi);

        Assert.Equal(10m, spesa.Amount);
        Assert.Equal("Questa colonna non si modifica.",
            TabellaSpese.Applica(spesa, TabellaSpese.Colonne.Pagante, "x", Oggi).Errore);
    }

    // --- le azioni di massa ---

    private static readonly Guid[] Cinque = [Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()];

    [Fact]
    public void Eliminazione_si_conta_su_presenti_prima_meno_presenti_dopo()
    {
        // la quinta non c'era già più; la quarta è sopravvissuta (RLS: non era tua)
        var prima = Cinque.Take(4).ToHashSet();
        var dopo = new HashSet<Guid> { Cinque[3] };

        Assert.Equal(new EsitoMassa(5, 3, 1, 1), EsitoMassa.DaEliminazione(Cinque, prima, dopo));
    }

    [Fact]
    public void Se_la_RLS_filtra_la_lettura_non_si_dice_tutte_eliminate()
    {
        // Prima e dopo vuote: con "richieste meno presenti dopo" sarebbero 5 eliminate, su righe ancora lì.
        var nessuna = new HashSet<Guid>();

        Assert.Equal(new EsitoMassa(5, 0, 0, 5), EsitoMassa.DaEliminazione(Cinque, nessuna, nessuna));
    }

    [Fact]
    public void Cambia_categoria_distingue_non_tue_da_sparite()
    {
        var toccate = Cinque.Take(3).ToHashSet();
        var ancoraPresenti = new HashSet<Guid> { Cinque[3] };

        Assert.Equal(new EsitoMassa(5, 3, 1, 1), EsitoMassa.DaModifica(Cinque, toccate, ancoraPresenti));
    }

    [Fact]
    public void Il_testo_dell_esito_quando_sono_passate_tutte()
    {
        Assert.Equal("15 spese modificate.", TabellaSpese.TestoEsitoMassa(AzioneMassa.CambiaCategoria, new EsitoMassa(15, 15, 0, 0)));
        Assert.Equal("1 spesa eliminata.", TabellaSpese.TestoEsitoMassa(AzioneMassa.Elimina, new EsitoMassa(1, 1, 0, 0)));
    }

    [Fact]
    public void Il_testo_dell_esito_quando_ne_passano_solo_alcune()
    {
        Assert.Equal("12 su 15 modificate: 3 non erano tue.",
            TabellaSpese.TestoEsitoMassa(AzioneMassa.CambiaCategoria, new EsitoMassa(15, 12, 3, 0)));
        Assert.Equal("3 su 5 eliminate: 1 non era tua, 1 non c'era più.",
            TabellaSpese.TestoEsitoMassa(AzioneMassa.Elimina, new EsitoMassa(5, 3, 1, 1)));
    }

    [Fact]
    public void Il_testo_dell_esito_quando_non_ne_passa_nessuna()
    {
        Assert.Equal("Nessuna eliminata: 2 non erano tue.",
            TabellaSpese.TestoEsitoMassa(AzioneMassa.Elimina, new EsitoMassa(2, 0, 2, 0)));
        Assert.Equal("Nessuna modificata: 5 non c'erano più.",
            TabellaSpese.TestoEsitoMassa(AzioneMassa.CambiaCategoria, new EsitoMassa(5, 0, 0, 5)));
    }

    // --- la vista nell'URL ---

    [Fact]
    public void La_vista_predefinita_non_scrive_niente_nell_url()
    {
        Assert.Equal("", VistaTabella.Predefinita.InQuery());

        var letta = VistaTabella.DaQuery("");
        Assert.Equal(TipoPeriodo.QuestoMese, letta.Periodo);
        Assert.Equal(TabellaSpese.Colonne.Data, letta.OrdinaPer);
        Assert.True(letta.Decrescente);
        Assert.Null(letta.RaggruppaPer);
    }

    [Fact]
    public void Una_vista_scritta_nell_url_si_rilegge_identica()
    {
        var vista = VistaTabella.Predefinita with
        {
            Periodo = TipoPeriodo.Libero, Da = new DateTime(2026, 3, 2), A = new DateTime(2026, 4, 9),
            Filtri = new FiltriSpese(new HashSet<string> { "Casa", "Svago" }, Altro, "bolletta",
                new HashSet<StatoSpesa> { StatoSpesa.Prevista, StatoSpesa.InArrivo }),
            OrdinaPer = TabellaSpese.Colonne.Importo, Decrescente = false, RaggruppaPer = TabellaSpese.Gruppi.Mese
        };

        var riletta = VistaTabella.DaQuery(vista.InQuery());

        Assert.Equal(vista.Periodo, riletta.Periodo);
        Assert.Equal(vista.Da, riletta.Da);
        Assert.Equal(vista.A, riletta.A);
        Assert.True(riletta.Filtri.Categorie.SetEquals(["Casa", "Svago"]));
        Assert.Equal(Altro, riletta.Filtri.Pagante);
        Assert.Equal("bolletta", riletta.Filtri.Testo);
        Assert.True(riletta.Filtri.Stati.SetEquals([StatoSpesa.Prevista, StatoSpesa.InArrivo]));
        Assert.Equal(vista.OrdinaPer, riletta.OrdinaPer);
        Assert.False(riletta.Decrescente);
        Assert.Equal(TabellaSpese.Gruppi.Mese, riletta.RaggruppaPer);
    }

    [Fact]
    public void Un_parametro_illeggibile_torna_al_default_senza_rompere_gli_altri()
    {
        var letta = VistaTabella.DaQuery("?periodo=boh&ord=password&dir=x&cat=Inesistente,Casa&pagante=xyz&grp=colore&stato=forse&testo=pane");

        Assert.Equal(TipoPeriodo.QuestoMese, letta.Periodo);
        Assert.Equal(TabellaSpese.Colonne.Data, letta.OrdinaPer);
        Assert.True(letta.Decrescente);
        Assert.True(letta.Filtri.Categorie.SetEquals(["Casa"]));
        Assert.Null(letta.Filtri.Pagante);
        Assert.Null(letta.RaggruppaPer);
        Assert.Empty(letta.Filtri.Stati);
        Assert.Equal("pane", letta.Filtri.Testo);

        // Un periodo libero con una data impossibile torna a questo mese, e le date si perdono.
        var libera = VistaTabella.DaQuery("?periodo=libero&da=2026-13-45&a=2026-09-30");
        Assert.Equal(TipoPeriodo.QuestoMese, libera.Periodo);
        Assert.Null(libera.Da);
        Assert.Null(libera.A);
    }

    [Fact]
    public void Il_testo_del_filtro_sopravvive_ai_caratteri_dell_url()
    {
        var vista = VistaTabella.Predefinita with { Filtri = FiltriSpese.Nessuno with { Testo = "caffè & co=1?" } };

        Assert.Equal("caffè & co=1?", VistaTabella.DaQuery(vista.InQuery()).Filtri.Testo);
    }
}
