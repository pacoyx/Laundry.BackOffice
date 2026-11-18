using System.Collections.Concurrent;

namespace _24dex_backend_comercial.Features.PrinterHub;

public sealed class ConnectedWorker
{
    public int WorkerNodeId { get; set; }
    public int BranchId { get; set; }
    public string ConnectionId { get; set; } = string.Empty;
    public DateTime ConnectedAt { get; set; }
    public DateTime LastHeartbeat { get; set; }

    // Sello monotónico tomado al INICIO de RegisterPrinter. Permite descartar un registro
    // que empezó antes que el vigente pero terminó después (p. ej. una conexión vieja que
    // se quedó colgada en la BD y no debe desplazar a la conexión nueva ya registrada).
    public long RegistrationStamp { get; set; }
}

public enum RegisterStatus
{
    // La conexión quedó como la vigente del worker.
    Registered,

    // Rechazado: el registro empezó antes que el de la entrada vigente (sello menor).
    Stale,

    // Rechazado: esta conexión ya está registrada para otro worker.
    ConnectionBelongsToOtherWorker
}

/// <param name="Previous">
/// Registered: conexión previa distinta que fue reemplazada (o null).
/// Stale / ConnectionBelongsToOtherWorker: la entrada vigente que provocó el rechazo.
/// </param>
public sealed record RegisterResult(
    RegisterStatus Status,
    ConnectedWorker? Previous);


/// <summary>
/// Registro en memoria de los workers de impresión conectados.
/// Mantiene UNA sola entrada vigente por WorkerNodeId: si un worker se reconecta,
/// la conexión nueva reemplaza a la anterior. Así una conexión vieja que quedó
/// medio abierta no infla el conteo ni se considera "conectada".
/// </summary>
public sealed class PrinterRegistry
{
    // Clave: WorkerNodeId. Las lecturas no bloquean; las escrituras van bajo _lock
    // porque Register/Remove necesitan comparar y reemplazar de forma atómica
    // (son pocos workers, el lock no es un cuello de botella).
    private readonly ConcurrentDictionary<int, ConnectedWorker>
        _workers = new();

    private readonly object _lock = new();

    private long _lastStamp;

    /// <summary>
    /// Devuelve un sello creciente. Debe tomarse al inicio del registro,
    /// antes de cualquier operación lenta (BD, grupos).
    /// </summary>
    public long NextRegistrationStamp() =>
        Interlocked.Increment(ref _lastStamp);

    /// <summary>
    /// Intenta registrar la conexión como la vigente del worker.
    /// </summary>
    public RegisterResult Register(
        int workerNodeId,
        int branchId,
        string connectionId,
        long registrationStamp)
    {
        var now = DateTime.UtcNow;

        lock (_lock)
        {
            // Una conexión pertenece a un solo worker: no se mueve la entrada.
            var owner = _workers.Values
                .FirstOrDefault(x =>
                    x.ConnectionId == connectionId &&
                    x.WorkerNodeId != workerNodeId);

            if (owner is not null)
            {
                return new RegisterResult(
                    RegisterStatus.ConnectionBelongsToOtherWorker,
                    owner);
            }

            _workers.TryGetValue(workerNodeId, out var current);

            // Un registro que empezó antes que el vigente no lo desplaza.
            if (current is not null &&
                current.ConnectionId != connectionId &&
                registrationStamp < current.RegistrationStamp)
            {
                return new RegisterResult(
                    RegisterStatus.Stale,
                    current);
            }

            _workers[workerNodeId] =
                new ConnectedWorker
                {
                    WorkerNodeId = workerNodeId,
                    BranchId = branchId,
                    ConnectionId = connectionId,
                    ConnectedAt = now,
                    LastHeartbeat = now,
                    RegistrationStamp = registrationStamp
                };

            return new RegisterResult(
                RegisterStatus.Registered,
                current is not null &&
                current.ConnectionId != connectionId
                    ? current
                    : null);
        }
    }

    /// <summary>
    /// Actualiza el heartbeat. Devuelve true si la conexión es la vigente de algún
    /// worker; false si no (el worker debe volver a registrarse).
    /// </summary>
    public bool Heartbeat(
        string connectionId)
    {
        var worker = GetByConnectionId(connectionId);

        if (worker is null)
        {
            return false;
        }

        worker.LastHeartbeat =
            DateTime.UtcNow;

        return true;
    }

    /// <summary>
    /// Elimina la entrada solo si la conexión vigente del worker es ESTA.
    /// Una desconexión tardía de una conexión vieja no debe borrar la nueva.
    /// Devuelve el worker eliminado o null si la conexión no era la vigente.
    /// </summary>
    public ConnectedWorker? Remove(
        string connectionId)
    {
        lock (_lock)
        {
            var worker = _workers.Values
                .FirstOrDefault(x =>
                    x.ConnectionId == connectionId);

            if (worker is null)
            {
                return null;
            }

            // TryRemove con KeyValuePair compara también el valor: solo borra si la
            // entrada sigue siendo exactamente la encontrada.
            return _workers.TryRemove(
                new KeyValuePair<int, ConnectedWorker>(
                    worker.WorkerNodeId,
                    worker))
                ? worker
                : null;
        }
    }

    public IReadOnlyCollection<ConnectedWorker> GetAll()
    {
        return _workers.Values.ToList();
    }

    public ConnectedWorker? GetByConnectionId(
        string connectionId)
    {
        return _workers.Values
            .FirstOrDefault(x =>
                x.ConnectionId == connectionId);
    }

    public ConnectedWorker? GetByWorkerNodeId(
        int workerNodeId)
    {
        _workers.TryGetValue(
            workerNodeId,
            out var worker);

        return worker;
    }

    public IEnumerable<ConnectedWorker>
        GetByBranchId(
            int branchId)
    {
        return _workers.Values
            .Where(x =>
                x.BranchId == branchId);
    }

    public bool IsConnected(
        int workerNodeId)
    {
        return _workers.ContainsKey(workerNodeId);
    }

    /// <summary>Cantidad de workers conectados (una conexión vigente por worker).</summary>
    public int Count =>
        _workers.Count;
}
