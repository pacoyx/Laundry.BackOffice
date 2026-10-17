using System.Text.Json.Serialization;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PickupGuideRevisionActionType
{
    QuantityAdjustment = 0,
    AddedItem = 1,
    RemovedItem = 2
}
