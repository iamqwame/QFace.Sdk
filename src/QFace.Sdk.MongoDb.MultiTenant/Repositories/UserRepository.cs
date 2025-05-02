namespace QFace.Sdk.MongoDb.MultiTenant.Repositories;

/// <summary>
    /// Interface for user repository
    /// </summary>
    public interface IUserRepository : IMongoRepository<User>
    {
        /// <summary>
        /// Gets a user by email
        /// </summary>
        /// <param name="email">The user's email</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The user or null if not found</returns>
        Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets a user by username
        /// </summary>
        /// <param name="username">The username</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The user or null if not found</returns>
        Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Checks if an email is already registered
        /// </summary>
        /// <param name="email">The email to check</param>
        /// <param name="excludeUserId">Optional user ID to exclude from check</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if email exists, false otherwise</returns>
        Task<bool> EmailExistsAsync(string email, string? excludeUserId = null, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Checks if a username is already registered
        /// </summary>
        /// <param name="username">The username to check</param>
        /// <param name="excludeUserId">Optional user ID to exclude from check</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if username exists, false otherwise</returns>
        Task<bool> UsernameExistsAsync(string username, string? excludeUserId = null, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Updates a user's password
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="passwordHash">The new password hash</param>
        /// <param name="passwordSalt">The new password salt</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> UpdatePasswordAsync(string userId, string passwordHash, string passwordSalt, CancellationToken cancellationToken = default);
    }
    
    
    /// <summary>
    /// Implementation of user repository
    /// </summary>
    public class UserRepository : MongoRepository<User>, IUserRepository
    {
        /// <summary>
        /// Creates a new user repository
        /// </summary>
        public UserRepository(
            IMongoDatabase database,
            string collectionName,
            ILogger<UserRepository> logger)
            : base(database, collectionName, logger)
        {
        }
        
        /// <summary>
        /// Gets a user by email
        /// </summary>
        public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(email))
                return null;
                
            return await FindOneAsync(u => u.Email == email.ToLowerInvariant(), cancellationToken: cancellationToken);
        }
        
        /// <summary>
        /// Gets a user by username
        /// </summary>
        public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(username))
                return null;
                
            return await FindOneAsync(u => u.Username == username.ToLowerInvariant(), cancellationToken: cancellationToken);
        }
        
        /// <summary>
        /// Checks if an email is already registered
        /// </summary>
        public async Task<bool> EmailExistsAsync(string email, string? excludeUserId = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(email))
                return false;
                
            var normalizedEmail = email.ToLowerInvariant();
            
            Expression<Func<User, bool>> filter;
            if (string.IsNullOrEmpty(excludeUserId))
            {
                filter = u => u.Email == normalizedEmail;
            }
            else
            {
                filter = u => u.Email == normalizedEmail && u.Id != excludeUserId;
            }
            
            var count = await CountAsync(filter, cancellationToken: cancellationToken);
            return count > 0;
        }
        
        /// <summary>
        /// Checks if a username is already registered
        /// </summary>
        public async Task<bool> UsernameExistsAsync(string username, string? excludeUserId = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(username))
                return false;
                
            var normalizedUsername = username.ToLowerInvariant();
            
            Expression<Func<User, bool>> filter;
            if (string.IsNullOrEmpty(excludeUserId))
            {
                filter = u => u.Username == normalizedUsername;
            }
            else
            {
                filter = u => u.Username == normalizedUsername && u.Id != excludeUserId;
            }
            
            var count = await CountAsync(filter, cancellationToken: cancellationToken);
            return count > 0;
        }
        
        /// <summary>
        /// Updates a user's password
        /// </summary>
        public async Task<bool> UpdatePasswordAsync(string userId, string passwordHash, string passwordSalt, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(userId))
                return false;
                
            var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
            var update = Builders<User>.Update
                .Set(u => u.PasswordHash, passwordHash)
                .Set(u => u.PasswordSalt, passwordSalt)
                .Set(u => u.LastPasswordChangeDate, DateTime.UtcNow)
                .Set(u => u.LastModifiedDate, DateTime.UtcNow);
                
            var result = await _collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
            return result.ModifiedCount > 0;
        }
        
        /// <summary>
        /// Override to create custom indexes
        /// </summary>
        public override async Task CreateIndexesAsync(CancellationToken cancellationToken = default)
        {
            // Create standard indexes from base class
            await base.CreateIndexesAsync(cancellationToken);
            
            // Create unique index on Email
            var emailIndexModel = new CreateIndexModel<User>(
                Builders<User>.IndexKeys.Ascending(u => u.Email),
                new CreateIndexOptions { Unique = true, Background = true, Name = "email_unique_idx" }
            );
            
            await _collection.Indexes.CreateOneAsync(emailIndexModel, cancellationToken: cancellationToken);
            
            // Create unique index on Username
            var usernameIndexModel = new CreateIndexModel<User>(
                Builders<User>.IndexKeys.Ascending(u => u.Username),
                new CreateIndexOptions { Unique = true, Background = true, Name = "username_unique_idx" }
            );
            
            await _collection.Indexes.CreateOneAsync(usernameIndexModel, cancellationToken: cancellationToken);
        }
    }
    
    
    /// <summary>
    /// Interface for refresh token repository
    /// </summary>
    public interface IRefreshTokenRepository : IMongoRepository<RefreshToken>
    {
        /// <summary>
        /// Gets a refresh token by its value
        /// </summary>
        /// <param name="token">The token value</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The refresh token or null if not found</returns>
        Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets active refresh tokens for a user
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="tenantId">Optional tenant ID to filter by</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of active refresh tokens</returns>
        Task<IEnumerable<RefreshToken>> GetActiveTokensForUserAsync(
            string userId, 
            string? tenantId = null, 
            CancellationToken cancellationToken = default);
            
        /// <summary>
        /// Revokes a refresh token
        /// </summary>
        /// <param name="token">The token value</param>
        /// <param name="reason">Reason for revocation</param>
        /// <param name="replacedByToken">Optional replacement token</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> RevokeTokenAsync(
            string token, 
            string reason, 
            string? replacedByToken = null, 
            CancellationToken cancellationToken = default);
            
        /// <summary>
        /// Revokes all active tokens for a user
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="reason">Reason for revocation</param>
        /// <param name="tenantId">Optional tenant ID to limit scope</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Number of tokens revoked</returns>
        Task<int> RevokeAllUserTokensAsync(
            string userId, 
            string reason, 
            string? tenantId = null, 
            CancellationToken cancellationToken = default);
            
        /// <summary>
        /// Purges expired tokens
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Number of tokens purged</returns>
        Task<int> PurgeExpiredTokensAsync(CancellationToken cancellationToken = default);
    }
    
    /// <summary>
    /// Implementation of refresh token repository
    /// </summary>
    public class RefreshTokenRepository : MongoRepository<RefreshToken>, IRefreshTokenRepository
    {
        /// <summary>
        /// Creates a new refresh token repository
        /// </summary>
        public RefreshTokenRepository(
            IMongoDatabase database,
            string collectionName,
            ILogger<RefreshTokenRepository> logger)
            : base(database, collectionName, logger)
        {
        }
        
        /// <summary>
        /// Gets a refresh token by its value
        /// </summary>
        public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(token))
                return null;
                
            return await FindOneAsync(t => t.Token == token, cancellationToken: cancellationToken);
        }
        
        /// <summary>
        /// Gets active refresh tokens for a user
        /// </summary>
        public async Task<IEnumerable<RefreshToken>> GetActiveTokensForUserAsync(
            string userId, 
            string? tenantId = null, 
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(userId))
                return Enumerable.Empty<RefreshToken>();
                
            Expression<Func<RefreshToken, bool>> filter;
            
            if (string.IsNullOrEmpty(tenantId))
            {
                filter = t => t.UserId == userId && !t.IsRevoked && t.ExpiresAt > DateTime.UtcNow;
            }
            else
            {
                filter = t => t.UserId == userId && t.TenantId == tenantId && !t.IsRevoked && t.ExpiresAt > DateTime.UtcNow;
            }
            
            return await FindAsync(filter, cancellationToken: cancellationToken);
        }
        
        /// <summary>
        /// Revokes a refresh token
        /// </summary>
        public async Task<bool> RevokeTokenAsync(
            string token, 
            string reason, 
            string? replacedByToken = null, 
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(token))
                return false;
                
            var filter = Builders<RefreshToken>.Filter.Eq(t => t.Token, token);
            var update = Builders<RefreshToken>.Update
                .Set(t => t.IsRevoked, true)
                .Set(t => t.RevokedAt, DateTime.UtcNow)
                .Set(t => t.RevocationReason, reason)
                .Set(t => t.LastModifiedDate, DateTime.UtcNow);
                
            if (!string.IsNullOrEmpty(replacedByToken))
            {
                update = update.Set(t => t.ReplacedByToken, replacedByToken);
            }
            
            var result = await _collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
            return result.ModifiedCount > 0;
        }
        
        /// <summary>
        /// Revokes all active tokens for a user
        /// </summary>
        public async Task<int> RevokeAllUserTokensAsync(
            string userId, 
            string reason, 
            string? tenantId = null, 
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(userId))
                return 0;
                
            var now = DateTime.UtcNow;
            
            FilterDefinition<RefreshToken> filter;
            if (string.IsNullOrEmpty(tenantId))
            {
                filter = Builders<RefreshToken>.Filter.And(
                    Builders<RefreshToken>.Filter.Eq(t => t.UserId, userId),
                    Builders<RefreshToken>.Filter.Eq(t => t.IsRevoked, false),
                    Builders<RefreshToken>.Filter.Gt(t => t.ExpiresAt, now)
                );
            }
            else
            {
                filter = Builders<RefreshToken>.Filter.And(
                    Builders<RefreshToken>.Filter.Eq(t => t.UserId, userId),
                    Builders<RefreshToken>.Filter.Eq(t => t.TenantId, tenantId),
                    Builders<RefreshToken>.Filter.Eq(t => t.IsRevoked, false),
                    Builders<RefreshToken>.Filter.Gt(t => t.ExpiresAt, now)
                );
            }
            
            var update = Builders<RefreshToken>.Update
                .Set(t => t.IsRevoked, true)
                .Set(t => t.RevokedAt, now)
                .Set(t => t.RevocationReason, reason)
                .Set(t => t.LastModifiedDate, now);
                
            var result = await _collection.UpdateManyAsync(filter, update, cancellationToken: cancellationToken);
            return (int)result.ModifiedCount;
        }
        
        /// <summary>
        /// Purges expired tokens
        /// </summary>
        public async Task<int> PurgeExpiredTokensAsync(CancellationToken cancellationToken = default)
        {
            var cutoff = DateTime.UtcNow.AddDays(-30); // Keep expired tokens for 30 days
            
            var filter = Builders<RefreshToken>.Filter.Lt(t => t.ExpiresAt, cutoff);
            var result = await _collection.DeleteManyAsync(filter, cancellationToken);
            
            return (int)result.DeletedCount;
        }
        
        /// <summary>
        /// Override to create custom indexes
        /// </summary>
        public override async Task CreateIndexesAsync(CancellationToken cancellationToken = default)
        {
            // Create standard indexes from base class
            await base.CreateIndexesAsync(cancellationToken);
            
            // Create index on Token
            var tokenIndexModel = new CreateIndexModel<RefreshToken>(
                Builders<RefreshToken>.IndexKeys.Ascending(t => t.Token),
                new CreateIndexOptions { Unique = true, Background = true, Name = "token_unique_idx" }
            );
            
            await _collection.Indexes.CreateOneAsync(tokenIndexModel, cancellationToken: cancellationToken);
            
            // Create index on UserId
            var userIdIndexModel = new CreateIndexModel<RefreshToken>(
                Builders<RefreshToken>.IndexKeys.Ascending(t => t.UserId),
                new CreateIndexOptions { Background = true, Name = "user_id_idx" }
            );
            
            await _collection.Indexes.CreateOneAsync(userIdIndexModel, cancellationToken: cancellationToken);
            
            // Create compound index on UserId and TenantId
            var userTenantIndexModel = new CreateIndexModel<RefreshToken>(
                Builders<RefreshToken>.IndexKeys
                    .Ascending(t => t.UserId)
                    .Ascending(t => t.TenantId),
                new CreateIndexOptions { Background = true, Name = "user_tenant_idx" }
            );
            
            await _collection.Indexes.CreateOneAsync(userTenantIndexModel, cancellationToken: cancellationToken);
            
            // Create index on ExpiresAt for purging
            var expiryIndexModel = new CreateIndexModel<RefreshToken>(
                Builders<RefreshToken>.IndexKeys.Ascending(t => t.ExpiresAt),
                new CreateIndexOptions { Background = true, Name = "expiry_idx" }
            );
            
            await _collection.Indexes.CreateOneAsync(expiryIndexModel, cancellationToken: cancellationToken);
        }
    }