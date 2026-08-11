using System.Text.Json.Serialization;

namespace OxfordOnline.Models.Dto
{
    public class SeniorEmployeeResponse
    {
        public int TotalPages { get; set; }
        public int TotalElements { get; set; }

        public List<SeniorEmployeeData> Contents { get; set; } = new();
    }

    public class SeniorEmployeeData
    {
        [JsonPropertyName("registerNumber")]
        public int Registration { get; set; }

        public SeniorPerson? Person { get; set; }

        public SeniorJobPosition? JobPosition { get; set; }

        public SeniorDepartment? Department { get; set; }

        public SeniorEmployer? Employer { get; set; }

        public SeniorWorkShift? WorkShift { get; set; }

        [JsonPropertyName("phoneContact")]
        public List<SeniorPhoneContact> PhoneContact { get; set; } = new();

        public List<SeniorEmail> Emails { get; set; } = new();

        [JsonIgnore]
        public string BadgeCode { get; set; } = string.Empty;

        [JsonIgnore]
        public bool Active { get; set; }

        [JsonIgnore]
        public string Name =>
            Person?.FullName ?? string.Empty;

        [JsonIgnore]
        public string Position =>
            JobPosition?.Name ?? string.Empty;

        [JsonIgnore]
        public string DepartmentName =>
            Department?.Name ?? string.Empty;

        [JsonIgnore]
        public string Email =>
            Emails.FirstOrDefault()?.Email ?? string.Empty;

        [JsonIgnore]
        public SeniorHeadquarter? Headquarter =>
            Employer?.Headquarter;

        [JsonIgnore]
        public string Phone =>
            PhoneContact.FirstOrDefault()?.Number ?? string.Empty;

        [JsonIgnore]
        public string SeniorWorkShift =>
            WorkShift?.Name ?? string.Empty;

        public SeniorCostCenter? CostCenter { get; set; }
    }

    public class SeniorPerson
    {
        [JsonPropertyName("fullName")]
        public string FullName { get; set; } = string.Empty;
    }

    public class SeniorJobPosition
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Codcar { get; set; } = string.Empty;
    }

    public class SeniorDepartment
    {
        public string Id { get; set; } = string.Empty;
        public int TableCode { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

    public class SeniorEmployer
    {
        public string Id { get; set; } = string.Empty;
        public int Numemp { get; set; }
        public string TradingName { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string Cnpj { get; set; } = string.Empty;
        public string CompanyType { get; set; } = string.Empty;
        public string Cnae { get; set; } = string.Empty;

        public SeniorHeadquarter? Headquarter { get; set; }
    }

    public class SeniorHeadquarter
    {
        public string Id { get; set; } = string.Empty;
        public int Numemp { get; set; }
        public string TradingName { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string Cnpj { get; set; } = string.Empty;
    }

    public class SeniorWorkShift
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int Codesc { get; set; }
        public int Workload { get; set; }
    }

    public class SeniorCostCenter
    {
        public string Id { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("codccu")]
        public string Codccu { get; set; } = string.Empty;
    }

    public class SeniorPhoneContact
    {
        public string Id { get; set; } = string.Empty;
        public int CountryCode { get; set; }
        public int LocalCode { get; set; }
        public string Number { get; set; } = string.Empty;
        public string PhoneContactType { get; set; } = string.Empty;
    }

    public class SeniorEmail
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}