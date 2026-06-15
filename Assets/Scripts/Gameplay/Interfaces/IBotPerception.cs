using System.Collections.Generic;

public interface IBotPerception
{
    IReadOnlyList<NoteController> VisibleNotes { get; }
    IReadOnlyList<CarController> VisibleTraffic { get; }
    IReadOnlyList<CarController> VisibleBots { get; }
}