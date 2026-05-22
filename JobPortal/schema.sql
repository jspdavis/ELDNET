-- ============================================================
-- Job Portal System - MySQL Schema
-- Run this file in MySQL to set up the database.
-- ============================================================

CREATE DATABASE IF NOT EXISTS job_portal CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE job_portal;

-- ============================================================
-- TABLE: users
-- Stores both company and applicant accounts.
-- ============================================================
CREATE TABLE IF NOT EXISTS users (
    user_id       INT AUTO_INCREMENT PRIMARY KEY,
    email         VARCHAR(255) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL,
    role          ENUM('company', 'applicant') NOT NULL,
    full_name     VARCHAR(150) NOT NULL,
    created_at    DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- ============================================================
-- TABLE: companies
-- Extended profile for users with role = 'company'.
-- ============================================================
CREATE TABLE IF NOT EXISTS companies (
    company_id   INT AUTO_INCREMENT PRIMARY KEY,
    user_id      INT NOT NULL,
    company_name VARCHAR(200) NOT NULL,
    description  TEXT,
    location     VARCHAR(200),
    website      VARCHAR(255),
    profile_picture_url VARCHAR(500),
    FOREIGN KEY (user_id) REFERENCES users(user_id) ON DELETE CASCADE
);

-- ============================================================
-- TABLE: applicant_profiles
-- Extended profile data for applicant users
-- ============================================================
CREATE TABLE IF NOT EXISTS applicant_profiles (
    profile_id     INT AUTO_INCREMENT PRIMARY KEY,
    user_id        INT NOT NULL UNIQUE,
    phone          VARCHAR(20),
    location       VARCHAR(200),
    bio            TEXT,
    profile_picture_url VARCHAR(500),
    FOREIGN KEY (user_id) REFERENCES users(user_id) ON DELETE CASCADE
);

-- ============================================================
-- TABLE: job_categories
-- Lookup table for job categories (Engineering, Design, etc.)
-- ============================================================
CREATE TABLE IF NOT EXISTS job_categories (
    category_id   INT AUTO_INCREMENT PRIMARY KEY,
    category_name VARCHAR(100) NOT NULL
);

-- ============================================================
-- TABLE: job_postings
-- Jobs posted by companies.
-- is_active = 0 means soft-deleted (not shown publicly).
-- ============================================================
CREATE TABLE IF NOT EXISTS job_postings (
    job_id       INT AUTO_INCREMENT PRIMARY KEY,
    company_id   INT NOT NULL,
    category_id  INT NOT NULL,
    title        VARCHAR(255) NOT NULL,
    description  TEXT NOT NULL,
    requirements TEXT,
    location     VARCHAR(200),
    salary_range VARCHAR(100),
    is_active    BOOLEAN NOT NULL DEFAULT TRUE,
    posted_at    DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (company_id)  REFERENCES companies(company_id)    ON DELETE CASCADE,
    FOREIGN KEY (category_id) REFERENCES job_categories(category_id) ON DELETE RESTRICT
);

-- ============================================================
-- TABLE: applications
-- Applicants apply to job postings.
-- status tracks the hiring pipeline stage.
-- ============================================================
CREATE TABLE IF NOT EXISTS applications (
    application_id INT AUTO_INCREMENT PRIMARY KEY,
    job_id         INT NOT NULL,
    user_id        INT NOT NULL,
    cover_letter   TEXT,
    status         ENUM('pending', 'reviewed', 'accepted', 'rejected') NOT NULL DEFAULT 'pending',
    applied_at     DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (job_id)  REFERENCES job_postings(job_id) ON DELETE CASCADE,
    FOREIGN KEY (user_id) REFERENCES users(user_id)       ON DELETE CASCADE,
    UNIQUE KEY unique_application (job_id, user_id)       -- one application per job per user
);

-- ============================================================
-- SAMPLE DATA
-- ============================================================

-- Sample users (passwords are BCrypt hashes of "Password123!")
INSERT INTO users (email, password_hash, role, full_name, created_at) VALUES
('techcorp@example.com',    '$2a$11$ZJjOyYc/3gVAa8GRRg5TM.2UVoPYp/1lpnx0O6HFdujuIStRfHWzW', 'company',   'TechCorp Inc.',        '2025-01-10 09:00:00'),
('designhub@example.com',   '$2a$11$ZJjOyYc/3gVAa8GRRg5TM.2UVoPYp/1lpnx0O6HFdujuIStRfHWzW', 'company',   'Design Hub Ltd.',      '2025-01-12 10:30:00'),
('alice@example.com',       '$2a$11$ZJjOyYc/3gVAa8GRRg5TM.2UVoPYp/1lpnx0O6HFdujuIStRfHWzW', 'applicant', 'Alice Santos',         '2025-01-15 08:00:00'),
('bob@example.com',         '$2a$11$ZJjOyYc/3gVAa8GRRg5TM.2UVoPYp/1lpnx0O6HFdujuIStRfHWzW', 'applicant', 'Bob Reyes',            '2025-01-18 11:00:00'),
('marketpro@example.com',   '$2a$11$ZJjOyYc/3gVAa8GRRg5TM.2UVoPYp/1lpnx0O6HFdujuIStRfHWzW', 'company',   'MarketPro Solutions',  '2025-01-20 14:00:00'),
('carlos@example.com',      '$2a$11$ZJjOyYc/3gVAa8GRRg5TM.2UVoPYp/1lpnx0O6HFdujuIStRfHWzW', 'applicant', 'Carlos Mendoza',       '2025-01-22 09:30:00'),
('diana@example.com',       '$2a$11$ZJjOyYc/3gVAa8GRRg5TM.2UVoPYp/1lpnx0O6HFdujuIStRfHWzW', 'applicant', 'Diana Lim',            '2025-01-25 10:00:00');

-- Sample company profiles
INSERT INTO companies (user_id, company_name, description, location, website) VALUES
(1, 'TechCorp Inc.',       'A leading software development company building enterprise solutions.', 'Makati, Metro Manila',    'https://techcorp.example.com'),
(2, 'Design Hub Ltd.',     'A creative agency specializing in UI/UX and brand identity.',           'BGC, Taguig',            'https://designhub.example.com'),
(5, 'MarketPro Solutions', 'Full-service digital marketing agency focused on growth.',              'Ortigas, Pasig City',    'https://marketpro.example.com');

-- Sample job categories
INSERT INTO job_categories (category_name) VALUES
('Engineering'),
('Design'),
('Marketing'),
('Finance'),
('Operations');

-- Sample job postings
INSERT INTO job_postings (company_id, category_id, title, description, requirements, location, salary_range, is_active, posted_at) VALUES
(1, 1, 'Senior Backend Developer',
 'Join our core engineering team to build scalable REST APIs and microservices for our enterprise clients.',
 'At least 3 years experience in C# or Java. Experience with Docker and Kubernetes is a plus.',
 'Makati, Metro Manila', '₱80,000 – ₱120,000/month', TRUE, '2025-02-01 09:00:00'),

(1, 1, 'Junior Frontend Developer',
 'Work alongside senior developers to build responsive web interfaces using React and TypeScript.',
 'Graduate of Computer Science or related field. Knowledge of HTML, CSS, JavaScript required.',
 'Remote', '₱30,000 – ₱50,000/month', TRUE, '2025-02-05 10:00:00'),

(2, 2, 'UI/UX Designer',
 'Design intuitive user experiences for mobile and web applications. Collaborate with product and dev teams.',
 'Proficient in Figma or Adobe XD. Portfolio of at least 3 live projects required.',
 'BGC, Taguig', '₱45,000 – ₱70,000/month', TRUE, '2025-02-08 11:00:00'),

(3, 3, 'Digital Marketing Specialist',
 'Plan and execute SEO, SEM, and social media campaigns for our diverse client portfolio.',
 'At least 2 years of digital marketing experience. Google Ads and Meta Ads certification preferred.',
 'Ortigas, Pasig City', '₱35,000 – ₱55,000/month', TRUE, '2025-02-10 09:30:00'),

(3, 3, 'Content Writer',
 'Create compelling blog posts, ad copy, and email campaigns that drive engagement and conversions.',
 'Excellent written English. Experience in SEO writing is an advantage.',
 'Remote', '₱20,000 – ₱35,000/month', TRUE, '2025-02-12 14:00:00'),

(1, 4, 'Accounting Analyst',
 'Handle financial reporting, accounts reconciliation, and budget monitoring for our finance team.',
 'CPA or accounting graduate. 1–2 years relevant experience preferred.',
 'Makati, Metro Manila', '₱40,000 – ₱60,000/month', TRUE, '2025-02-15 08:00:00'),

(2, 2, 'Graphic Designer',
 'Create visual assets for social media, print, and digital advertising campaigns.',
 'Proficient in Adobe Photoshop and Illustrator. Strong eye for layout and typography.',
 'BGC, Taguig', '₱28,000 – ₱42,000/month', TRUE, '2025-02-18 13:00:00');

-- Sample applications
INSERT INTO applications (job_id, user_id, cover_letter, status, applied_at) VALUES
(1, 3, 'I am a passionate backend developer with 4 years of experience in C# and ASP.NET. I believe I can be a great addition to your team.', 'reviewed',  '2025-02-10 10:00:00'),
(2, 3, 'I have built several React projects and I am eager to grow as a frontend developer at TechCorp.', 'pending',   '2025-02-11 09:00:00'),
(3, 4, 'As a UI/UX Designer with a strong Figma portfolio, I am excited about the opportunity to work at Design Hub.', 'accepted',  '2025-02-12 11:30:00'),
(4, 6, 'I have 3 years in digital marketing and have managed Google Ads campaigns exceeding ₱500k monthly budgets.', 'pending',   '2025-02-13 14:00:00'),
(5, 7, 'Writing is my passion. I specialize in SEO-optimized content and have published over 200 articles online.', 'reviewed',  '2025-02-14 08:30:00'),
(1, 6, 'I am transitioning from a QA role into backend development and have completed several personal projects in Java.', 'rejected',  '2025-02-15 10:00:00'),
(7, 4, 'I am a Graphic Design graduate with a strong portfolio across print and digital media.', 'pending',   '2025-02-20 09:00:00');


-- ============================================================
-- ADD ADMIN ROLE TO USERS TABLE
-- ============================================================

ALTER TABLE users 
MODIFY COLUMN role ENUM('company', 'applicant', 'admin') NOT NULL;

-- ============================================================
-- DEFAULT ADMIN ACCOUNT
-- Email: admin@jobportal.com
-- Password: Admin@1234
-- ============================================================

INSERT INTO users (email, password_hash, role, full_name, created_at)
VALUES (
    'admin@jobportal.com',
    '$2a$11$tk0tUJL0aDwzU0hwyEpVoeMAbrdYgZB4pRPLhymbFgiC.zCxAKr16',
    'admin',
    'System Administrator',
    NOW()
);

-- ============================================================
-- FILE UPLOAD COLUMNS
-- resume_filename: server-side UUID-based stored filename
-- resume_original_name: original filename the user uploaded
-- resume_uploaded_at: timestamp of the upload
-- ============================================================

ALTER TABLE applicant_profiles
    ADD COLUMN IF NOT EXISTS resume_filename      VARCHAR(500),
    ADD COLUMN IF NOT EXISTS resume_original_name VARCHAR(500),
    ADD COLUMN IF NOT EXISTS resume_uploaded_at   DATETIME;

-- cover_letter_filename: server-side stored filename for uploaded PDF
-- cover_letter_original_name: original PDF filename the applicant uploaded
-- NOTE: the existing cover_letter TEXT column remains for typed cover letters.
-- Both typed and uploaded options are valid; neither is required.

ALTER TABLE applications
    ADD COLUMN IF NOT EXISTS cover_letter_filename      VARCHAR(500),
    ADD COLUMN IF NOT EXISTS cover_letter_original_name VARCHAR(500);