using System.ComponentModel.DataAnnotations;
using Biblioteca.Infrastructure.Auth;
using FluentAssertions;
using Xunit;

namespace Biblioteca.Tests.Tests;

// JWT secret: JwtOptions debe rechazar SecretKey con menos de 32 caracteres
public sealed class JwtSecretValidationTests
{
    [Theory]
    [InlineData("")]
    [InlineData("short")]
    [InlineData("exactly31characterslong_________")] // 32 chars — pasa
    public void JwtOptions_valida_longitud_minima_de_SecretKey(string secretKey)
    {
        var options = new JwtOptions
        {
            Issuer = "BibliotecaUdem",
            Audience = "BibliotecaUdem.Client",
            SecretKey = secretKey,
            ExpiresInMinutes = 60
        };

        var context = new ValidationContext(options);
        var results = new List<ValidationResult>();
        bool isValid = Validator.TryValidateObject(options, context, results, validateAllProperties: true);

        if (secretKey.Length < 32)
        {
            isValid.Should().BeFalse("SecretKey más corta que 32 chars debe fallar la validación");
            results.Should().Contain(r => r.MemberNames.Contains(nameof(JwtOptions.SecretKey)),
                "el error debe señalar SecretKey");
        }
        else
        {
            isValid.Should().BeTrue("SecretKey de 32+ chars debe pasar la validación");
        }
    }

    [Fact]
    public void JwtOptions_falla_cuando_SecretKey_es_nula_o_vacia()
    {
        var options = new JwtOptions
        {
            Issuer = "BibliotecaUdem",
            Audience = "BibliotecaUdem.Client",
            SecretKey = string.Empty,
            ExpiresInMinutes = 60
        };

        var context = new ValidationContext(options);
        var results = new List<ValidationResult>();
        bool isValid = Validator.TryValidateObject(options, context, results, validateAllProperties: true);

        isValid.Should().BeFalse();
        results.Should().Contain(r => r.MemberNames.Contains(nameof(JwtOptions.SecretKey)));
    }
}
