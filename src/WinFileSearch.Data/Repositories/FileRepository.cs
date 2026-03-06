using Microsoft.Data.Sqlite;
using WinFileSearch.Data.Models;

namespace WinFileSearch.Data.Repositories;

public class FileRepository : IFileRepository
{
    private readonly FileSearchDbContext _context;

    public FileRepository(FileSearchDbContext context)
    {
        _context = context;
    }

    #region File Operations

    public async Task<long> InsertFileAsync(FileEntry file)
    {
        var connection = _context.GetConnection();
        var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT OR REPLACE INTO Files (FileName, FullPath, Extension, Directory, Size, CreatedAt, ModifiedAt, FolderId, Category)
            VALUES (@fileName, @fullPath, @extension, @directory, @size, @createdAt, @modifiedAt, @folderId, @category);
            SELECT last_insert_rowid();
        ";
        
        command.Parameters.AddWithValue("@fileName", file.FileName);
        command.Parameters.AddWithValue("@fullPath", file.FullPath);
        command.Parameters.AddWithValue("@extension", file.Extension);
        command.Parameters.AddWithValue("@directory", file.Directory);
        command.Parameters.AddWithValue("@size", file.Size);
        command.Parameters.AddWithValue("@createdAt", file.CreatedAt.ToString("o"));
        command.Parameters.AddWithValue("@modifiedAt", file.ModifiedAt.ToString("o"));
        command.Parameters.AddWithValue("@folderId", file.FolderId);
        command.Parameters.AddWithValue("@category", (int)file.Category);
        
        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt64(result);
    }

    /// <summary>
    /// Inserts multiple files using a prepared statement for optimal performance.
    /// Uses synchronous execution within transaction for maximum throughput (~3x faster).
    /// </summary>
    public async Task InsertFilesAsync(IEnumerable<FileEntry> files)
    {
        var connection = _context.GetConnection();
        await using var transaction = (SqliteTransaction)await connection.BeginTransactionAsync();

        try
        {
            // Use prepared statement with typed parameters for better performance
            using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = @"
                INSERT OR REPLACE INTO Files (FileName, FullPath, Extension, Directory, Size, CreatedAt, ModifiedAt, FolderId, Category)
                VALUES ($fileName, $fullPath, $extension, $directory, $size, $createdAt, $modifiedAt, $folderId, $category);
            ";

            var fileNameParam = command.Parameters.Add("$fileName", SqliteType.Text);
            var fullPathParam = command.Parameters.Add("$fullPath", SqliteType.Text);
            var extensionParam = command.Parameters.Add("$extension", SqliteType.Text);
            var directoryParam = command.Parameters.Add("$directory", SqliteType.Text);
            var sizeParam = command.Parameters.Add("$size", SqliteType.Integer);
            var createdAtParam = command.Parameters.Add("$createdAt", SqliteType.Text);
            var modifiedAtParam = command.Parameters.Add("$modifiedAt", SqliteType.Text);
            var folderIdParam = command.Parameters.Add("$folderId", SqliteType.Integer);
            var categoryParam = command.Parameters.Add("$category", SqliteType.Integer);

            // Prepare the command once for reuse
            command.Prepare();

            foreach (var file in files)
            {
                fileNameParam.Value = file.FileName;
                fullPathParam.Value = file.FullPath;
                extensionParam.Value = file.Extension;
                directoryParam.Value = file.Directory;
                sizeParam.Value = file.Size;
                createdAtParam.Value = file.CreatedAt.ToString("o");
                modifiedAtParam.Value = file.ModifiedAt.ToString("o");
                folderIdParam.Value = file.FolderId;
                categoryParam.Value = (int)file.Category;

                command.ExecuteNonQuery(); // Sync is faster for batch operations within transaction
            }

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<FileEntry?> GetFileByPathAsync(string fullPath)
    {
        var connection = _context.GetConnection();
        var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM Files WHERE FullPath = @fullPath";
        command.Parameters.AddWithValue("@fullPath", fullPath);
        
        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapFileEntry(reader);
        }
        return null;
    }

    public async Task<IEnumerable<FileEntry>> SearchFilesAsync(SearchFilter filter)
    {
        var connection = _context.GetConnection();
        var command = connection.CreateCommand();
        
        var results = new List<FileEntry>();
        
        if (!string.IsNullOrWhiteSpace(filter.Query))
        {
            // Use FTS5 for text search
            var searchQuery = filter.Query.Replace("'", "''");
            
            // Support wildcard patterns
            if (searchQuery.Contains('*'))
            {
                searchQuery = searchQuery.Replace("*", "");
            }
            
            command.CommandText = @"
                SELECT f.* FROM Files f
                INNER JOIN FilesSearch fs ON f.Id = fs.rowid
                WHERE FilesSearch MATCH @query
            ";
            
            // Add wildcards for partial matching
            command.Parameters.AddWithValue("@query", $"\"{searchQuery}\"*");
        }
        else
        {
            command.CommandText = "SELECT * FROM Files WHERE 1=1";
        }
        
        // Add filters
        var conditions = new List<string>();
        
        if (filter.Category.HasValue)
        {
            conditions.Add("Category = @category");
            command.Parameters.AddWithValue("@category", (int)filter.Category.Value);
        }
        
        if (filter.ModifiedAfter.HasValue)
        {
            conditions.Add("ModifiedAt >= @modifiedAfter");
            command.Parameters.AddWithValue("@modifiedAfter", filter.ModifiedAfter.Value.ToString("o"));
        }
        
        if (filter.ModifiedBefore.HasValue)
        {
            conditions.Add("ModifiedAt <= @modifiedBefore");
            command.Parameters.AddWithValue("@modifiedBefore", filter.ModifiedBefore.Value.ToString("o"));
        }
        
        if (filter.MinSize.HasValue)
        {
            conditions.Add("Size >= @minSize");
            command.Parameters.AddWithValue("@minSize", filter.MinSize.Value);
        }
        
        if (filter.MaxSize.HasValue)
        {
            conditions.Add("Size <= @maxSize");
            command.Parameters.AddWithValue("@maxSize", filter.MaxSize.Value);
        }
        
        if (!string.IsNullOrEmpty(filter.Location))
        {
            conditions.Add("Directory LIKE @location");
            command.Parameters.AddWithValue("@location", $"%{filter.Location}%");
        }
        
        if (conditions.Count > 0)
        {
            if (command.CommandText.Contains("WHERE FilesSearch"))
            {
                command.CommandText += " AND " + string.Join(" AND ", conditions);
            }
            else
            {
                command.CommandText += " AND " + string.Join(" AND ", conditions);
            }
        }
        
        command.CommandText += $" ORDER BY ModifiedAt DESC LIMIT {filter.MaxResults}";
        
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            results.Add(MapFileEntry(reader));
        }
        
        return results;
    }

