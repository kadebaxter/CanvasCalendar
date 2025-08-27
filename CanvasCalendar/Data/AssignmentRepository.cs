using CanvasCalendar.Models;
using Microsoft.Data.Sqlite;

namespace CanvasCalendar.Data;

/// <summary>
/// Repository class for managing assignments in the database.
/// </summary>
public class AssignmentRepository
{
    private bool _hasBeenInitialized = false;
    private readonly CourseRepository _courseRepository;

    public AssignmentRepository(CourseRepository courseRepository)
    {
        _courseRepository = courseRepository;
    }

    /// <summary>
    /// Initializes the database connection and creates the Assignment table if it does not exist.
    /// </summary>
    private async Task Init()
    {
        if (_hasBeenInitialized)
            return;

        await using var connection = new SqliteConnection(Constants.DatabasePath);
        await connection.OpenAsync();

        try
        {
            var createTableCmd = connection.CreateCommand();
            createTableCmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS Assignment (
                    ID INTEGER PRIMARY KEY AUTOINCREMENT,
                    CanvasId TEXT NOT NULL UNIQUE,
                    Title TEXT NOT NULL,
                    Description TEXT NOT NULL,
                    DueDate TEXT NOT NULL,
                    CourseID INTEGER NOT NULL,
                    PointsPossible REAL NOT NULL,
                    Status INTEGER NOT NULL DEFAULT 0,
                    AssignmentGroupId TEXT NOT NULL,
                    HtmlUrl TEXT NOT NULL,
                    Published INTEGER NOT NULL DEFAULT 1,
                    CreatedAt TEXT NOT NULL,
                    UpdatedAt TEXT NOT NULL,
                    FOREIGN KEY (CourseID) REFERENCES Course (ID)
                );";
            await createTableCmd.ExecuteNonQueryAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error creating Assignment table: {e.Message}");
            throw;
        }

        _hasBeenInitialized = true;
    }

    /// <summary>
    /// Retrieves a list of all assignments from the database.
    /// </summary>
    public async Task<List<Assignment>> ListAsync()
    {
        await Init();
        await using var connection = new SqliteConnection(Constants.DatabasePath);
        await connection.OpenAsync();

        var selectCmd = connection.CreateCommand();
        selectCmd.CommandText = "SELECT * FROM Assignment ORDER BY DueDate ASC";
        var assignments = new List<Assignment>();

        await using var reader = await selectCmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            assignments.Add(new Assignment
            {
                ID = reader.GetInt32(0),
                CanvasId = reader.GetString(1),
                Title = reader.GetString(2),
                Description = reader.GetString(3),
                DueDate = DateTime.Parse(reader.GetString(4)),
                CourseID = reader.GetInt32(5),
                PointsPossible = reader.GetDouble(6),
                Status = (AssignmentStatus)reader.GetInt32(7),
                AssignmentGroupId = reader.GetString(8),
                HtmlUrl = reader.GetString(9),
                Published = reader.GetInt32(10) == 1,
                CreatedAt = DateTime.Parse(reader.GetString(11)),
                UpdatedAt = DateTime.Parse(reader.GetString(12))
            });
        }

        // Load course information for each assignment
        foreach (var assignment in assignments)
        {
            assignment.Course = await _courseRepository.GetAsync(assignment.CourseID);
        }

