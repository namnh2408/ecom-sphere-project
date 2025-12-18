using BuildingBlocks.Abstractions;
using MediatR;
using Users.Application.Commands.Users;
using Users.Domain.Repositories;

namespace Users.Application.Handlers.CommandHandlers;

public class UploadProfilePictureCommandHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UploadProfilePictureCommand, Result<string>>
{
    private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB
    private static readonly string[] AllowedContentTypes = { "image/jpeg", "image/png", "image/gif", "image/webp" };
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

    public async Task<Result<string>> Handle(UploadProfilePictureCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.UserId);
        if (user == null)
        {
            return Result<string>.Fail("USER_NOT_FOUND", "User not found");
        }

        // Validate file size
        if (request.FileContent.Length > MaxFileSizeBytes)
        {
            return Result<string>.Fail("FILE_TOO_LARGE", $"File size must be less than 5 MB");
        }

        // Validate content type
        if (!AllowedContentTypes.Contains(request.ContentType))
        {
            return Result<string>.Fail("INVALID_CONTENT_TYPE", "Only image files (JPEG, PNG, GIF, WebP) are allowed");
        }

        // Validate file extension
        var extension = Path.GetExtension(request.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
        {
            return Result<string>.Fail("INVALID_FILE_EXTENSION", "Only image files (JPEG, PNG, GIF, WebP) are allowed");
        }

        // Generate file path (can be configured to use cloud storage)
        // For now, using local file system path format
        var fileName = $"{request.UserId}_{DateTime.UtcNow:yyyyMMddHHmmss}{extension}";
        var relativePath = $"uploads/profile-pictures/{fileName}";

        // Here you would save the file to disk or cloud storage
        // For development, we'll just store the path
        user.SetProfilePicture(relativePath);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<string>.Success(relativePath);
    }
}
