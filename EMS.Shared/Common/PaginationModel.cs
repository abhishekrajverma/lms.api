namespace EMS.Shared.Common;

/// <summary>
/// Pagination parameters for querying large datasets
/// 
/// Used for:
/// - Listing all students in a school
/// - Listing all courses
/// - Listing student grades
/// - Listing attendance records
/// - Any endpoint returning multiple items
/// 
/// Example API Call:
/// GET /api/v1/students?pageNumber=2&pageSize=25
/// 
/// This returns students 26-50 from the database
/// </summary>
public class PaginationModel
{
    private int _pageNumber = 1;
    private int _pageSize = 10;

    /// <summary>
    /// Current page number (1-based)
    /// 
    /// Valid range: >= 1
    /// If invalid, automatically reset to 1
    /// 
    /// Example:
    /// pageNumber = 1 ? First 10 students (items 1-10)
    /// pageNumber = 2 ? Next 10 students (items 11-20)
    /// pageNumber = 5 ? Items 41-50
    /// </summary>
    public int PageNumber
    {
        get => _pageNumber;
        set => _pageNumber = value < 1 ? 1 : value; // Auto-correct invalid values
    }

    /// <summary>
    /// Number of items to return per page (page size)
    /// 
    /// Valid range: 1 to 100 (max 100 to prevent large queries)
    /// Default: 10 items per page
    /// 
    /// If invalid:
    /// - Less than 1 ? Reset to 10
    /// - Greater than 100 ? Reset to 100 (prevents database overload)
    /// 
    /// Example:
    /// pageSize = 10 ? Return 10 items per page
    /// pageSize = 25 ? Return 25 items per page
    /// pageSize = 500 (too large) ? Automatically capped at 100
    /// </summary>
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value < 1 ? 10 : (value > 100 ? 100 : value);
    }

    /// <summary>
    /// Calculates the SQL OFFSET for database queries
    /// 
    /// Formula: (PageNumber - 1) * PageSize
    /// 
    /// Examples:
    /// Page 1, Size 10 ? Offset = 0 (start at item 1)
    /// Page 2, Size 10 ? Offset = 10 (start at item 11)
    /// Page 5, Size 25 ? Offset = 100 (start at item 101)
    /// 
    /// SQL Example:
    /// SELECT * FROM Students OFFSET 10 ROWS FETCH NEXT 10 ROWS ONLY
    /// This returns items 11-20
    /// </summary>
    public int GetOffset() => (PageNumber - 1) * PageSize;

    /// <summary>
    /// Gets the remaining items to skip from the beginning
    /// Useful for some ORM frameworks that use Skip/Take pattern
    /// </summary>
    public int GetSkip() => GetOffset();

    /// <summary>
    /// Gets the number of items to take (same as PageSize)
    /// Useful for some ORM frameworks that use Skip/Take pattern
    /// </summary>
    public int GetTake() => PageSize;

    /// <summary>
    /// Validates pagination parameters
    /// Returns true if valid, false otherwise
    /// </summary>
    public bool IsValid() => PageNumber >= 1 && PageSize >= 1 && PageSize <= 100;

    /// <summary>
    /// Creates a new pagination model with specified values
    /// 
    /// Example:
    /// var pagination = PaginationModel.Create(pageNumber: 2, pageSize: 25);
    /// </summary>
    public static PaginationModel Create(int pageNumber = 1, int pageSize = 10)
    {
        return new PaginationModel
        {
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public override string ToString() => $"Page {PageNumber}, Size {PageSize}";
}
