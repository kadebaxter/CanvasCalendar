using CanvasCalendar.Models;
using Microsoft.Data.Sqlite;

namespace CanvasCalendar.Data;

/// <summary>
/// Repository class for managing courses in the database.
/// </summary>
public class CourseRepository
{
    private bool _hasBeenInitialized = false;

    public CourseRepository()
    {
    }

    /// <summary>
    /// Initializes the database connection and creates the Course table if it does not exist.
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
                CREATE TABLE IF NOT EXISTS Course (
                    ID INTEGER PRIMARY KEY AUTOINCREMENT,
                    CanvasId TEXT NOT NULL UNIQUE,
                    Name TEXT NOT NULL,
                    Code TEXT NOT NULL,
                    Term TEXT NOT NULL,
                    AccountId TEXT NOT NULL,
                    SisCourseId TEXT NOT NULL,
                    WorkflowState TEXT NOT NULL,
                    StartAt TEXT NOT NULL,
                    EndAt TEXT NOT NULL,
                    CreatedAt TEXT NOT NULL
                );";
            await createTableCmd.ExecuteNonQueryAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error creating Course table: {e.Message}");
            throw;
        }

        _hasBeenInitialized = true;
    }

    /// <summary>
    /// Retrieves a list of all courses from the database.
    /// </summary>
    public async Task<List<Course>> ListAsync()
    {
        await Init();
        await using var connection = new SqliteConnection(Constants.DatabasePath);
        await connection.OpenAsync();

        var selectCmd = connection.CreateCommand();
        selectCmd.CommandText = "SELECT * FROM Course ORDER BY Code";
        var courses = new List<Course>();

        await using var reader = await selectCmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            courses.Add(new Course
            {
                ID = reader.GetInt32(0),
                CanvasId = reader.GetString(1),
                Name = reader.GetString(2),
                Code = reader.GetString(3),
                Term = reader.GetString(4),
                AccountId = reader.GetString(5),
                SisCourseId = reader.GetString(6),
                WorkflowState = reader.GetString(7),
                StartAt = DateTime.Parse(reader.GetString(8)),
                EndAt = DateTime.Parse(reader.GetString(9)),
                CreatedAt = DateTime.Parse(reader.GetString(10))
            });
        }

        return courses;
    }

    /// <summary>
    /// Retrieves a specific course by its ID.
    /// </summary>
    public async Task<Course?> GetAsync(int id)
    {
        await Init();
        await using var connection = new SqliteConnection(Constants.DatabasePath);
        await connection.OpenAsync();

        var selectCmd = connection.CreateCommand();
        selectCmd.CommandText = "SELECT * FROM Course WHERE ID = @id";
        selectCmd.Parameters.AddWithValue("@id", id);

        await using var reader = await selectCmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new Course
            {
                ID = reader.GetInt32(0),
                CanvasId = reader.GetString(1),
                Name = reader.GetString(2),
                Code = reader.GetString(3),
                Term = reader.GetString(4),
                AccountId = reader.GetString(5),
                SisCourseId = reader.GetString(6),
                WorkflowState = reader.GetString(7),
                StartAt = DateTime.Parse(reader.GetString(8)),
                EndAt = DateTime.Parse(reader.GetString(9)),
                CreatedAt = DateTime.Parse(reader.GetString(10))
            };
        }

        return null;
    }

    /// <summary>
    /// Gets a course by Canvas ID.
    /// </summary>
    public async Task<Course?> GetByCanvasIdAsync(string canvasId)
    {
        await Init();
        await using var connection = new SqliteConnection(Constants.DatabasePath);
        await connection.OpenAsync();

        var selectCmd = connection.CreateCommand();
        selectCmd.CommandText = "SELECT * FROM Course WHERE CanvasId = @canvasId";
        selectCmd.Parameters.AddWithValue("@canvasId", canvasId);

        await using var reader = await selectCmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new Course
            {
                ID = reader.GetInt32(0),
                CanvasId = reader.GetString(1),
                Name = reader.GetString(2),
                Code = reader.GetString(3),
                Term = reader.GetString(4),
                AccountId = reader.GetString(5),
                SisCourseId = reader.GetString(6),
                WorkflowState = reader.GetString(7),
                StartAt = DateTime.Parse(reader.GetString(8)),
                EndAt = DateTime.Parse(reader.GetString(9)),
                CreatedAt = DateTime.Parse(reader.GetString(10))
            };
        }

        return null;
    }

    /// <summary>
    /// Saves a course to the database. If the course ID is 0, a new course is created; otherwise, the existing course is updated.
    /// </summary>
    public async Task<int> SaveItemAsync(Course item)
    {
        await Init();
        await using var connection = new SqliteConnection(Constants.DatabasePath);
        await connection.OpenAsync();

        var saveCmd = connection.CreateCommand();
        if (item.ID == 0)
        {
            saveCmd.CommandText = @"
                INSERT INTO Course (CanvasId, Name, Code, Term, AccountId, SisCourseId, WorkflowState, StartAt, EndAt, CreatedAt)
                VALUES (@CanvasId, @Name, @Code, @Term, @AccountId, @SisCourseId, @WorkflowState, @StartAt, @EndAt, @CreatedAt);
                SELECT last_insert_rowid();";
        }
        else
        {
            saveCmd.CommandText = @"
                UPDATE Course
                SET CanvasId = @CanvasId, Name = @Name, Code = @Code, Term = @Term, 
                    AccountId = @AccountId, SisCourseId = @SisCourseId, WorkflowState = @WorkflowState,
                    StartAt = @StartAt, EndAt = @EndAt, CreatedAt = @CreatedAt
                WHERE ID = @ID";
            saveCmd.Parameters.AddWithValue("@ID", item.ID);
        }

        saveCmd.Parameters.AddWithValue("@CanvasId", item.CanvasId);
        saveCmd.Parameters.AddWithValue("@Name", item.Name);
        saveCmd.Parameters.AddWithValue("@Code", item.Code);
        saveCmd.Parameters.AddWithValue("@Term", item.Term);
        saveCmd.Parameters.AddWithValue("@AccountId", item.AccountId);
        saveCmd.Parameters.AddWithValue("@SisCourseId", item.SisCourseId);
        saveCmd.Parameters.AddWithValue("@WorkflowState", item.WorkflowState);
        saveCmd.Parameters.AddWithValue("@StartAt", item.StartAt.ToString("O"));
        saveCmd.Parameters.AddWithValue("@EndAt", item.EndAt.ToString("O"));
        saveCmd.Parameters.AddWithValue("@CreatedAt", item.CreatedAt.ToString("O"));

        var result = await saveCmd.ExecuteScalarAsync();
        if (item.ID == 0)
        {
            item.ID = Convert.ToInt32(result);
        }

        return item.ID;
    }

    /// <summary>
    /// Deletes a course from the database.
    /// </summary>
    public async Task<int> DeleteItemAsync(Course item)
    {
        await Init();
        await using var connection = new SqliteConnection(Constants.DatabasePath);
        await connection.OpenAsync();

        var deleteCmd = connection.CreateCommand();
        deleteCmd.CommandText = "DELETE FROM Course WHERE ID = @ID";
        deleteCmd.Parameters.AddWithValue("@ID", item.ID);

        return await deleteCmd.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// Drops the Course table from the database.
    /// </summary>
    public async Task DropTableAsync()
    {
        await Init();
        await using var connection = new SqliteConnection(Constants.DatabasePath);
        await connection.OpenAsync();

        var dropCmd = connection.CreateCommand();
        dropCmd.CommandText = "DROP TABLE IF EXISTS Course";
        await dropCmd.ExecuteNonQueryAsync();

        _hasBeenInitialized = false;
    }
}
