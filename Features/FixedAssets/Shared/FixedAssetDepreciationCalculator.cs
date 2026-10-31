public static class FixedAssetDepreciationCalculator
{
    public static int GetEffectiveUsefulLifeYears(FixedAsset asset) =>
        asset.UsefulLifeYearsOverride ?? asset.Category.DefaultUsefulLifeYears;

    public static decimal GetBookValue(FixedAsset asset)
    {
        var usefulLifeYears = GetEffectiveUsefulLifeYears(asset);
        if (usefulLifeYears <= 0)
        {
            return asset.AcquisitionCost;
        }

        var yearsElapsed = (decimal)(DateTime.UtcNow - asset.AcquisitionDate).TotalDays / 365m;
        var annualDepreciation = asset.AcquisitionCost / usefulLifeYears;
        var accumulatedDepreciation = annualDepreciation * yearsElapsed;

        var bookValue = asset.AcquisitionCost - accumulatedDepreciation;
        return Math.Max(0, Math.Round(bookValue, 2));
    }
}
