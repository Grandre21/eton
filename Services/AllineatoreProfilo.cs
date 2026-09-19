using Microsoft.JSInterop;
using Eton.Models;

namespace Eton.Services;

/// <summary>
/// Allinea <c>profiles.display_name</c> e <c>profiles.avatar_url</c> al valore vivo della sessione:
/// <c>handle_new_user</c> li scrive una volta sola al primo accesso, e senza questo servizio chi
/// cambia nome o foto su Google li vede aggiornati solo sulla propria pagina, mai negli spazi
/// condivisi con gli altri membri.
/// <para>
/// Non più di una chiamata di rete aggiuntiva al bootstrap, nel caso normale: si tiene in
/// localStorage un'impronta di ciò che <b>questo dispositivo</b> ha già scritto con successo, e si
/// scrive solo quando la sessione viva se ne discosta. <see cref="Impronta"/> e
/// <see cref="VaScritto"/> sono le due decisioni pure, isolate per essere verificabili senza
/// browser né rete — stesso motivo per cui esiste <see cref="SessionFreshness"/>.
/// </para>
/// </summary>
public class AllineatoreProfilo
{
    private const string ChiaveArchivio = "eton.profilo";

    private readonly IJSInProcessRuntime _js;

    public AllineatoreProfilo(IJSInProcessRuntime js) => _js = js;

    /// <summary>Impronta di un valore nome/foto per un utente. L'identificatore utente ci sta
    /// dentro apposta: senza, due account sullo stesso browser produrrebbero la stessa impronta e
    /// il cambio account non farebbe scattare la scrittura. Si confronta per intero e nessuno la
    /// scompone: una collisione fra campi — che richiederebbe il carattere di controllo dentro un
    /// nome — costerebbe al più una scrittura saltata fino al successivo accesso Google, che
    /// ignora l'impronta; non un dato corrotto.</summary>
    public static string Impronta(string utenteId, string? nome, string? foto)
        => $"{utenteId}\u001F{nome}\u001F{foto}";

    /// <summary>True se va scritto: sempre appena tornati da un accesso Google (l'impronta
    /// salvata può essere quella di un altro dispositivo, e in quel caso mentirebbe), altrimenti
    /// solo se l'impronta salvata — compresa quella assente — differisce da quella viva.</summary>
    public static bool VaScritto(string? improntaSalvata, string improntaViva, bool dopoAccesso)
        => dopoAccesso || improntaSalvata != improntaViva;

    /// <summary>
    /// Scrive display_name e avatar_url se e solo se serve. Non propaga MAI un'eccezione: gira sul
    /// percorso di avvio, dove un'eccezione blocca l'app sul caricamento (v. il commento in
    /// <see cref="SupabaseService.GetClientAsync"/>).
    /// </summary>
    public async Task AllineaAsync(SupabaseClient client, bool dopoAccesso)
    {
        try
        {
            var utente = client.Auth.CurrentSession?.User;
            if (utente is null || !Guid.TryParse(utente.Id, out var id)) return;

            var nome = IdentitaGoogle.NomeDa(utente);
            var foto = IdentitaGoogle.FotoDa(utente);

            // Guardia per la sessione rotta, non per i metadati vuoti: con metadati vuoti NomeDa
            // ripiega comunque sull'email, quindi nome non è mai vuoto in quel caso e la scrittura
            // procede normalmente. Scatta solo quando anche l'email manca, cioè quando la sessione
            // non dice nulla di sensato su cui scrivere.
            //
            // foto invece non ha ripiego — nemmeno in handle_new_user, che scrive avatar_url dalla
            // sola chiave omonima — quindi null qui è un valore legittimo, non un'assenza di
            // informazione: il mandato chiede di propagare il valore nuovo quando l'utente cambia
            // nome O foto, e se la foto è stata rimossa il valore nuovo è proprio quell'assenza. Una
            // guardia che non scrivesse mai null lascerebbe sulla foto lo stesso difetto che questa
            // guardia chiude sul nome.
            if (string.IsNullOrWhiteSpace(nome)) return;

            var viva = Impronta(utente.Id, nome, foto);
            if (!VaScritto(Leggi(), viva, dopoAccesso)) return;

            var risposta = await client.From<Profile>()
                .Where(p => p.Id == id)
                .Set(p => p.DisplayName!, nome)
                .Set(p => p.AvatarUrl!, foto)
                .Update();

            // Update() non lancia quando la policy filtra la riga o la riga non esiste: torna una
            // risposta con Models vuoto. Salvare l'impronta solo qui, e non "perché non ci sono
            // state eccezioni", è l'unico modo di non richiudere il difetto in silenzio.
            if (risposta.Models.Count > 0) Salva(viva);
        }
        catch (Exception ex)
        {
            // Si riprova al prossimo avvio: l'impronta non è stata salvata, quindi VaScritto
            // tornerà true alla prossima occasione.
            Console.Error.WriteLine($"[Profilo] Allineamento del profilo fallito, riprovo al prossimo avvio: {ex.Message}");
        }
    }

    private string? Leggi()
    {
        try
        {
            var valore = _js.Invoke<string?>("localStorage.getItem", ChiaveArchivio);
            return string.IsNullOrEmpty(valore) ? null : valore;
        }
        catch (Exception ex)
        {
            // Si ripiega su null, cioè "non risulta allineato": costa una scrittura in più al
            // prossimo avvio, mai un dato scorretto — è il degrado peggiore possibile e non va oltre.
            Console.Error.WriteLine($"[Profilo] Impronta salvata illeggibile, la ignoro: {ex.Message}");
            return null;
        }
    }

    private void Salva(string impronta)
    {
        try
        {
            _js.InvokeVoid("localStorage.setItem", ChiaveArchivio, impronta);
        }
        catch (Exception ex)
        {
            // Si perde solo il risparmio della prossima chiamata di rete: il profilo è già stato
            // scritto con successo, quindi non è la correttezza a essere in gioco qui.
            Console.Error.WriteLine($"[Profilo] Salvataggio dell'impronta fallito: {ex.Message}");
        }
    }
}
