using System;
using System.Collections.Generic;

namespace RPPP_WebApp.Models;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Klasa koja predstavlja entitet Suradnik.
/// </summary>
public partial class Suradnik
{
    /// <summary>
    /// Jedinstveni identifikator suradnika.
    /// </summary>
    public int SuradnikId { get; set; }

    /// <summary>
    /// Ime suradnika. Obavezno polje.
    /// </summary>
    [Required(ErrorMessage="Ime je obavezno.")]
    public string Ime { get; set; }

    /// <summary>
    /// Prezime suradnika. Obavezno polje.
    /// </summary>
    [Required(ErrorMessage="Prezime je obavezno polje.")]
    public string Prezime { get; set; }

    /// <summary>
    /// Email suradnika. Mora biti u ispravnom formatu.
    /// </summary>
    [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage =  "Loš format emaila.")]
    [Required(ErrorMessage="Email je obavezno polje.")]
    public string Email { get; set; }

    /// <summary>
    /// Broj mobitela suradnika. Obavezno polje.
    /// </summary>
    [Display(Name = "Broj Mobitela")]
    [Required(ErrorMessage="Broj mobitela je obavezno polje.")]
    public string BrojMobitela { get; set; }

    /// <summary>
    /// Kolekcija veza između suradnika i projekata.
    /// </summary>
    public virtual ICollection<SuradnikProjekt> SuradnikProjekts { get; set; } = new List<SuradnikProjekt>();

    /// <summary>
    /// Kolekcija veza između suradnika i zadataka.
    /// </summary>
    public virtual ICollection<ZadatakSuradnik> ZadatakSuradniks { get; set; } = new List<ZadatakSuradnik>();

    /// <summary>
    /// Kolekcija poslova na kojima suradnik sudjeluje.
    /// </summary>
    [Display(Name = "Poslovi")]
    public virtual ICollection<Posao> Posaos { get; set; } = new List<Posao>();
}
