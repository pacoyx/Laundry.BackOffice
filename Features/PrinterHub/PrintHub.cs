using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace _24dex_backend_comercial.Features.PrinterHub;
public class PrintHub : Hub
{
    // Clave en Context.Items para recordar qué worker registró esta conexión
    // (sirve para loguear en OnDisconnectedAsync aunque ya no sea la vigente).
    private const string WorkerNodeIdItemKey = "WorkerNodeId";

    private readonly RecepcionDbContext _db;
    private readonly PrinterRegistry _registry;
    private readonly ILogger<PrintHub> _logger;

    public PrintHub(
        RecepcionDbContext db,
        PrinterRegistry registry,
        ILogger<PrintHub> logger)
    {
        _db = db;
        _registry = registry;
        _logger = logger;
    }

    public async Task RegisterPrinter(
        string workerKey, string computerName)
    {
        // El sello se toma ANTES de ir a la BD: si esta conexión se queda colgada y otra
        // del mismo worker se registra mientras tanto, este registro tardío será rechazado.
        var registrationStamp = _registry.NextRegistrationStamp();
        var connectionId = Context.ConnectionId;

        // La WorkerKey es la credencial del worker: nunca se loguea completa.
        _logger.LogInformation(
            "Intentando registrar WorkerKey {WorkerKey} desde {ComputerName} (ConnectionId {ConnectionId})",
            MaskWorkerKey(workerKey),
            computerName,
            connectionId);

        var worker = await _db.WorkerNodes
            .Include(x => x.Branch)
            .SingleOrDefaultAsync(x =>
                x.WorkerKey == workerKey &&
                x.IsActive);
        if (worker is null)
        {
            _logger.LogWarning(
                "WorkerKey {WorkerKey} no encontrado (ComputerName {ComputerName}, ConnectionId {ConnectionId})",
                MaskWorkerKey(workerKey),
                computerName,
                connectionId);
            throw new HubException(
                "WorkerKey no registrado.");
        }

        _logger.LogInformation(
            "WorkerKey resuelta a WorkerNode {WorkerNodeId} (ConnectionId {ConnectionId})",
            worker.Id,
            connectionId);

        // Una conexión pertenece a un solo worker (Register lo vuelve a validar de forma atómica).
        var owner = _registry.GetByConnectionId(connectionId);
        if (owner is not null && owner.WorkerNodeId != worker.Id)
        {
            _logger.LogWarning(
                "ConnectionId {ConnectionId} ya registrada para WorkerNode {OwnerWorkerNodeId}; se rechaza registro para WorkerNode {WorkerNodeId}",
                connectionId,
                owner.WorkerNodeId,
                worker.Id);
            throw new HubException(
                "La conexión ya está registrada para otro worker.");
        }

        worker.ComputerName = computerName;
        worker.LastConnectionAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        var workerGroup = $"worker-{worker.Id}";
        var branchGroup = $"branch-{worker.BranchId}";

        // Se agrega a los grupos ANTES de registrar: si otra conexión del mismo worker
        // se registra después, al reemplazar a ésta ya estará en los grupos y la sacará
        // correctamente (al revés, podría quedar en el grupo como zombi).
        await Groups.AddToGroupAsync(connectionId, workerGroup);
        await Groups.AddToGroupAsync(connectionId, branchGroup);

        // Si la conexión se cerró mientras esperábamos la BD, no debe registrarse
        // (desplazaría a una conexión viva del mismo worker).
        if (Context.ConnectionAborted.IsCancellationRequested)
        {
            _logger.LogWarning(
                "ConnectionId {ConnectionId} se cerró antes de registrarse para WorkerNode {WorkerNodeId}; se descarta el registro",
                connectionId,
                worker.Id);
            _registry.Remove(connectionId);
            await RemoveFromGroupSafeAsync(connectionId, workerGroup);
            await RemoveFromGroupSafeAsync(connectionId, branchGroup);
            return;
        }

        var result = _registry.Register(
            worker.Id,
            worker.BranchId,
            connectionId,
            registrationStamp);

        if (result.Status == RegisterStatus.ConnectionBelongsToOtherWorker)
        {
            // Carrera con otro RegisterPrinter de esta misma conexión: se deshace solo lo
            // agregado aquí, sin sacarla de los grupos del worker al que sí pertenece.
            _logger.LogWarning(
                "ConnectionId {ConnectionId} ya registrada para WorkerNode {OwnerWorkerNodeId}; se rechaza registro para WorkerNode {WorkerNodeId}",
                connectionId,
                result.Previous?.WorkerNodeId,
                worker.Id);
            await RemoveFromGroupSafeAsync(connectionId, workerGroup);
            if (result.Previous?.BranchId != worker.BranchId)
            {
                await RemoveFromGroupSafeAsync(connectionId, branchGroup);
            }
            throw new HubException(
                "La conexión ya está registrada para otro worker.");
        }

        if (result.Status == RegisterStatus.Stale)
        {
            // Registro tardío: la vigente empezó a registrarse después que ésta.
            // Se saca a la conexión ENTRANTE de los grupos y no se toca la vigente.
            _logger.LogWarning(
                "Registro tardío de ConnectionId {ConnectionId} para WorkerNode {WorkerNodeId} descartado; la vigente es {CurrentConnectionId}",
                connectionId,
                worker.Id,
                result.Previous?.ConnectionId);
            await RemoveFromGroupSafeAsync(connectionId, workerGroup);
            await RemoveFromGroupSafeAsync(connectionId, branchGroup);
            return;
        }

        // Re-chequeo: si se abortó justo durante el registro, se quita la entrada (solo si
        // sigue siendo la vigente) y NO se saca a la previa de los grupos.
        if (Context.ConnectionAborted.IsCancellationRequested)
        {
            _logger.LogWarning(
                "ConnectionId {ConnectionId} se cerró durante el registro de WorkerNode {WorkerNodeId}; se revierte",
                connectionId,
                worker.Id);
            _registry.Remove(connectionId);
            await RemoveFromGroupSafeAsync(connectionId, workerGroup);
            await RemoveFromGroupSafeAsync(connectionId, branchGroup);
            return;
        }

        Context.Items[WorkerNodeIdItemKey] = worker.Id;

        var previous = result.Previous;
        if (previous is not null)
        {
            _logger.LogInformation(
                "WorkerNode {WorkerNodeId}: conexión previa {PreviousConnectionId} reemplazada por {ConnectionId}",
                worker.Id,
                previous.ConnectionId,
                connectionId);

            // La conexión vieja puede seguir medio abierta; se saca de los grupos para
            // que no reciba despachos. Un fallo aquí no debe impedir el registro.
            await RemoveFromGroupSafeAsync(previous.ConnectionId, workerGroup);
            await RemoveFromGroupSafeAsync(previous.ConnectionId, $"branch-{previous.BranchId}");
        }

        _logger.LogInformation(
            "WorkerNode {WorkerNodeId} registrado para sucursal {BranchId} (ConnectionId {ConnectionId})",
            worker.Id,
            worker.BranchId,
            connectionId);
    }

