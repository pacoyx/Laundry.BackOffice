public static class WorkGuideModificationGuard
{
    public static bool IsBlocked(string estadoPago) => estadoPago == "PA" || estadoPago == "AN";
}
