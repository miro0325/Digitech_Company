public interface IPVReAllocatable
{
    public string GetReAllocatedData();
    public string ReAllocatePhotonViews();
    public void ReBindPhotonViews(string allocatedDate);
}