using JobPortal.Models;

namespace JobPortal.Services
{
    /// <summary>
    /// Contract for user authentication and registration operations.
    /// </summary>
    public interface IUserService
    {
        /// <summary>Returns a User by email, or null if not found.</summary>
        Task<User?> GetByEmailAsync(string email);

        /// <summary>Creates a new user account (hashes password internally).</summary>
        Task<int> CreateUserAsync(string email, string password, string role, string fullName);

        /// <summary>Returns a User by their primary key.</summary>
        Task<User?> GetByIdAsync(int userId);

        /// <summary>Creates a company profile linked to a user.</summary>
        Task CreateCompanyProfileAsync(int userId, string companyName, string description, string location, string website);

        /// <summary>Updates an existing company profile.</summary>
        Task UpdateCompanyProfileAsync(int companyId, string companyName, string description, string location, string website);

        /// <summary>Retrieves the company profile for a given user.</summary>
        Task<Company?> GetCompanyByUserIdAsync(int userId);
    }
}
