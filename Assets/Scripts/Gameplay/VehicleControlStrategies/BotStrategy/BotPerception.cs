using System.Collections.Generic;

public class BotPerception : IBotPerception
{
    TrafficSpawner trafficSpawner;
    NotesSpawner notesSpawner;
    CompetitorSpawner competitorSpawner;

    public BotPerception(TrafficSpawner trafficSpawner, NotesSpawner notesSpawner, CompetitorSpawner competitorSpawner)
    {
        this.trafficSpawner = trafficSpawner;
        this.notesSpawner = notesSpawner;
        this.competitorSpawner = competitorSpawner;
    }

    public IReadOnlyList<NoteController> VisibleNotes => notesSpawner.ActiveInstances;
    public IReadOnlyList<CarController> VisibleTraffic => trafficSpawner.ActiveInstances;

    public IReadOnlyList<CarController> VisibleBots => competitorSpawner.AllActiveCars;
}