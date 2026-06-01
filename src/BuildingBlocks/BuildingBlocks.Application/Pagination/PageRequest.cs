namespace SchoolErp.BuildingBlocks.Application.Pagination;

public sealed record PageRequest(int Page = 1, int PageSize = 20)
{
    public int Skip => Math.Max(0, (Page - 1) * PageSize);
}
