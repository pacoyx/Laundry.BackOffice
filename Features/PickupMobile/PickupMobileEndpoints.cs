// API para la PWA Laundry.PickupMobile (personal de recojo).
//
// Seguridad: todo este grupo exige el permiso "pickup_mobile.access" (PermissionCodes.PickupMobile)
// presente en el JWT. Un usuario que SOLO tenga este permiso puede: ver sus guías, crear/editar guías
// en estado Draft y adjuntar/consultar/descargar/eliminar evidencias (fotos y firma) de sus propias guías.
// NO puede: cambiar estados administrativos, generar PDF, crear revisiones, reenviar correos ni acceder
// a ninguna funcionalidad de Backoffice (esas acciones siguen exigiendo corporate-operations.access).
//
// El login sigue siendo el mismo POST /login del Backoffice: no existe un login propio para la PWA.
// La identidad del "usuario actual" se resuelve vía el claim "employeeId" del JWT (ver User.EmployeeId
// y CurrentUserExtensions.GetEmployeeId), nunca recibiendo EmployeeId como parámetro del cliente.
//
// Reutilización: los comandos de crear/editar guía y de subir/eliminar/descargar evidencias llaman
// directamente a los métodos ExecuteAsync ya existentes en Features/CorporateOperations/PickupGuides,
// agregando únicamente las reglas de acceso y restricciones propias del canal móvil.
public static class PickupMobileEndpoints
{
    public static void MapPickupMobile(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/pickup-mobile")
            .WithTags("PickupMobile")
            .RequirePermission(PermissionCodes.PickupMobile);

        group.MapGetPickupMobileCustomers();
        group.MapGetPickupMobileVehicles();
        group.MapGetPickupMobileEmployees();
        group.MapGetPickupMobileServiceItems();
        group.MapGetPickupMobileDashboard();

        group.MapGetMyPickupGuides();
        group.MapGetPickupMobileGuide();
        group.MapCreatePickupMobileGuide();
        group.MapUpdatePickupMobileGuide();

        group.MapGetPickupMobileAttachments();
        group.MapCreatePickupMobileAttachment();
        group.MapDeletePickupMobileAttachment();
        group.MapDownloadPickupMobileAttachment();
    }

    internal static PickupMobileAttachmentDto ToAttachmentDto(PickupGuideAttachment attachment) => new(
        attachment.Id, attachment.AttachmentType.ToString(), attachment.OriginalFileName,
        attachment.ContentType, attachment.FileSize, attachment.CreatedAt
    );

    internal static PickupMobileGuideDto ToGuideDto(PickupGuide guide) => new(
        guide.Id, guide.GuideNumber, guide.PhysicalGuideNumber, PickupGuidesEndpoints.GetCustomerDisplayName(guide.Customer), guide.PickupDate, guide.Status,
        guide.Attachments.Any(a => a.IsCurrent && a.AttachmentType == PickupGuideAttachmentType.Photo),
        guide.Attachments.Any(a => a.IsCurrent && a.AttachmentType == PickupGuideAttachmentType.Signature)
    );
}
