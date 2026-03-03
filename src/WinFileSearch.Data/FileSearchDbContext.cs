using Microsoft.Data.Sqlite;
using WinFileSearch.Data.Models;

namespace WinFileSearch.Data;

/// <summary>
/// SQLite database context with FTS5 full-text search support
/// </summary>
public class FileSearchDbContext : IDisposable
{
    private readonly string _connectionString;
    private SqliteConnection? _connection;
    
    public FileSearchDbContext(string? dbPath = null)
    {
        dbPath ??= Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "WinFileSearch",
            "fileindex.db");
        
        var directory = Path.GetDirectoryName(dbPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        
        _connectionString = $"Data Source={dbPath}";
    }
    
    public SqliteConnection GetConnection()
    {
        if (_connection == null)
        {
            _connection = new SqliteConnection(_connectionString);
            _connection.Open();
        }
        return _connection;
    }
    
    public async Task InitializeDatabaseAsync()
    {
        var connection = GetConnection();
        
        // Create main tables
        var createTablesCommand = connection.CreateCommand();
        createTablesCommand.CommandText = @"
            -- Indexed folders table
            CREATE TABLE IF NOT EXISTS IndexedFolders (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Path TEXT UNIQUE NOT NULL,
                LastIndexed TEXT,
                FileCount INTEGER DEFAULT 0,
                TotalSize INTEGER DEFAULT 0,
                IsExcluded INTEGER DEFAULT 0
            );
            
            -- Files table
            CREATE TABLE IF NOT EXISTS Files (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                FileName TEXT NOT NULL,
                FullPath TEXT UNIQUE NOT NULL,
                Extension TEXT,
                Directory TEXT,
                Size INTEGER,
                CreatedAt TEXT,
                ModifiedAt TEXT,
                FolderId INTEGER,
                Category INTEGER DEFAULT 0,
                FOREIGN KEY (FolderId) REFERENCES IndexedFolders(Id) ON DELETE CASCADE
            );
            
            -- Create indexes for faster queries
            CREATE INDEX IF NOT EXISTS idx_files_filename ON Files(FileName);
            CREATE INDEX IF NOT EXISTS idx_files_extension ON Files(Extension);
            CREATE INDEX IF NOT EXISTS idx_files_directory ON Files(Directory);
            CREATE INDEX IF NOT EXISTS idx_files_category ON Files(Category);
            CREATE INDEX IF NOT EXISTS idx_files_modified ON Files(ModifiedAt);
        ";
        await createTablesCommand.ExecuteNonQueryAsync();
        
        // Create FTS5 virtual table for fast full-text search
        var createFtsCommand = connection.CreateCommand();
        createFtsCommand.CommandText = @"
            CREATE VIRTUAL TABLE IF NOT EXISTS FilesSearch USING fts5(
                FileName,
                FullPath,
                content='Files',
                content_rowid='Id',
                tokenize='unicode61 remove_diacritics 1'
            );
        ";
        await createFtsCommand.ExecuteNonQueryAsync();
        
        // Create triggers to keep FTS index in sync
        var createTriggersCommand = connection.CreateCommand();
        createTriggersCommand.CommandText = @"
            -- Trigger for INSERT
            CREATE TRIGGER IF NOT EXISTS files_ai AFTER INSERT ON Files BEGIN
                INSERT INTO FilesSearch(rowid, FileName, FullPath) 
                VALUES (new.Id, new.FileName, new.FullPath);
            END;
            
            -- Trigger for DELETE
            CREATE TRIGGER IF NOT EXISTS files_ad AFTER DELETE ON Files BEGIN
                INSERT INTO FilesSearch(FilesSearch, rowid, FileName, FullPath) 
                VALUES('delete', old.Id, old.FileName, old.FullPath);
            END;
            
            -- Trigger for UPDATE
            CREATE TRIGGER IF NOT EXISTS files_au AFTER UPDATE ON Files BEGIN
                INSERT INTO FilesSearch(FilesSearch, rowid, FileName, FullPath) 
                VALUES('delete', old.Id, old.FileName, old.FullPath);
                INSERT INTO FilesSearch(rowid, FileName, FullPath) 
                VALUES (new.Id, new.FileName, new.FullPath);
            END;
        ";
        await createTriggersCommand.ExecuteNonQueryAsync();
    }
    
    public async Task RebuildFtsIndexAsync()
    {
        var connection = GetConnection();
        var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO FilesSearch(FilesSearch) VALUES('rebuild');
        ";
        await command.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// Clears all files but preserves the indexed folders list
    /// </summary>
    public async Task ClearFilesOnlyAsync()
    {
        var connection = GetConnection();
        var command = connection.CreateCommand();
        command.CommandText = @"
            DELETE FROM Files;
            DELETE FROM FilesSearch;
            UPDATE IndexedFolders SET FileCount = 0, TotalSize = 0;
        ";
        await command.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// Clears all data including folders
    /// </summary>
    public async Task ClearAllDataAsync()
    {
        var connection = GetConnection();
        var command = connection.CreateCommand();
        command.CommandText = @"
            DELETE FROM Files;
            DELETE FROM IndexedFolders;
            DELETE FROM FilesSearch;
        ";
        await command.ExecuteNonQueryAsync();
    }
    
    public void Dispose()
    {
        _connection?.Close();
        _connection?.Dispose();
        _connection = null;
    }
}