        return assignments;
    }

    /// <summary>
    /// Retrieves assignments due within the specified date range.
    /// </summary>
    public async Task<List<Assignment>> GetAssignmentsDueInRangeAsync(DateTime startDate, DateTime endDate)
    {
        await Init();
        await using var connection = new SqliteConnection(Constants.DatabasePath);
        await connection.OpenAsync();

        var selectCmd = connection.CreateCommand();
        selectCmd.CommandText = @"
            SELECT * FROM Assignment 
            WHERE DueDate >= @startDate AND DueDate <= @endDate 
            ORDER BY DueDate ASC";
        selectCmd.Parameters.AddWithValue("@startDate", startDate.ToString("O"));
        selectCmd.Parameters.AddWithValue("@endDate", endDate.ToString("O"));

        var assignments = new List<Assignment>();

        await using var reader = await selectCmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            assignments.Add(new Assignment
            {
                ID = reader.GetInt32(0),
                CanvasId = reader.GetString(1),
                Title = reader.GetString(2),
                Description = reader.GetString(3),
                DueDate = DateTime.Parse(reader.GetString(4)),
                CourseID = reader.GetInt32(5),
                PointsPossible = reader.GetDouble(6),
                Status = (AssignmentStatus)reader.GetInt32(7),
                AssignmentGroupId = reader.GetString(8),
                HtmlUrl = reader.GetString(9),
                Published = reader.GetInt32(10) == 1,
                CreatedAt = DateTime.Parse(reader.GetString(11)),
                UpdatedAt = DateTime.Parse(reader.GetString(12))
            });
        }

        // Load course information for each assignment
        foreach (var assignment in assignments)
        {
            assignment.Course = await _courseRepository.GetAsync(assignment.CourseID);
        }

        return assignments;
    }

    /// <summary>
    /// Retrieves a specific assignment by its ID.
    /// </summary>
    public async Task<Assignment?> GetAsync(int id)
    {
        await Init();
        await using var connection = new SqliteConnection(Constants.DatabasePath);
        await connection.OpenAsync();

        var selectCmd = connection.CreateCommand();
        selectCmd.CommandText = "SELECT * FROM Assignment WHERE ID = @id";
        selectCmd.Parameters.AddWithValue("@id", id);

        await using var reader = await selectCmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            var assignment = new Assignment
            {
                ID = reader.GetInt32(0),
                CanvasId = reader.GetString(1),
                Title = reader.GetString(2),
                Description = reader.GetString(3),
                DueDate = DateTime.Parse(reader.GetString(4)),
                CourseID = reader.GetInt32(5),
                PointsPossible = reader.GetDouble(6),
                Status = (AssignmentStatus)reader.GetInt32(7),
                AssignmentGroupId = reader.GetString(8),
                HtmlUrl = reader.GetString(9),
                Published = reader.GetInt32(10) == 1,
                CreatedAt = DateTime.Parse(reader.GetString(11)),
                UpdatedAt = DateTime.Parse(reader.GetString(12))
            };

            assignment.Course = await _courseRepository.GetAsync(assignment.CourseID);
            return assignment;
        }

        return null;
    }

    /// <summary>
    /// Gets an assignment by Canvas ID.
    /// </summary>
    public async Task<Assignment?> GetByCanvasIdAsync(string canvasId)
    {
        await Init();
        await using var connection = new SqliteConnection(Constants.DatabasePath);
        await connection.OpenAsync();

        var selectCmd = connection.CreateCommand();
        selectCmd.CommandText = "SELECT * FROM Assignment WHERE CanvasId = @canvasId";
        selectCmd.Parameters.AddWithValue("@canvasId", canvasId);

        await using var reader = await selectCmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            var assignment = new Assignment
            {
                ID = reader.GetInt32(0),
                CanvasId = reader.GetString(1),
                Title = reader.GetString(2),
                Description = reader.GetString(3),
                DueDate = DateTime.Parse(reader.GetString(4)),
                CourseID = reader.GetInt32(5),
                PointsPossible = reader.GetDouble(6),
                Status = (AssignmentStatus)reader.GetInt32(7),
                AssignmentGroupId = reader.GetString(8),
                HtmlUrl = reader.GetString(9),
                Published = reader.GetInt32(10) == 1,
                CreatedAt = DateTime.Parse(reader.GetString(11)),
                UpdatedAt = DateTime.Parse(reader.GetString(12))
            };

            assignment.Course = await _courseRepository.GetAsync(assignment.CourseID);
            return assignment;
        }

        return null;
    }

    /// <summary>
    /// Saves an assignment to the database.
    /// </summary>
    public async Task<int> SaveItemAsync(Assignment item)
    {
        await Init();
        await using var connection = new SqliteConnection(Constants.DatabasePath);
        await connection.OpenAsync();

        var saveCmd = connection.CreateCommand();
        if (item.ID == 0)
        {
            saveCmd.CommandText = @"
                INSERT INTO Assignment (CanvasId, Title, Description, DueDate, CourseID, PointsPossible, 
                                      Status, AssignmentGroupId, HtmlUrl, Published, CreatedAt, UpdatedAt)
                VALUES (@CanvasId, @Title, @Description, @DueDate, @CourseID, @PointsPossible, 
                        @Status, @AssignmentGroupId, @HtmlUrl, @Published, @CreatedAt, @UpdatedAt);
                SELECT last_insert_rowid();";
        }
        else
        {
            saveCmd.CommandText = @"
                UPDATE Assignment
                SET CanvasId = @CanvasId, Title = @Title, Description = @Description, DueDate = @DueDate,
                    CourseID = @CourseID, PointsPossible = @PointsPossible, Status = @Status,
                    AssignmentGroupId = @AssignmentGroupId, HtmlUrl = @HtmlUrl, Published = @Published,
                    CreatedAt = @CreatedAt, UpdatedAt = @UpdatedAt
                WHERE ID = @ID";
            saveCmd.Parameters.AddWithValue("@ID", item.ID);
        }

        saveCmd.Parameters.AddWithValue("@CanvasId", item.CanvasId);
        saveCmd.Parameters.AddWithValue("@Title", item.Title);
        saveCmd.Parameters.AddWithValue("@Description", item.Description);
        saveCmd.Parameters.AddWithValue("@DueDate", item.DueDate.ToString("O"));
        saveCmd.Parameters.AddWithValue("@CourseID", item.CourseID);
        saveCmd.Parameters.AddWithValue("@PointsPossible", item.PointsPossible);
        saveCmd.Parameters.AddWithValue("@Status", (int)item.Status);
        saveCmd.Parameters.AddWithValue("@AssignmentGroupId", item.AssignmentGroupId);
        saveCmd.Parameters.AddWithValue("@HtmlUrl", item.HtmlUrl);
        saveCmd.Parameters.AddWithValue("@Published", item.Published ? 1 : 0);
        saveCmd.Parameters.AddWithValue("@CreatedAt", item.CreatedAt.ToString("O"));
        saveCmd.Parameters.AddWithValue("@UpdatedAt", item.UpdatedAt.ToString("O"));

        var result = await saveCmd.ExecuteScalarAsync();
        if (item.ID == 0)
        {
            item.ID = Convert.ToInt32(result);
        }

        return item.ID;
    }

    /// <summary>
    /// Deletes an assignment from the database.
    /// </summary>
    public async Task<int> DeleteItemAsync(Assignment item)
    {
        await Init();
        await using var connection = new SqliteConnection(Constants.DatabasePath);
        await connection.OpenAsync();

        var deleteCmd = connection.CreateCommand();
        deleteCmd.CommandText = "DELETE FROM Assignment WHERE ID = @ID";
        deleteCmd.Parameters.AddWithValue("@ID", item.ID);

        return await deleteCmd.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// Drops the Assignment table from the database.
    /// </summary>
    public async Task DropTableAsync()
    {
        await Init();
        await using var connection = new SqliteConnection(Constants.DatabasePath);
        await connection.OpenAsync();

        var dropCmd = connection.CreateCommand();
        dropCmd.CommandText = "DROP TABLE IF EXISTS Assignment";
        await dropCmd.ExecuteNonQueryAsync();

        _hasBeenInitialized = false;
    }
}
