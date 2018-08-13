namespace Evgo
{
    // scoring run types
    public enum eRunType
    {
        None,
        Strike,
        Foul,
        Dud,
        Single,
        Double,
        Triple,
        HomerunIn,
        HomerunOut
    }

    // ball landing score zones
    public enum eScoreZone
    {
        None,
        Foul,
        PotentialSingle,
        PotentialDouble,
        PotentialTriple,
        PotentialHomerun,
        HomerunIn,
        HomerunOut
    }

    // score zone kill ball types
    public enum eBallKillType
    {
        None,
        Delayed,
        Immediate
    }

    // game mode states
    public enum eGamePlayState
    {
        Idle,
        Live,
        Hit,
        Resolve,
    }
}