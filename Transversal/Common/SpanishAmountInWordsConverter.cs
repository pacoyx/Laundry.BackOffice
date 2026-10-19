public static class SpanishAmountInWordsConverter
{
    private static readonly string[] Unidades =
    {
        "", "UN", "DOS", "TRES", "CUATRO", "CINCO", "SEIS", "SIETE", "OCHO", "NUEVE",
        "DIEZ", "ONCE", "DOCE", "TRECE", "CATORCE", "QUINCE", "DIECISEIS", "DIECISIETE",
        "DIECIOCHO", "DIECINUEVE", "VEINTE"
    };

    private static readonly string[] Decenas =
    {
        "", "", "VEINTI", "TREINTA", "CUARENTA", "CINCUENTA", "SESENTA", "SETENTA", "OCHENTA", "NOVENTA"
    };

    private static readonly string[] Centenas =
    {
        "", "CIENTO", "DOSCIENTOS", "TRESCIENTOS", "CUATROCIENTOS", "QUINIENTOS",
        "SEISCIENTOS", "SETECIENTOS", "OCHOCIENTOS", "NOVECIENTOS"
    };

    public static string Convert(decimal amount, string currencyId)
    {
        var integerPart = (long)Math.Truncate(amount);
        var decimalPart = (int)Math.Round((amount - integerPart) * 100);

        var words = integerPart == 0 ? "CERO" : ConvertInteger(integerPart);
        var currencyName = GetCurrencyName(currencyId);

        return $"{words} Y {decimalPart:00}/100 {currencyName}";
    }

    private static string GetCurrencyName(string currencyId) => currencyId switch
    {
        "PEN" => "SOLES",
        "USD" => "DOLARES AMERICANOS",
        _ => currencyId
    };

    private static string ConvertInteger(long number)
    {
        if (number == 0)
        {
            return "";
        }

        if (number == 100)
        {
            return "CIEN";
        }

        if (number < 21)
        {
            return Unidades[number];
        }

        if (number < 100)
        {
            var decena = number / 10;
            var unidad = number % 10;

            if (decena == 2)
            {
                return unidad == 0 ? "VEINTE" : Decenas[2] + Unidades[unidad];
            }

            return unidad == 0 ? Decenas[decena] : $"{Decenas[decena]} Y {Unidades[unidad]}";
        }

        if (number < 1000)
        {
            var centena = number / 100;
            var resto = number % 100;
            var centenaText = Centenas[centena];

            return resto == 0 ? centenaText : $"{centenaText} {ConvertInteger(resto)}";
        }

        if (number < 1_000_000)
        {
            var miles = number / 1000;
            var resto = number % 1000;
            var milesText = miles == 1 ? "MIL" : $"{ConvertInteger(miles)} MIL";

            return resto == 0 ? milesText : $"{milesText} {ConvertInteger(resto)}";
        }

        if (number < 1_000_000_000)
        {
            var millones = number / 1_000_000;
            var resto = number % 1_000_000;
            var millonesText = millones == 1 ? "UN MILLON" : $"{ConvertInteger(millones)} MILLONES";

            return resto == 0 ? millonesText : $"{millonesText} {ConvertInteger(resto)}";
        }

        return number.ToString();
    }
}
