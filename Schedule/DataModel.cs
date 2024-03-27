// Model file, disable some unwanted code inspections: 
// ReSharper disable CollectionNeverUpdated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable ClassNeverInstantiated.Global

using sip.Experiments;
using sip.Userman;

namespace sip.Schedule;

public enum ReservationType
{
    Normal, Warning, Special
}

public record ScheduleReservation(
    DateTime Since,
    DateTime Until,

    IUserInfo ForUser,

    IUserInfo CreatedBy,
    DateTime CreatedAt,

    ReservationType Type,
    string Subject,
    string Project,
    string Subtype,
    string ResearchGroup
);



public record ScheduleInstrument(IInstrument InstrumentSubject, List<ScheduleReservation> Reservations, bool Render);

public record ScheduleData(DateTime DtDataGathered, List<ScheduleInstrument> Instruments)
{
    public IEnumerable<ScheduleInstrument> InstrumentsToRender => Instruments.Where(i => i.Render);
};