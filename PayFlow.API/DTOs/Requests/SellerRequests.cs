using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PayFlow.API.DTOs.Requests;

public class CreateSellerRequest
{
    public string Name { get; set; } = string.Empty;
    public string StoreName { get; set; } = string.Empty;
    public string? PixKey { get; set; }
    public string? Email { get; set; }
    public string? CpfCnpj { get; set; }
    public string? MobilePhone { get; set; }
    public string? Phone { get; set; }
    public decimal? IncomeValue { get; set; }
    public string? CompanyType { get; set; }
    public string? Address { get; set; }
    public string? AddressNumber { get; set; }
    public string? Complement { get; set; }
    public string? Province { get; set; }
    public string? PostalCode { get; set; }
    public string? Site { get; set; }
}

public class UpdateSellerRequest
{
    public string Name { get; set; } = string.Empty;
    public string StoreName { get; set; } = string.Empty;
    public string? PixKey { get; set; }
    public string? Email { get; set; }
    public string? CpfCnpj { get; set; }
    public string? MobilePhone { get; set; }
    public string? Phone { get; set; }
    public decimal? IncomeValue { get; set; }
    public string? CompanyType { get; set; }
    public string? Address { get; set; }
    public string? AddressNumber { get; set; }
    public string? Complement { get; set; }
    public string? Province { get; set; }
    public string? PostalCode { get; set; }
    public string? Site { get; set; }
}

public class UpdateSellerAsaasSubaccountRequest : IValidatableObject
{
    [Required(ErrorMessage = "O email do vendedor é obrigatório")]
    [EmailAddress(ErrorMessage = "Informe um email válido")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "O CNPJ é obrigatório")]
    [StringLength(18, MinimumLength = 14, ErrorMessage = "Informe um CNPJ válido")]
    [RegularExpression(@"^\D*\d{2}\D*\d{3}\D*\d{3}\D*\d{4}\D*\d{2}\D*$", ErrorMessage = "Informe um CNPJ válido")]
    public string CpfCnpj { get; set; } = string.Empty;

    [Required(ErrorMessage = "O celular é obrigatório")]
    [RegularExpression(@"^\D*\d{2}\D*\d{4,5}\D*\d{4}\D*$", ErrorMessage = "Informe um celular válido")]
    public string MobilePhone { get; set; } = string.Empty;

    [Required(ErrorMessage = "O faturamento/renda é obrigatório")]
    [Range(1, 999999999, ErrorMessage = "Informe um faturamento/renda maior que zero")]
    public decimal? IncomeValue { get; set; }

    [Required(ErrorMessage = "O tipo de empresa é obrigatório")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public AsaasCompanyType? CompanyType { get; set; }

    [Required(ErrorMessage = "O endereço é obrigatório")]
    [StringLength(120, ErrorMessage = "O endereço deve ter no máximo 120 caracteres")]
    public string Address { get; set; } = string.Empty;

    [Required(ErrorMessage = "O número é obrigatório")]
    [StringLength(20, ErrorMessage = "O número deve ter no máximo 20 caracteres")]
    public string AddressNumber { get; set; } = string.Empty;

    [StringLength(80, ErrorMessage = "O complemento deve ter no máximo 80 caracteres")]
    public string? Complement { get; set; }

    [Required(ErrorMessage = "O bairro é obrigatório")]
    [StringLength(80, ErrorMessage = "O bairro deve ter no máximo 80 caracteres")]
    public string Province { get; set; } = string.Empty;

    [Required(ErrorMessage = "O CEP é obrigatório")]
    [RegularExpression(@"^\D*\d{5}\D*\d{3}\D*$", ErrorMessage = "Informe um CEP válido")]
    public string PostalCode { get; set; } = string.Empty;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!IsValidCnpj(CpfCnpj))
        {
            yield return new ValidationResult(
                "Informe um CNPJ válido",
                new[] { nameof(CpfCnpj) });
        }
    }

    private static bool IsValidCnpj(string? value)
    {
        var digits = OnlyDigits(value);
        if (digits.Length != 14 || digits.Distinct().Count() == 1)
            return false;

        var tempCnpj = digits.Substring(0, 12);
    
        // Cálculo do Primeiro Dígito
        var firstWeights = new[] { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
        var sum = 0;
        for (var i = 0; i < 12; i++)
            sum += (tempCnpj[i] - '0') * firstWeights[i];
        
        var remainder = sum % 11;
        var firstDigit = remainder < 2 ? 0 : 11 - remainder;
    
        // Concatena o primeiro dígito encontrado para calcular o segundo
        tempCnpj += firstDigit;
    
        // Cálculo do Segundo Dígito
        var secondWeights = new[] { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
        sum = 0;
        for (var i = 0; i < 13; i++)
            sum += (tempCnpj[i] - '0') * secondWeights[i];
        
        remainder = sum % 11;
        var secondDigit = remainder < 2 ? 0 : 11 - remainder;

        return digits.EndsWith($"{firstDigit}{secondDigit}");
    }

    private static int CalculateDigit(string digits, int[] weights)
    {
        var sum = 0;
        for (var index = 0; index < weights.Length; index++)
            sum += (digits[index] - '0') * weights[index];

        var remainder = sum % 11;
        return remainder < 2 ? 0 : 11 - remainder;
    }

    private static string OnlyDigits(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : new string(value.Where(char.IsDigit).ToArray());
    }
}

public enum AsaasCompanyType
{
    MEI,
    LIMITED,
    INDIVIDUAL,
    ASSOCIATION
}
