using Assets.Scripts.Models;

[System.Serializable]
public class Note
{
	public string Name;
	public string Description;
	public TraceTypes TraceType;

	public Note(string name, string description, TraceTypes traceType)
	{
		Name = name;
		Description = description;
		TraceType = traceType;
	}
}