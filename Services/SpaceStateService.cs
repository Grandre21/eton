using Microsoft.JSInterop;
using Eton.Models;

namespace Eton.Services;

/// <summary>
/// Lo spazio su cui l'utente sta lavorando. Persistito in localStorage perché è un contesto, non
/// una navigazione: chi ricarica la pagina si aspetta di ritrovarsi dov'era.
/// <para>
/// La scelta salvata non è mai creduta sulla parola: alla ricarica si verifica che quello spazio
/// sia ancora fra i propri. Uno spazio da cui si è usciti, o che il proprietario ha cancellato,
/// resterebbe altrimenti selezionato all'infinito, e ogni pagina mostrerebbe il vuoto senza
/// spiegare perché. In quel caso si ripiega sullo spazio personale, che esiste sempre.
/// </para>
/// </summary>
public class SpaceStateService
{
    private const string ChiaveArchivio = "eton.spazio";

    private readonly SpaceRepository _spazi;
    private readonly IJSInProcessRuntime _js;
    private readonly SemaphoreSlim _cancello = new(1, 1);
    private IReadOnlyList<Space> _elenco = [];
    private bool _caricato;
    private int _generazione;

    public SpaceStateService(SpaceRepository spazi, IJSRuntime js)
    {
        _spazi = spazi;
        _js = (IJSInProcessRuntime)js;
    }

    /// <summary>Lo spazio attivo, o null se non è ancora stato caricato nulla.</summary>
    public Space? Attivo { get; private set; }

    /// <summary>Scatta quando cambia lo spazio attivo o l'elenco. I componenti che mostrano il
    /// contesto ci si agganciano per ridisegnarsi.</summary>
    public event Action? Cambiato;

    /// <summary>Carica una sola volta; le chiamate successive non toccano la rete.
    /// Ogni pagina può chiamarlo senza sapere chi è arrivato prima.</summary>
    public async Task<Space?> AssicuraCaricatoAsync()
    {
        if (_caricato) return Attivo;

        await _cancello.WaitAsync();
        try
        {
            // Ricontrollato DENTRO il cancello: chi ha aspettato qui mentre un altro caricava
            // troverebbe il lavoro già fatto, e rifarlo sarebbe una seconda query identica.
            // Stessa forma del doppio controllo di _initialized in SupabaseService.GetClientAsync.
            if (!_caricato) await CaricaAsync();
        }
        finally
        {
            _cancello.Release();
        }

        return Attivo;
    }

    /// <summary>Rilegge l'elenco dal database e riconvalida la scelta salvata. A differenza di
    /// <see cref="AssicuraCaricatoAsync"/> interroga sempre: la chiama chi ha appena cambiato
    /// qualcosa e non può accontentarsi di quello che c'era prima.</summary>
    public async Task RicaricaAsync()
    {
        await _cancello.WaitAsync();
        try
        {
            await CaricaAsync();
        }
        finally
        {
            _cancello.Release();
        }
    }

    private async Task CaricaAsync()
    {
        var mia = _generazione;

        IReadOnlyList<Space> elenco;
        try
        {
            elenco = await _spazi.ElencaAsync();
        }
        catch
        {
            // Una lettura fallita lascia in mano l'elenco precedente. Rimettere _caricato a false
            // costringe il prossimo AssicuraCaricatoAsync a riprovare invece di servire per sempre
            // una cache stantia: senza, uno spazio appena creato resterebbe invisibile nel
            // selettore della Home e nell'elenco fino a un ricaricamento completo della pagina.
            // L'elenco vecchio si tiene comunque — mostrarne uno vuoto sarebbe più falso di
            // mostrarne uno superato.
            if (mia == _generazione) _caricato = false;
            throw;
        }

        // Il controllo di generazione non è pignoleria: `await` è un punto di sospensione, e
        // Dimentica() è sincrona, quindi un logout può infilarsi esattamente qui. Senza questa
        // riga la risposta arrivata in ritardo riscriverebbe _elenco, _caricato e localStorage
        // con i dati dell'utente appena uscito, rimettendo in piedi lo stato appena azzerato.
        // Oggi nessun accesso avviene senza un ricaricamento completo della pagina (si passa
        // sempre da Google), quindi il danno non è visibile; lo diventerebbe il giorno in cui si
        // aggiungesse un accesso che non esce dall'applicazione.
        if (mia != _generazione) return;

        _elenco = elenco;
        _caricato = true;

        var salvato = Leggi();
        Attivo = _elenco.FirstOrDefault(s => s.Id == salvato)
                 ?? _elenco.FirstOrDefault(s => s.IsPersonal)
                 ?? _elenco.FirstOrDefault();

        if (Attivo is not null && Attivo.Id != salvato) Salva(Attivo.Id);
        Cambiato?.Invoke();
    }

    /// <summary>Gli spazi dell'utente. Vuoto finché <see cref="AssicuraCaricatoAsync"/> non è stato chiamato.</summary>
    public IReadOnlyList<Space> Elenco => _elenco;

    public void Imposta(Guid spazioId)
    {
        var scelto = _elenco.FirstOrDefault(s => s.Id == spazioId);
        if (scelto is null || scelto.Id == Attivo?.Id) return;

        Attivo = scelto;
        Salva(scelto.Id);
        Cambiato?.Invoke();
    }

    /// <summary>Da chiamare al logout: lo spazio di una persona non deve restare selezionato per
    /// la successiva che usa lo stesso dispositivo.</summary>
    public void Dimentica()
    {
        // Invalida i caricamenti già in volo: v. il controllo in CaricaAsync.
        _generazione++;
        _elenco = [];
        _caricato = false;
        Attivo = null;
        try
        {
            _js.InvokeVoid("localStorage.removeItem", ChiaveArchivio);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[Spazi] Rimozione dello spazio salvato fallita: {ex.Message}");
        }
        Cambiato?.Invoke();
    }

    private Guid? Leggi()
    {
        try
        {
            var valore = _js.Invoke<string?>("localStorage.getItem", ChiaveArchivio);
            return Guid.TryParse(valore, out var id) ? id : null;
        }
        catch (Exception ex)
        {
            // Si riparte dallo spazio personale: una preferenza illeggibile non vale il blocco
            // dell'applicazione, ma va detta, altrimenti un localStorage rotto resta invisibile.
            Console.Error.WriteLine($"[Spazi] Spazio salvato illeggibile, lo ignoro: {ex.Message}");
            return null;
        }
    }

    private void Salva(Guid id)
    {
        try
        {
            _js.InvokeVoid("localStorage.setItem", ChiaveArchivio, id.ToString());
        }
        catch (Exception ex)
        {
            // Si perde solo la memoria della scelta fra un caricamento e l'altro: non vale
            // un'eccezione in faccia all'utente, ma nemmeno il silenzio.
            Console.Error.WriteLine($"[Spazi] Salvataggio dello spazio scelto fallito: {ex.Message}");
        }
    }
}
