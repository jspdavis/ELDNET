using System.Data;
using Dapper;
using JobPortal.Data;
using JobPortal.Models;
using JobPortal.Models.ViewModels;

namespace JobPortal.Services
{
    public class ProfileService : IProfileService
    {
        private readonly DatabaseContext _context;

        public ProfileService(DatabaseContext context)
        {
            _context = context;
        }

        // Returns the full applicant profile joined with users table
        public async Task<ApplicantProfile> GetApplicantProfileAsync(int userId)
        {
            using var db = _context.CreateConnection();
            const string sql = @"
                SELECT p.profile_id AS ProfileId,
                       p.user_id AS UserId,
                       p.phone AS Phone,
                       p.location AS Location,
                       p.bio AS Bio,
                       p.profile_picture_url AS ProfilePictureUrl,
                       u.full_name AS FullName,
                       u.email AS Email
                FROM applicant_profiles p
                INNER JOIN users u ON u.user_id = p.user_id
                WHERE p.user_id = @UserId
                LIMIT 1";
            return await db.QueryFirstOrDefaultAsync<ApplicantProfile>(sql, new { UserId = userId });
        }

        // Updates full_name in users table AND phone, location, bio in applicant_profiles. Wrap both updates in a transaction.
        public async Task UpdateApplicantProfileAsync(int userId, EditApplicantProfileViewModel model)
        {
            using var db = _context.CreateConnection();
            db.Open();
            using var transaction = db.BeginTransaction();
            try
            {
                const string userSql = @"
                    UPDATE users
                    SET full_name = @FullName
                    WHERE user_id = @UserId";
                await db.ExecuteAsync(userSql, new { FullName = model.FullName, UserId = userId }, transaction);

                const string profileSql = @"
                    UPDATE applicant_profiles
                    SET phone = @Phone,
                        location = @Location,
                        bio = @Bio
                    WHERE user_id = @UserId";
                await db.ExecuteAsync(profileSql, new { Phone = model.Phone, Location = model.Location, Bio = model.Bio, UserId = userId }, transaction);

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        // Returns the full company profile joined with users table
        public async Task<CompanyProfile> GetCompanyProfileAsync(int userId)
        {
            using var db = _context.CreateConnection();
            const string sql = @"
                SELECT c.company_id AS CompanyId,
                       c.user_id AS UserId,
                       c.company_name AS CompanyName,
                       c.description AS Description,
                       c.location AS Location,
                       c.website AS Website,
                       c.profile_picture_url AS ProfilePictureUrl,
                       u.full_name AS FullName,
                       u.email AS Email
                FROM companies c
                INNER JOIN users u ON u.user_id = c.user_id
                WHERE c.user_id = @UserId
                LIMIT 1";
            return await db.QueryFirstOrDefaultAsync<CompanyProfile>(sql, new { UserId = userId });
        }

        // Updates full_name in users table AND company_name, description, location, website in companies table. Wrap in a transaction.
        public async Task UpdateCompanyProfileAsync(int userId, EditCompanyProfileViewModel model)
        {
            using var db = _context.CreateConnection();
            db.Open();
            using var transaction = db.BeginTransaction();
            try
            {
                const string userSql = @"
                    UPDATE users
                    SET full_name = @FullName
                    WHERE user_id = @UserId";
                await db.ExecuteAsync(userSql, new { FullName = model.FullName, UserId = userId }, transaction);

                const string companySql = @"
                    UPDATE companies
                    SET company_name = @CompanyName,
                        description = @Description,
                        location = @Location,
                        website = @Website
                    WHERE user_id = @UserId";
                await db.ExecuteAsync(companySql, new 
                { 
                    CompanyName = model.CompanyName, 
                    Description = model.Description, 
                    Location = model.Location, 
                    Website = model.Website, 
                    UserId = userId 
                }, transaction);

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}
