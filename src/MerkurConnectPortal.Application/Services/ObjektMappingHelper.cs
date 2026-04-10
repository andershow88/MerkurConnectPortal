using MerkurConnectPortal.Application.DTOs;
using MerkurConnectPortal.Domain.Entities;

namespace MerkurConnectPortal.Application.Services;

internal static class ObjektMappingHelper
{
    internal static string GetStatusBezeichnung(ObjektStatus status) => status switch
    {
        ObjektStatus.InPlanung => "In Planung",
        ObjektStatus.InBau => "In Bau",
        ObjektStatus.Fertiggestellt => "Fertiggestellt",
        ObjektStatus.Abgeschlossen => "Abgeschlossen",
        _ => "Unbekannt"
    };

    internal static string GetStatusCssClass(ObjektStatus status) => status switch
    {
        ObjektStatus.InPlanung => "badge-warning",
        ObjektStatus.InBau => "badge-primary",
        ObjektStatus.Fertiggestellt => "badge-success",
        ObjektStatus.Abgeschlossen => "badge-secondary",
        _ => "badge-secondary"
    };

    internal static ObjektKurzDto ToKurzDto(Objekt o) => new()
    {
        Id = o.Id,
        Objektname = o.Objektname,
        Standort = o.Standort,
        Bautraeger = o.Bautraeger?.Name ?? string.Empty,
        Status = GetStatusBezeichnung(o.Status),
        StatusCssClass = GetStatusCssClass(o.Status),
        Unterbeteiligungsquote = o.Unterbeteiligungsquote,
        Metakontosaldo = o.Metakontosaldo,
        Kaufpreissammelkontosaldo = o.Kaufpreissammelkontosaldo,
        Avale = o.Avale,
        EinheitenGesamt = o.EinheitenGesamt,
        EinheitenVerkauft = o.EinheitenVerkauft,
        Verkaufsquote = o.Verkaufsquote,
        BautenstandProzent = o.BautenstandProzent,
        LetzteAktualisierung = o.LetzteAktualisierung
    };
}
