namespace JobPortal.Helpers
{
    /// <summary>
    /// Utility methods for validating and naming uploaded document files.
    /// </summary>
    public static class FileUploadHelper
    {
        private const long MaxFileSizeBytes = 5L * 1024 * 1024; // 5 MB

        private static readonly HashSet<string> AllowedMimeTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "application/pdf",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
        };

        /// <summary>
        /// Validates that the uploaded file is a PDF or DOCX and does not exceed 5 MB.
        /// Returns true when valid; sets errorMessage when invalid.
        /// </summary>
        public static bool IsValidDocument(IFormFile file, out string errorMessage)
        {
            if (!AllowedMimeTypes.Contains(file.ContentType))
            {
                errorMessage = "Only PDF or DOCX files are accepted.";
                return false;
            }

            if (file.Length > MaxFileSizeBytes)
            {
                errorMessage = "File size must not exceed 5 MB.";
                return false;
            }

            errorMessage = string.Empty;
            return true;
        }

        /// <summary>
        /// Generates a unique server-side filename in the format {guid}_{sanitized_original_name}.
        /// Strips any path characters from the original name before constructing the result.
        /// </summary>
        public static string GenerateUniqueFilename(string originalName)
        {
            // Remove any path-traversal characters; keep only the bare filename
            var sanitized = Path.GetFileName(originalName) ?? "upload";
            return $"{Guid.NewGuid():N}_{sanitized}";
        }
    }
}