    /// <summary>
    /// Devuelve true si esta conexión es la vigente de algún worker; false si no
    /// (el worker debe volver a llamar RegisterPrinter). Los workers que invocan
    /// "Heartbeat" sin leer el resultado siguen funcionando igual.
    /// </summary>
    public bool Heartbeat()
    {
        return _registry.Heartbeat(Context.ConnectionId);
    }

    public override async Task OnDisconnectedAsync(
        Exception? exception)
    {
        var removed = _registry.Remove(Context.ConnectionId);

        int? workerNodeId = removed?.WorkerNodeId
            ?? (Context.Items.TryGetValue(WorkerNodeIdItemKey, out var item) && item is int id
                ? id
                : null);

        // Vigente: era la conexión registrada del worker y se quitó del registro.
        // Vieja: ya había sido reemplazada (o nunca se registró); el registro no se toca.
        var connectionKind = removed is not null ? "vigente" : "vieja/no vigente";

        if (exception is null)
        {
            _logger.LogInformation(
                "Desconexión de ConnectionId {ConnectionId} (WorkerNode {WorkerNodeId}, conexión {ConnectionKind})",
                Context.ConnectionId,
                workerNodeId,
                connectionKind);
        }
        else
        {
            _logger.LogWarning(
                exception,
                "Desconexión con error de ConnectionId {ConnectionId} (WorkerNode {WorkerNodeId}, conexión {ConnectionKind})",
                Context.ConnectionId,
                workerNodeId,
                connectionKind);
        }

        await base.OnDisconnectedAsync(exception);
    }

    private async Task RemoveFromGroupSafeAsync(
        string connectionId, string groupName)
    {
        try
        {
            await Groups.RemoveFromGroupAsync(connectionId, groupName);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "No se pudo sacar la conexión {ConnectionId} del grupo {GroupName}",
                connectionId,
                groupName);
        }
    }

    // Muestra solo los primeros 4 caracteres; si la key es tan corta que eso la
    // revelaría entera (o es vacía), se oculta por completo.
    private static string MaskWorkerKey(string? workerKey)
    {
        if (string.IsNullOrEmpty(workerKey) || workerKey.Length <= 4)
        {
            return "***";
        }

        return workerKey[..4] + "***";
    }
}
