using Dapper;
using JobPortal.Data;
using JobPortal.Models;

namespace JobPortal.Services
{
    /// <summary>
    /// Handles all user-related database operations using Dapper.
    /// Passwords are hashed with BCrypt before storing.
    /// </summary>
    public class UserService : IUserService
    {
        private readonly DatabaseContext _context;

        public UserService(DatabaseContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Fetches a single user by their email address.
        /// Returns null if no user is found (used during login).
        /// </summary>
        public async Task<User?> GetByEmailAsync(string email)
        {
            using var db = _context.CreateConnection();

            // Select user where email matches (case-insensitive by MySQL default)
            const string sql = @"
                SELECT user_id   AS UserId,
                       email     AS Email,
                       password_hash AS PasswordHash,
                       role      AS Role,
                       full_name AS FullName,
                       created_at AS CreatedAt
                FROM users
                WHERE email = @Email
                LIMIT 1";

            return await db.QueryFirstOrDefaultAsync<User>(sql, new { Email = email });
        }

        /// <summary>
        /// Fetches a user by their primary key.
        /// Used to reload user info from session userId.
        /// </summary>
        public async Task<User?> GetByIdAsync(int userId)
        {
            using var db = _context.CreateConnection();

            // Select user by primary key
            const string sql = @"
                SELECT user_id   AS UserId,
                       email     AS Email,
                       password_hash AS PasswordHash,
                       role      AS Role,
                       full_name AS FullName,
                       created_at AS CreatedAt
                FROM users
                WHERE user_id = @UserId
                LIMIT 1";

            return await db.QueryFirstOrDefaultAsync<User>(sql, new { UserId = userId });
        }

        /// <summary>
        /// Inserts a new user record into the database.
        /// The plain-text password is hashed with BCrypt before storage.
        /// Returns the newly created user_id.
        /// </summary>
        public async Task<int> CreateUserAsync(string email, string password, string role, string fullName)
        {
            using var db = _context.CreateConnection();

            // Hash the password using BCrypt (work factor 11 for security without being too slow)
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 11);

            // Insert the user and return the new auto-incremented ID
            const string sql = @"
                INSERT INTO users (email, password_hash, role, full_name, created_at)
                VALUES (@Email, @PasswordHash, @Role, @FullName, NOW());
                SELECT LAST_INSERT_ID();";

            return await db.ExecuteScalarAsync<int>(sql, new
            {
                Email = email,
                PasswordHash = passwordHash,
                Role = role,
                FullName = fullName
            });
        }

        /// <summary>
        /// Creates a company profile linked to the given user.
        /// Called immediately after a company user registers.
        /// </summary>
        public async Task CreateCompanyProfileAsync(int userId, string companyName, string description, string location, string website)
        {
            using var db = _context.CreateConnection();

            // Insert a new company row tied to the user_id
            const string sql = @"
                INSERT INTO companies (user_id, company_name, description, location, website)
                VALUES (@UserId, @CompanyName, @Description, @Location, @Website)";

            await db.ExecuteAsync(sql, new
            {
                UserId = userId,
                CompanyName = companyName,
                Description = description,
                Location = location,
                Website = website
            });
        }

        /// <summary>
        /// Updates an existing company profile.
        /// Called from the Company Profile edit page.
        /// </summary>
        public async Task UpdateCompanyProfileAsync(int companyId, string companyName, string description, string location, string website)
        {
            using var db = _context.CreateConnection();

            // Update all editable company fields by company_id
            const string sql = @"
                UPDATE companies
                SET company_name = @CompanyName,
                    description  = @Description,
                    location     = @Location,
                    website      = @Website
                WHERE company_id = @CompanyId";

            await db.ExecuteAsync(sql, new
            {
                CompanyId = companyId,
                CompanyName = companyName,
                Description = description,
                Location = location,
                Website = website
            });
        }

        /// <summary>
        /// Returns the company profile for a given user_id.
        /// Returns null if the user has no company profile.
        /// </summary>
        public async Task<Company?> GetCompanyByUserIdAsync(int userId)
        {
            using var db = _context.CreateConnection();

            // Join companies with users to get owner info alongside company details
            const string sql = @"
                SELECT c.company_id   AS CompanyId,
                       c.user_id      AS UserId,
                       c.company_name AS CompanyName,
                       c.description  AS Description,
                       c.location     AS Location,
                       c.website      AS Website,
                       u.email        AS OwnerEmail,
                       u.full_name    AS OwnerFullName
                FROM companies c
                INNER JOIN users u ON u.user_id = c.user_id
                WHERE c.user_id = @UserId
                LIMIT 1";

            return await db.QueryFirstOrDefaultAsync<Company>(sql, new { UserId = userId });
        }
    }
}
