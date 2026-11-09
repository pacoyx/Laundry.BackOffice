public static class ChatbotToolDefinitions
{
    public static readonly ChatToolDefinition ListDistricts = new(
        "list_districts",
        "Lista los distritos donde hay cobertura de recojo/entrega, con su id y zona. Usar esto primero para resolver el nombre de distrito que menciona el cliente a un districtId antes de llamar a get_price_list.",
        """{ "type": "object", "properties": {} } """);

    public static readonly ChatToolDefinition GetPriceList = new(
        "get_price_list",
        "Devuelve la tarifa vigente (servicios, precio y tiempo estimado de entrega en horas) para un distrito. Requiere el districtId obtenido con list_districts.",
        """
        {
          "type": "object",
          "properties": {
            "districtId": { "type": "integer", "description": "Id del distrito obtenido previamente con list_districts" }
          },
          "required": ["districtId"]
        }
        """);

    public static readonly ChatToolDefinition GetOrderStatus = new(
        "get_order_status",
        "Consulta el estado de un pedido web por su numero de pedido y el telefono de contacto registrado. Ambos datos deben coincidir exactamente.",
        """
        {
          "type": "object",
          "properties": {
            "orderNumber": { "type": "string", "description": "Numero de pedido tal como lo escribe el cliente" },
            "phone": { "type": "string", "description": "Telefono de contacto del pedido" }
          },
          "required": ["orderNumber", "phone"]
        }
        """);

    public static readonly IReadOnlyList<ChatToolDefinition> All = new[]
    {
        ListDistricts, GetPriceList, GetOrderStatus
    };
}
