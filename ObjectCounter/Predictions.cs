namespace AzureObjCounterS{
    public class Predictions {
    public double probability { get; set; }
    public string tagId { get; set; }
    public string tagName { get; set; }
    public BoundingBox boundingBox { get; set; }
    }
}