    public async Task<IEnumerable<FileEntry>> GetRecentFilesAsync(int count = 20)
    {
        var connection = _context.GetConnection();
        var command = connection.CreateCommand();
        command.CommandText = $"SELECT * FROM Files ORDER BY ModifiedAt DESC LIMIT {count}";
        
        var results = new List<FileEntry>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            results.Add(MapFileEntry(reader));
        }
        return results;
    }

    public async Task DeleteFileAsync(long id)
    {
        var connection = _context.GetConnection();
        var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Files WHERE Id = @id";
        command.Parameters.AddWithValue("@id", id);
        await command.ExecuteNonQueryAsync();
    }

    public async Task DeleteFileByPathAsync(string fullPath)
    {
        var connection = _context.GetConnection();
        var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Files WHERE FullPath = @fullPath";
        command.Parameters.AddWithValue("@fullPath", fullPath);
        await command.ExecuteNonQueryAsync();
    }

    public async Task UpdateFileAsync(FileEntry file)
    {
        var connection = _context.GetConnection();
        var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE Files SET 
                FileName = @fileName,
                Extension = @extension,
                Directory = @directory,
                Size = @size,
                ModifiedAt = @modifiedAt,
                Category = @category
            WHERE Id = @id
        ";
        
        command.Parameters.AddWithValue("@id", file.Id);
        command.Parameters.AddWithValue("@fileName", file.FileName);
        command.Parameters.AddWithValue("@extension", file.Extension);
        command.Parameters.AddWithValue("@directory", file.Directory);
        command.Parameters.AddWithValue("@size", file.Size);
        command.Parameters.AddWithValue("@modifiedAt", file.ModifiedAt.ToString("o"));
        command.Parameters.AddWithValue("@category", (int)file.Category);
        
        await command.ExecuteNonQueryAsync();
    }

    public async Task<long> GetTotalFileCountAsync()
    {
        var connection = _context.GetConnection();
        var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM Files";
        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt64(result);
    }

    public async Task DeleteFilesByFolderIdAsync(long folderId)
    {
        var connection = _context.GetConnection();
        var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Files WHERE FolderId = @folderId";
        command.Parameters.AddWithValue("@folderId", folderId);
        await command.ExecuteNonQueryAsync();
    }

    #endregion

    #region Folder Operations

    public async Task<long> InsertFolderAsync(IndexedFolder folder)
    {
        var connection = _context.GetConnection();
        var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT OR REPLACE INTO IndexedFolders (Path, LastIndexed, FileCount, TotalSize, IsExcluded)
            VALUES (@path, @lastIndexed, @fileCount, @totalSize, @isExcluded);
            SELECT last_insert_rowid();
        ";
        
        command.Parameters.AddWithValue("@path", folder.Path);
        command.Parameters.AddWithValue("@lastIndexed", folder.LastIndexed.ToString("o"));
        command.Parameters.AddWithValue("@fileCount", folder.FileCount);
        command.Parameters.AddWithValue("@totalSize", folder.TotalSize);
        command.Parameters.AddWithValue("@isExcluded", folder.IsExcluded ? 1 : 0);
        
        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt64(result);
    }

    public async Task<IndexedFolder?> GetFolderByPathAsync(string path)
    {
        var connection = _context.GetConnection();
        var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM IndexedFolders WHERE Path = @path";
        command.Parameters.AddWithValue("@path", path);
        
        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapIndexedFolder(reader);
        }
        return null;
    }

    public async Task<IEnumerable<IndexedFolder>> GetAllFoldersAsync(bool includeExcluded = false)
    {
        var connection = _context.GetConnection();
        var command = connection.CreateCommand();
        command.CommandText = includeExcluded 
            ? "SELECT * FROM IndexedFolders ORDER BY Path"
            : "SELECT * FROM IndexedFolders WHERE IsExcluded = 0 ORDER BY Path";
        
        var results = new List<IndexedFolder>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            results.Add(MapIndexedFolder(reader));
        }
        return results;
    }

    public async Task<IEnumerable<IndexedFolder>> GetIncludedFoldersAsync()
    {
        var connection = _context.GetConnection();
        var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM IndexedFolders WHERE IsExcluded = 0 ORDER BY Path";
        
        var results = new List<IndexedFolder>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            results.Add(MapIndexedFolder(reader));
        }
        return results;
    }

    public async Task<IEnumerable<IndexedFolder>> GetExcludedFoldersAsync()
    {
        var connection = _context.GetConnection();
        var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM IndexedFolders WHERE IsExcluded = 1 ORDER BY Path";
        
        var results = new List<IndexedFolder>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            results.Add(MapIndexedFolder(reader));
        }
        return results;
    }

    public async Task UpdateFolderAsync(IndexedFolder folder)
    {
        var connection = _context.GetConnection();
        var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE IndexedFolders SET 
                LastIndexed = @lastIndexed,
                FileCount = @fileCount,
                TotalSize = @totalSize,
                IsExcluded = @isExcluded
            WHERE Id = @id
        ";
        
        command.Parameters.AddWithValue("@id", folder.Id);
        command.Parameters.AddWithValue("@lastIndexed", folder.LastIndexed.ToString("o"));
        command.Parameters.AddWithValue("@fileCount", folder.FileCount);
        command.Parameters.AddWithValue("@totalSize", folder.TotalSize);
        command.Parameters.AddWithValue("@isExcluded", folder.IsExcluded ? 1 : 0);
        
        await command.ExecuteNonQueryAsync();
    }

    public async Task DeleteFolderAsync(long id)
    {
        var connection = _context.GetConnection();
        
        // First delete all files in this folder
        await DeleteFilesByFolderIdAsync(id);
        
        // Then delete the folder
        var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM IndexedFolders WHERE Id = @id";
        command.Parameters.AddWithValue("@id", id);
        await command.ExecuteNonQueryAsync();
    }

    #endregion

    #region Mapping

    private static FileEntry MapFileEntry(SqliteDataReader reader)
    {
        return new FileEntry
        {
            Id = reader.GetInt64(reader.GetOrdinal("Id")),
            FileName = reader.GetString(reader.GetOrdinal("FileName")),
            FullPath = reader.GetString(reader.GetOrdinal("FullPath")),
            Extension = reader.IsDBNull(reader.GetOrdinal("Extension")) ? "" : reader.GetString(reader.GetOrdinal("Extension")),
            Directory = reader.IsDBNull(reader.GetOrdinal("Directory")) ? "" : reader.GetString(reader.GetOrdinal("Directory")),
            Size = reader.IsDBNull(reader.GetOrdinal("Size")) ? 0 : reader.GetInt64(reader.GetOrdinal("Size")),
            CreatedAt = DateTime.TryParse(reader.GetString(reader.GetOrdinal("CreatedAt")), out var created) ? created : DateTime.MinValue,
            ModifiedAt = DateTime.TryParse(reader.GetString(reader.GetOrdinal("ModifiedAt")), out var modified) ? modified : DateTime.MinValue,
            FolderId = reader.IsDBNull(reader.GetOrdinal("FolderId")) ? 0 : reader.GetInt64(reader.GetOrdinal("FolderId")),
            Category = (FileCategory)(reader.IsDBNull(reader.GetOrdinal("Category")) ? 0 : reader.GetInt32(reader.GetOrdinal("Category")))
        };
    }

    private static IndexedFolder MapIndexedFolder(SqliteDataReader reader)
    {
        return new IndexedFolder
        {
            Id = reader.GetInt64(reader.GetOrdinal("Id")),
            Path = reader.GetString(reader.GetOrdinal("Path")),
            LastIndexed = DateTime.TryParse(reader.GetString(reader.GetOrdinal("LastIndexed")), out var indexed) ? indexed : DateTime.MinValue,
            FileCount = reader.IsDBNull(reader.GetOrdinal("FileCount")) ? 0 : reader.GetInt32(reader.GetOrdinal("FileCount")),
            TotalSize = reader.IsDBNull(reader.GetOrdinal("TotalSize")) ? 0 : reader.GetInt64(reader.GetOrdinal("TotalSize")),
            IsExcluded = reader.GetInt32(reader.GetOrdinal("IsExcluded")) == 1
        };
    }

    #endregion
}
