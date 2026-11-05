using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiSchoolSingleDB;

public partial class TpreRegisterDocument
{
    public int PreRegisterDocumentId { get; set; }

    public int? PreRegisterId { get; set; }

    public int? DocumentId { get; set; }

    public int? Type { get; set; }

    public int? Vfiid { get; set; }

    public string? FileName { get; set; }

    public string? ContentType { get; set; }

    public string? FilePath { get; set; }

    public int? SchoolId { get; set; }

    public DateTime? UpdateDate { get; set; }

    public bool IsSkipped { get; set; }
}
