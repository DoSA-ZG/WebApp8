using System;
using System.Collections.Generic;

namespace RPPP_WebApp.Models;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Klasa koja predstavlja entitet Posao.
/// </summary>
public partial class Posao
{
    /// <summary>
    /// Jedinstveni identifikator posla.
    /// </summary>
    public int PosaoId { get; set; }

    /// <summary>
    /// Opis posla. Obavezno polje.
    /// </summary>
    [Required(ErrorMessage="Opis je obavezno polje.")]
    public string Opis { get; set; }

    /// <summary>
    /// Identifikator povezanog projekta. Mora biti veći od 0.
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Morate odabrati projekt.")]
    public int? ProjektId { get; set; }

    /// <summary>
    /// Identifikator vrste posla. Mora biti veći od 0.
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Morate odabrati vrstu posla.")]
    public int? VrstaPoslaId { get; set; }

    /// <summary>
    /// Povezani projekt.
    /// </summary>
    public virtual Projekt Projekt { get; set; }

    /// <summary>
    /// Vrsta posla.
    /// </summary>
    public virtual VrstaPosla VrstaPosla { get; set; }

    /// <summary>
    /// Kolekcija suradnika koji su uključeni u ovaj posao.
    /// </summary>
    public virtual ICollection<Suradnik> Suradniks { get; set; } = new List<Suradnik>();
}
