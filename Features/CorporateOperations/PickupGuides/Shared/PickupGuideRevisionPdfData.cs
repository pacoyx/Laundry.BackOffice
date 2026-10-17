public class PickupGuideRevisionPdfData
{
    public PickupGuideRevision Revision { get; }

    public List<byte[]> PhotoImages { get; }

    public PickupGuideRevisionPdfData(PickupGuideRevision revision, List<byte[]> photoImages)
    {
        Revision = revision;
        PhotoImages = photoImages;
    }
}
