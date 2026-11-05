using System;
using System.Collections.Generic;

namespace ServiceSchoolBot.Infrastructure.Entities.JabjaiMasterSingleDB;

public partial class Favorite
{
    public int Favoritesid { get; set; }

    public int? Tabletid { get; set; }

    public int? Productid { get; set; }
}
