using AutoMapper;
using Biblioteca.Application.DTOs.Auth;
using Biblioteca.Application.Exceptions;
using Biblioteca.Application.Interfaces.Auth;
using Biblioteca.Application.Interfaces.Common;
using Biblioteca.Application.Interfaces.Usuarios;
using Biblioteca.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Biblioteca.Application.Services.Auth;

public sealed class AuthService(
    IUsuarioRepository usuarioRepository,
    IPasswordHasher<Usuario> passwordHasher,
    IJwtTokenGenerator jwtTokenGenerator,
    IDateTimeProvider dateTimeProvider,
    IMapper mapper) : IAuthService
{
    public async Task<AuthResponseDto> RegisterAsync(RegisterUserRequest request, CancellationToken cancellationToken)
    {
        var normalizedUsername = request.Username.Trim();
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        await usuarioRepository.EnsureReferenceDataAsync(cancellationToken);

        if (await usuarioRepository.UsernameExistsAsync(normalizedUsername, cancellationToken))
        {
            throw new ConflictException("El nombre de usuario ya esta registrado.");
        }

        if (await usuarioRepository.EmailExistsAsync(normalizedEmail, cancellationToken))
        {
            throw new ConflictException("El correo electronico ya esta registrado.");
        }

        var studentRole = await usuarioRepository.GetRoleByCodeAsync("STUDENT", cancellationToken)
            ?? throw new InvalidOperationException("No se encontro el rol base STUDENT.");

        var now = dateTimeProvider.UtcNow;

        var usuario = new Usuario
        {
            Id = Guid.NewGuid(),
            Username = normalizedUsername,
            StatusCode = "active",
            PreferredLocale = string.IsNullOrWhiteSpace(request.PreferredLocale) ? "es_MX" : request.PreferredLocale.Trim(),
            CreatedAt = now,
            UpdatedAt = now
        };

        var auth = new AutenticacionUsuario
        {
            UserId = usuario.Id,
            PasswordHash = passwordHasher.HashPassword(usuario, request.Password),
            PasswordChangedAt = now,
            CreatedAt = now,
            UpdatedAt = now
        };

        var profile = new PerfilUsuario
        {
            UserId = usuario.Id,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            DisplayName = $"{request.FirstName.Trim()} {request.LastName.Trim()}".Trim(),
            CreatedAt = now,
            UpdatedAt = now
        };

        var contact = new ContactoUsuario
        {
            Id = Guid.NewGuid(),
            UserId = usuario.Id,
            Type = "email",
            Value = normalizedEmail,
            IsPrimary = true,
            IsVerified = false,
            CreatedAt = now,
            UpdatedAt = now
        };

        var userRole = new UsuarioRol
        {
            UserId = usuario.Id,
            RoleId = studentRole.Id,
            AssignedAt = now,
            Role = studentRole
        };

        usuario.Auth = auth;
        usuario.Profile = profile;
        usuario.Contacts.Add(contact);
        usuario.Roles.Add(userRole);

        var createdUser = await usuarioRepository.RegisterAsync(
            usuario,
            auth,
            profile,
            contact,
            userRole,
            cancellationToken);

        var token = jwtTokenGenerator.GenerateToken(createdUser);

        return new AuthResponseDto
        {
            AccessToken = token.AccessToken,
            ExpiresAtUtc = token.ExpiresAtUtc,
            User = mapper.Map<AuthenticatedUserDto>(createdUser)
        };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var usuario = await usuarioRepository.GetByUsernameAsync(request.Username.Trim(), cancellationToken);

        if (usuario?.Auth is null)
        {
            throw new UnauthorizedException("Credenciales invalidas.");
        }

        if (usuario.DeletedAt is not null || !string.Equals(usuario.StatusCode, "active", StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedException("El usuario no se encuentra activo.");
        }

        var now = dateTimeProvider.UtcNow;

        if (usuario.Auth.LockedUntil is not null && usuario.Auth.LockedUntil > now)
        {
            throw new UnauthorizedException("La cuenta se encuentra bloqueada temporalmente.");
        }

        var verificationResult = passwordHasher.VerifyHashedPassword(usuario, usuario.Auth.PasswordHash, request.Password);

        if (verificationResult == PasswordVerificationResult.Failed)
        {
            usuario.Auth.FailedLoginCount += 1;

            if (usuario.Auth.FailedLoginCount >= 5)
            {
                usuario.Auth.LockedUntil = now.AddMinutes(15);
                usuario.Auth.FailedLoginCount = 0;
            }

            usuario.Auth.UpdatedAt = now;
            await usuarioRepository.UpdateAuthAsync(usuario.Auth, cancellationToken);

            throw new UnauthorizedException("Credenciales invalidas.");
        }

        if (verificationResult == PasswordVerificationResult.SuccessRehashNeeded)
        {
            usuario.Auth.PasswordHash = passwordHasher.HashPassword(usuario, request.Password);
        }

        usuario.Auth.FailedLoginCount = 0;
        usuario.Auth.LockedUntil = null;
        usuario.Auth.UpdatedAt = now;

        await usuarioRepository.UpdateAuthAsync(usuario.Auth, cancellationToken);

        var token = jwtTokenGenerator.GenerateToken(usuario);

        return new AuthResponseDto
        {
            AccessToken = token.AccessToken,
            ExpiresAtUtc = token.ExpiresAtUtc,
            User = mapper.Map<AuthenticatedUserDto>(usuario)
        };
    }
}
